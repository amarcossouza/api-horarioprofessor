using System.Text.Json;
using API_HorarioProfessor.Universidade.Contratos;
using Dapper;
using MySqlConnector;

namespace API_HorarioProfessor.Universidade.Servicos;

/// <summary>
/// Grava um envio validado. Sempre roda dentro da transação recebida:
/// na prévia a transação é desfeita no fim; na aprovação ela é confirmada.
/// Só INSERE registros nas tabelas atuais (escola, turmas, matérias, cadastro do professor,
/// opções) e só ATUALIZA registros que o próprio módulo criou. Nada é apagado.
/// Cada gravação fica em hprof_uni_integracaoalteracao com o valor de antes e depois.
/// </summary>
public sealed class IntegracaoAplicador
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    // ============================== PESSOAS ==============================

    public async Task<PreviaDto> AplicarPessoasAsync(Gravacao g, LotePessoasConteudo lote)
    {
        var usados = (await g.Cn.QueryAsync<string>(
            "SELECT nome_curto FROM hprof_uni_pessoaprofessor WHERE id_instituicao = @i", new { i = g.IdInstituicao }, g.Tx))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var p in lote.Professores)
        {
            var id = await g.MapaAsync("PESSOA", p.Codigo)
                     ?? await g.Cn.ExecuteScalarAsync<long?>(
                         "SELECT id FROM hprof_uni_pessoaprofessor WHERE id_instituicao = @i AND documento = @d",
                         new { i = g.IdInstituicao, d = p.Documento.Trim() }, g.Tx);
            var ativo = p.Ativo ?? true;
            if (id is null)
            {
                var nomeCurto = NomesUniversidade.Unico(NomesUniversidade.NomeCurto(p.Nome), usados, 40);
                var novo = await g.Cn.ExecuteScalarAsync<long>(
                    """
                    INSERT INTO hprof_uni_pessoaprofessor (id_instituicao, nome, nome_curto, documento, carga_maxima, ativo, criado_por)
                    VALUES (@i, @nome, @curto, @doc, @carga, @ativo, @u);
                    SELECT LAST_INSERT_ID();
                    """, new { i = g.IdInstituicao, nome = p.Nome.Trim(), curto = nomeCurto, doc = p.Documento.Trim(), carga = p.CargaMaxima, ativo, u = g.Usuario }, g.Tx);
                await g.RegistrarAsync("Professores (pessoas)", "hprof_uni_pessoaprofessor", "INSERIR", novo, null,
                    new { p.Nome, NomeCurto = nomeCurto, p.Documento, p.CargaMaxima, Ativo = ativo });
                id = novo;
            }
            else
            {
                var atual = await g.Cn.QuerySingleAsync<PessoaDto>(
                    "SELECT id AS Id, nome AS Nome, nome_curto AS NomeCurto, documento AS Documento, carga_maxima AS CargaMaxima, ativo AS Ativo FROM hprof_uni_pessoaprofessor WHERE id = @id",
                    new { id }, g.Tx);
                await AcertarNomeCurtoAsync(g, id.Value, atual.NomeCurto, usados);
                if (atual.Nome != p.Nome.Trim() || atual.CargaMaxima != p.CargaMaxima || atual.Ativo != ativo || atual.Documento != p.Documento.Trim())
                {
                    await g.Cn.ExecuteAsync(
                        "UPDATE hprof_uni_pessoaprofessor SET nome = @nome, documento = @doc, carga_maxima = @carga, ativo = @ativo, atualizado_por = @u WHERE id = @id",
                        new { id, nome = p.Nome.Trim(), doc = p.Documento.Trim(), carga = p.CargaMaxima, ativo, u = g.Usuario }, g.Tx);
                    await g.RegistrarAsync("Professores (pessoas)", "hprof_uni_pessoaprofessor", "ATUALIZAR", id.Value, atual,
                        new { p.Nome, p.Documento, p.CargaMaxima, Ativo = ativo });
                }
                else g.SemMudanca("Professores (pessoas)", "hprof_uni_pessoaprofessor");
            }
            if (!string.IsNullOrWhiteSpace(p.Codigo)) await g.GravarMapaAsync("PESSOA", p.Codigo, "hprof_uni_pessoaprofessor", id.Value);
        }

        foreach (var v in lote.Vinculos)
        {
            var (idPessoa, _, _) = await IntegracaoValidador.ResolverPessoaComTransacaoAsync(g, v.Professor);
            var idEscola = await g.Cn.ExecuteScalarAsync<int?>(
                "SELECT id_escola FROM hprof_uni_curso WHERE id_instituicao = @i AND codigo = @c", new { i = g.IdInstituicao, c = v.Curso.Trim() }, g.Tx);
            if (idPessoa == 0 || idEscola is null) continue;
            await GravarVinculoAsync(g, idPessoa, idEscola.Value, lote.AnoLetivo, lote.Periodo, v.AulasReservadas, v.Ordem, substituir: true);
        }
        return g.Previa();
    }

    // ============================== CURSO ==============================

    public async Task<PreviaDto> AplicarCursoAsync(Gravacao g, LoteCursoConteudo lote)
    {
        var ano = lote.AnoLetivo;
        var periodo = lote.Periodo.Trim();
        // códigos de turma, disciplina e atribuição valem dentro do curso (dois cursos podem usar o mesmo código)
        string C(string codigo) => ChaveDoCurso(lote.Curso.Codigo, codigo);
        var inst = await g.Cn.QuerySingleAsync<InstituicaoDto>(
            "SELECT id AS Id, sigla AS Sigla, nome AS Nome, id_escola_principal AS IdEscolaPrincipal FROM hprof_uni_instituicao WHERE id = @i",
            new { i = g.IdInstituicao }, g.Tx);

        // ---------- 1. curso = escola ----------
        var horarios = lote.HorariosAula.OrderBy(h => h.Aula)
            .Select(h => { NomesUniversidade.TentarHora(h.Inicio, out var i); NomesUniversidade.TentarHora(h.Fim, out var f); return (i, f); })
            .ToList();
        var dias = lote.Curso.Dias.Select(NomesUniversidade.DiaDoTexto).Where(d => d > 0).Distinct().OrderBy(d => d)
            .Select(NomesUniversidade.Dia3).ToList();
        if (dias.Count == 0) dias = new List<string> { "SEG", "TER", "QUA", "QUI", "SEX" };
        var intervalosJson = JsonSerializer.Serialize(lote.Intervalos, Json);

        var curso = await g.Cn.QuerySingleOrDefaultAsync<CursoDto>(
            "SELECT id AS Id, id_escola AS IdEscola, codigo AS Codigo, nome AS Nome, turno AS Turno, dias AS Dias, intervalos_json AS IntervalosJson, ordem AS Ordem FROM hprof_uni_curso WHERE id_instituicao = @i AND codigo = @c",
            new { i = g.IdInstituicao, c = lote.Curso.Codigo.Trim() }, g.Tx);

        if (curso is null)
        {
            var tamanhoNomeEscola = Math.Min(100, await g.TamanhoColunaAsync("hprof_escolas", "Nome", 100));
            var principalExiste = await g.Cn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM hprof_escolas WHERE EscolaId = @e", new { e = inst.IdEscolaPrincipal }, g.Tx);
            if (principalExiste == 0)
                throw new InvalidOperationException($"A escola da instituição (EscolaId {inst.IdEscolaPrincipal}) não existe; o curso não pode ser criado.");
            var idEscola = await g.Cn.ExecuteScalarAsync<int>(
                """
                INSERT INTO hprof_escolas (CNPJ, Nome, Endereco, Bairro, Estado, Municipio, Diretor, ViceDiretor, Coordenador,
                                           Contato, Telefone, Celular, Email, QuantidadeTurmas, QuantidadeTurnos, TipoEscola,
                                           UsuarioAcesso, Senha, QuantidadeHorarios, QuantidadeHorariosManha,
                                           QuantidadeHorariosTarde, QuantidadeHorariosNoite)
                SELECT CNPJ, @nome, Endereco, Bairro, Estado, Municipio, Diretor, ViceDiretor, Coordenador,
                       Contato, Telefone, Celular, Email, @turmas, 1, 'Universidade', '', '', @qtd, @qtd, 0, 0
                  FROM hprof_escolas WHERE EscolaId = @principal;
                SELECT LAST_INSERT_ID();
                """, new { nome = Cortar($"{lote.Curso.Nome.Trim()} · {inst.Sigla}", tamanhoNomeEscola), turmas = lote.Turmas.Count, qtd = horarios.Count, principal = inst.IdEscolaPrincipal }, g.Tx);
            if (idEscola <= 0) throw new InvalidOperationException("Não foi possível criar a escola do curso (escola da instituição não encontrada).");
            await g.RegistrarAsync("Curso (escola)", "hprof_escolas", "INSERIR", idEscola, null,
                new { Nome = $"{lote.Curso.Nome.Trim()} · {inst.Sigla}", TipoEscola = "Universidade", QuantidadeHorarios = horarios.Count });

            var ordem = 1 + await g.Cn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM hprof_uni_curso WHERE id_instituicao = @i", new { i = g.IdInstituicao }, g.Tx);
            var idCurso = await g.Cn.ExecuteScalarAsync<long>(
                """
                INSERT INTO hprof_uni_curso (id_instituicao, id_escola, codigo, nome, turno, dias, intervalos_json, ordem, criado_por)
                VALUES (@i, @e, @c, @n, @t, @d, @j, @o, @u);
                SELECT LAST_INSERT_ID();
                """, new { i = g.IdInstituicao, e = idEscola, c = lote.Curso.Codigo.Trim(), n = lote.Curso.Nome.Trim(), t = lote.Curso.Turno, d = string.Join(",", dias), j = intervalosJson, o = ordem, u = g.Usuario }, g.Tx);
            await g.RegistrarAsync("Curso (escola)", "hprof_uni_curso", "INSERIR", idCurso, null,
                new { lote.Curso.Codigo, lote.Curso.Nome, lote.Curso.Turno, Dias = dias });
            await g.GravarMapaAsync("CURSO", lote.Curso.Codigo, "hprof_uni_curso", idCurso);
            curso = new CursoDto { Id = idCurso, IdEscola = idEscola, Codigo = lote.Curso.Codigo.Trim(), Nome = lote.Curso.Nome.Trim(), Ordem = ordem };
        }
        else if (!lote.Parcial)
        {
            var novoDias = string.Join(",", dias);
            var intervalosAtuais = string.IsNullOrWhiteSpace(curso.IntervalosJson) ? "[]"
                : JsonSerializer.Serialize(JsonSerializer.Deserialize<List<LoteIntervalo>>(curso.IntervalosJson, Json), Json);
            if (curso.Nome != lote.Curso.Nome.Trim() || curso.Turno != lote.Curso.Turno || curso.Dias != novoDias || intervalosAtuais != intervalosJson)
            {
                await g.Cn.ExecuteAsync(
                    "UPDATE hprof_uni_curso SET nome = @n, turno = @t, dias = @d, intervalos_json = @j, atualizado_por = @u WHERE id = @id",
                    new { id = curso.Id, n = lote.Curso.Nome.Trim(), t = lote.Curso.Turno, d = novoDias, j = intervalosJson, u = g.Usuario }, g.Tx);
                await g.RegistrarAsync("Curso (escola)", "hprof_uni_curso", "ATUALIZAR", curso.Id,
                    new { curso.Nome, curso.Turno, curso.Dias }, new { lote.Curso.Nome, lote.Curso.Turno, Dias = novoDias });
            }
            else g.SemMudanca("Curso (escola)", "hprof_uni_curso");

            var qtdAtual = await g.Cn.ExecuteScalarAsync<int>("SELECT QuantidadeHorarios FROM hprof_escolas WHERE EscolaId = @e", new { e = curso.IdEscola }, g.Tx);
            if (qtdAtual != horarios.Count)
            {
                await g.Cn.ExecuteAsync(
                    "UPDATE hprof_escolas SET QuantidadeHorarios = @q, QuantidadeHorariosManha = @q WHERE EscolaId = @e",
                    new { q = horarios.Count, e = curso.IdEscola }, g.Tx);
                await g.RegistrarAsync("Curso (escola)", "hprof_escolas", "ATUALIZAR", curso.IdEscola,
                    new { QuantidadeHorarios = qtdAtual }, new { QuantidadeHorarios = horarios.Count });
            }
        }
        else g.SemMudanca("Curso (escola)", "hprof_uni_curso");

        var escola = curso.IdEscola;

        // ---------- 2. horários do relógio ----------
        if (!lote.Parcial && horarios.Count > 0)
        {
            var textoHorarios = NomesUniversidade.FormatarHorariosConfig(horarios);
            var tamanhoHorarios = await g.TamanhoColunaAsync("hprof_gradehorariosconfig", "Horarios", int.MaxValue);
            if (textoHorarios.Length > tamanhoHorarios)
                throw new InvalidOperationException($"Os {horarios.Count} horários do curso ocupam {textoHorarios.Length} caracteres e a coluna Horarios de hprof_gradehorariosconfig aceita {tamanhoHorarios}.");
            var atual = await g.Cn.QueryFirstOrDefaultAsync<string>(
                "SELECT Horarios FROM hprof_gradehorariosconfig WHERE IdEscola = @e AND Periodo = @p AND AnoLetivo = @a LIMIT 1",
                new { e = escola, p = periodo, a = ano }, g.Tx);
            const string cabecalhos = "Horário,Segunda-feira,Terça-feira,Quarta-feira,Quinta-feira,Sexta-feira";
            if (atual is null)
            {
                await g.Cn.ExecuteAsync(
                    """
                    INSERT INTO hprof_gradehorariosconfig (IdEscola, Periodo, AnoLetivo, Cabecalhos, QuantidadeCabecalhos, Horarios, QuantidadeHorarios, DataCadastro)
                    VALUES (@e, @p, @a, @c, 6, @h, @q, NOW())
                    """, new { e = escola, p = periodo, a = ano, c = cabecalhos, h = textoHorarios, q = horarios.Count }, g.Tx);
                await g.RegistrarAsync("Horários das aulas", "hprof_gradehorariosconfig", "INSERIR", escola, null, new { Horarios = textoHorarios });
            }
            else if (atual != textoHorarios)
            {
                await g.Cn.ExecuteAsync(
                    "UPDATE hprof_gradehorariosconfig SET Horarios = @h, QuantidadeHorarios = @q WHERE IdEscola = @e AND Periodo = @p AND AnoLetivo = @a",
                    new { e = escola, p = periodo, a = ano, h = textoHorarios, q = horarios.Count }, g.Tx);
                await g.RegistrarAsync("Horários das aulas", "hprof_gradehorariosconfig", "ATUALIZAR", escola, new { Horarios = atual }, new { Horarios = textoHorarios });
            }
            else g.SemMudanca("Horários das aulas", "hprof_gradehorariosconfig");
        }

        // ---------- 3. turmas ----------
        // as siglas geradas cabem no tamanho real das colunas antigas (lido do banco, sem alterar nada)
        var tamanhoTurma = Math.Min(20, Math.Min(
            await g.TamanhoColunaAsync("hprof_turmapadraoescola", "Turmas", 20),
            await g.TamanhoColunaAsync("hprof_cadastroprofessor", "TurmasAtribuidas", 20)));
        var siglasTurma = (await g.Cn.QueryAsync<string>("SELECT sigla FROM hprof_uni_turma WHERE id_curso = @c", new { c = curso.Id }, g.Tx))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var turmaPorCodigo = new Dictionary<string, (long Id, string Sigla)>(StringComparer.OrdinalIgnoreCase);
        var ordemTurma = 0;
        foreach (var t in lote.Turmas)
        {
            ordemTurma++;
            var idUni = await g.MapaAsync("TURMA", C(t.Codigo));
            if (idUni is null)
            {
                var sigla = NomesUniversidade.Unico(NomesUniversidade.SiglaTurma(t.Nome), siglasTurma, tamanhoTurma);
                var idTurma = await g.Cn.ExecuteScalarAsync<int>(
                    """
                    INSERT INTO hprof_turmapadraoescola (IdEscola, Periodo, AnoLetivo, Turmas) VALUES (@e, @p, @a, @s);
                    SELECT LAST_INSERT_ID();
                    """, new { e = escola, p = periodo, a = ano, s = sigla }, g.Tx);
                await g.RegistrarAsync("Turmas (fases)", "hprof_turmapadraoescola", "INSERIR", idTurma, null, new { Turmas = sigla });
                var novo = await g.Cn.ExecuteScalarAsync<long>(
                    """
                    INSERT INTO hprof_uni_turma (id_curso, id_turmapadrao, codigo, sigla, nome, ordem, criado_por)
                    VALUES (@c, @tp, @cod, @s, @n, @o, @u);
                    SELECT LAST_INSERT_ID();
                    """, new { c = curso.Id, tp = idTurma, cod = t.Codigo.Trim(), s = sigla, n = t.Nome.Trim(), o = ordemTurma, u = g.Usuario }, g.Tx);
                await g.RegistrarAsync("Turmas (fases)", "hprof_uni_turma", "INSERIR", novo, null, new { t.Codigo, t.Nome, Sigla = sigla });
                await g.GravarMapaAsync("TURMA", C(t.Codigo), "hprof_uni_turma", novo);
                turmaPorCodigo[t.Codigo.Trim()] = (novo, sigla);
            }
            else
            {
                var atual = await g.Cn.QuerySingleAsync<TurmaUniversidadeDto>(
                    "SELECT sigla AS Sigla, nome AS Nome, ordem AS Ordem FROM hprof_uni_turma WHERE id = @id", new { id = idUni }, g.Tx);
                if (atual.Nome != t.Nome.Trim() || atual.Ordem != ordemTurma)
                {
                    await g.Cn.ExecuteAsync("UPDATE hprof_uni_turma SET nome = @n, ordem = @o, ativo = 1, atualizado_por = @u WHERE id = @id",
                        new { id = idUni, n = t.Nome.Trim(), o = ordemTurma, u = g.Usuario }, g.Tx);
                    await g.RegistrarAsync("Turmas (fases)", "hprof_uni_turma", "ATUALIZAR", idUni.Value, atual, new { t.Nome, Ordem = ordemTurma });
                }
                else g.SemMudanca("Turmas (fases)", "hprof_uni_turma");
                turmaPorCodigo[t.Codigo.Trim()] = (idUni.Value, atual.Sigla);
            }
        }

        async Task<(long Id, string Sigla)?> TurmaAsync(string codigo)
        {
            if (turmaPorCodigo.TryGetValue(codigo.Trim(), out var t)) return t;
            var id = await g.MapaAsync("TURMA", C(codigo));
            if (id is null) return null;
            var sigla = await g.Cn.ExecuteScalarAsync<string>("SELECT sigla FROM hprof_uni_turma WHERE id = @id", new { id }, g.Tx);
            turmaPorCodigo[codigo.Trim()] = (id.Value, sigla ?? "");
            return turmaPorCodigo[codigo.Trim()];
        }

        // ---------- 4. disciplinas (matérias) ----------
        var siglasMateria = (await g.Cn.QueryAsync<string>(
            "SELECT Sigla FROM hprof_materiaaulasporano WHERE EscolaId = @e AND AnoLetivo = @a AND Periodo = @p",
            new { e = escola, a = ano, p = periodo }, g.Tx)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var tamanhoMateria = Math.Min(8, Math.Min(
            await g.TamanhoColunaAsync("hprof_materiaaulasporano", "Sigla", 8),
            await g.TamanhoColunaAsync("hprof_cadastroprofessor", "Materia", 8)));
        // o nome da disciplina é cortado no tamanho da coluna Descricao; a comparação usa o nome já cortado
        var tamanhoDescricao = await g.TamanhoColunaAsync("hprof_materiaaulasporano", "Descricao", 100);
        var disciplinaPorCodigo = new Dictionary<string, DisciplinaGravada>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in lote.Disciplinas)
        {
            var turma = await TurmaAsync(d.Turma);
            if (turma is null) continue;
            var nomeMateria = Cortar(d.Nome.Trim(), tamanhoDescricao);
            var idMateria = await g.MapaAsync("DISCIPLINA", C(d.Codigo));
            if (idMateria is null)
            {
                var sigla = NomesUniversidade.Unico(NomesUniversidade.SiglaDisciplina(d.Nome, tamanhoMateria), siglasMateria, tamanhoMateria);
                var novo = await g.Cn.ExecuteScalarAsync<long>(
                    """
                    INSERT INTO hprof_materiaaulasporano (AnoLetivo, Periodo, EscolaId, DataCriacao, TipoEscola, Descricao, ModalidadeEscolar, Sigla, TodosOsAnos, QuantidadeAulas)
                    VALUES (@a, @p, @e, NOW(), 'Universidade', @n, 'SUP', @s, 0, @q);
                    SELECT LAST_INSERT_ID();
                    """, new { a = ano, p = periodo, e = escola, n = nomeMateria, s = sigla, q = d.AulasSemana }, g.Tx);
                await g.RegistrarAsync("Disciplinas", "hprof_materiaaulasporano", "INSERIR", novo, null, new { Nome = nomeMateria, Sigla = sigla, d.AulasSemana });
                await g.GravarMapaAsync("DISCIPLINA", C(d.Codigo), "hprof_materiaaulasporano", novo);
                disciplinaPorCodigo[d.Codigo.Trim()] = new DisciplinaGravada(novo, sigla, nomeMateria, turma.Value.Sigla);
            }
            else
            {
                var atual = await g.Cn.QuerySingleAsync<MateriaAtual>(
                    "SELECT Sigla, Descricao, QuantidadeAulas, ModalidadeEscolar FROM hprof_materiaaulasporano WHERE Id = @id", new { id = idMateria }, g.Tx);
                if (atual.ModalidadeEscolar != "SUP")
                {
                    // as telas antigas filtram as matérias pela sigla da modalidade
                    await g.Cn.ExecuteAsync("UPDATE hprof_materiaaulasporano SET ModalidadeEscolar = 'SUP' WHERE Id = @id", new { id = idMateria }, g.Tx);
                    await g.RegistrarAsync("Disciplinas", "hprof_materiaaulasporano", "ATUALIZAR", idMateria.Value,
                        new { atual.ModalidadeEscolar }, new { ModalidadeEscolar = "SUP" });
                }
                // envio parcial (tela/planilha) não muda o total de aulas de uma disciplina que já existe
                if (!lote.Parcial && (atual.Descricao != nomeMateria || atual.QuantidadeAulas != d.AulasSemana))
                {
                    await g.Cn.ExecuteAsync("UPDATE hprof_materiaaulasporano SET Descricao = @n, QuantidadeAulas = @q WHERE Id = @id",
                        new { id = idMateria, n = nomeMateria, q = d.AulasSemana }, g.Tx);
                    await g.RegistrarAsync("Disciplinas", "hprof_materiaaulasporano", "ATUALIZAR", idMateria.Value, atual, new { Nome = nomeMateria, d.AulasSemana });
                }
                else g.SemMudanca("Disciplinas", "hprof_materiaaulasporano");
                disciplinaPorCodigo[d.Codigo.Trim()] = new DisciplinaGravada(idMateria.Value, atual.Sigla, nomeMateria, turma.Value.Sigla);
            }
            await g.GravarMapaAsync("DISCIPLINA_TURMA", C(d.Codigo), "hprof_uni_turma", turma.Value.Id);
        }

        async Task<DisciplinaGravada?> DisciplinaAsync(string codigo)
        {
            if (disciplinaPorCodigo.TryGetValue(codigo.Trim(), out var d)) return d;
            var idMateria = await g.MapaAsync("DISCIPLINA", C(codigo));
            var idTurma = await g.MapaAsync("DISCIPLINA_TURMA", C(codigo));
            if (idMateria is null || idTurma is null) return null;
            var mat = await g.Cn.QuerySingleOrDefaultAsync<MateriaAtual>(
                "SELECT Sigla, Descricao, QuantidadeAulas FROM hprof_materiaaulasporano WHERE Id = @id", new { id = idMateria }, g.Tx);
            var siglaTurma = await g.Cn.ExecuteScalarAsync<string>("SELECT sigla FROM hprof_uni_turma WHERE id = @id", new { id = idTurma }, g.Tx);
            if (mat is null || siglaTurma is null) return null;
            var nova = new DisciplinaGravada(idMateria.Value, mat.Sigla, mat.Descricao, siglaTurma);
            disciplinaPorCodigo[codigo.Trim()] = nova;
            return nova;
        }

        // ---------- 5. atribuições (cadastro do professor do motor) ----------
        var pessoasTocadas = new HashSet<long>();
        var codigosNoLote = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var nomesUsados = (await g.Cn.QueryAsync<string>(
            "SELECT nome_curto FROM hprof_uni_pessoaprofessor WHERE id_instituicao = @i", new { i = g.IdInstituicao }, g.Tx))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var curtoPorPessoa = new Dictionary<long, string>();
        foreach (var a in lote.Atribuicoes)
        {
            codigosNoLote.Add(C(a.Codigo));
            var disc = await DisciplinaAsync(a.Disciplina);
            if (disc is null) continue;
            var pessoasDaAula = new List<(long Id, string NomeCurto)>();
            foreach (var referencia in a.Professores.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var (idPessoa, _, _) = await IntegracaoValidador.ResolverPessoaComTransacaoAsync(g, referencia);
                if (idPessoa == 0) continue;
                if (!curtoPorPessoa.TryGetValue(idPessoa, out var curto))
                {
                    var guardado = await g.Cn.ExecuteScalarAsync<string>("SELECT nome_curto FROM hprof_uni_pessoaprofessor WHERE id = @id", new { id = idPessoa }, g.Tx) ?? "";
                    curto = await AcertarNomeCurtoAsync(g, idPessoa, guardado, nomesUsados);
                    curtoPorPessoa[idPessoa] = curto;
                }
                pessoasDaAula.Add((idPessoa, curto));
            }
            if (pessoasDaAula.Count == 0) continue;
            var principal = pessoasDaAula[0];
            var nomeMotor = $"{principal.NomeCurto}-{disc.Sigla}";

            var idCadastro = await g.MapaAsync("ATRIBUICAO", C(a.Codigo));
            CadastroAtual? atual = null;
            if (idCadastro is not null)
                atual = await g.Cn.QuerySingleOrDefaultAsync<CadastroAtual>(
                    "SELECT NomeProfessor, TurmasAtribuidas, Materia, QuantidadeAula, fk_MateriaAulasPorAno AS FkMateria, Modalidade FROM hprof_cadastroprofessor WHERE ProfessorId = @id AND IdEscola = @e",
                    new { id = idCadastro, e = escola }, g.Tx);
            if (idCadastro is null || atual is null)
            {
                var novo = await g.Cn.ExecuteScalarAsync<int>(
                    """
                    INSERT INTO hprof_cadastroprofessor (IdEscola, NomeProfessor, TurmasAtribuidas, Materia, QuantidadeAula, AnoLetivo, Periodo, fk_MateriaAulasPorAno, Modalidade)
                    VALUES (@e, @nome, @turma, @materia, @q, @a, @p, @fk, 'SUP');
                    SELECT LAST_INSERT_ID();
                    """, new { e = escola, nome = nomeMotor, turma = disc.SiglaTurma, materia = disc.Sigla, q = a.AulasSemana, a = ano, p = periodo, fk = disc.Id }, g.Tx);
                await g.RegistrarAsync("Atribuições (cadastro do professor)", "hprof_cadastroprofessor", "INSERIR", novo, null,
                    new { NomeProfessor = nomeMotor, TurmasAtribuidas = disc.SiglaTurma, Materia = disc.Sigla, QuantidadeAula = a.AulasSemana, fk_MateriaAulasPorAno = disc.Id, Modalidade = "SUP" });
                await g.GravarMapaAsync("ATRIBUICAO", C(a.Codigo), "hprof_cadastroprofessor", novo);
                idCadastro = novo;
            }
            else
            {
                await g.GravarMapaAsync("ATRIBUICAO", C(a.Codigo), "hprof_cadastroprofessor", idCadastro.Value); // reativa se tinha saído num reenvio
                if (atual.NomeProfessor != nomeMotor || atual.TurmasAtribuidas != disc.SiglaTurma || atual.Materia != disc.Sigla || atual.QuantidadeAula != a.AulasSemana
                    || atual.FkMateria != disc.Id || atual.Modalidade != "SUP")
                {
                    await g.Cn.ExecuteAsync(
                        "UPDATE hprof_cadastroprofessor SET NomeProfessor = @nome, TurmasAtribuidas = @turma, Materia = @materia, QuantidadeAula = @q, fk_MateriaAulasPorAno = @fk, Modalidade = 'SUP' WHERE ProfessorId = @id",
                        new { id = idCadastro, nome = nomeMotor, turma = disc.SiglaTurma, materia = disc.Sigla, q = a.AulasSemana, fk = disc.Id }, g.Tx);
                    await g.RegistrarAsync("Atribuições (cadastro do professor)", "hprof_cadastroprofessor", "ATUALIZAR", idCadastro.Value, atual,
                        new { NomeProfessor = nomeMotor, TurmasAtribuidas = disc.SiglaTurma, Materia = disc.Sigla, QuantidadeAula = a.AulasSemana, fk_MateriaAulasPorAno = disc.Id, Modalidade = "SUP" });
                }
                else g.SemMudanca("Atribuições (cadastro do professor)", "hprof_cadastroprofessor");
            }

            // pessoa(s) ligadas à linha do cadastro: principal e apoio
            var ligadas = (await g.Cn.QueryAsync<LigacaoAtual>(
                "SELECT id AS Id, id_pessoa AS IdPessoa, papel AS Papel, ativo AS Ativo FROM hprof_uni_pessoacadastro WHERE id_cadastroprofessor = @c",
                new { c = idCadastro }, g.Tx)).ToList();
            for (var i = 0; i < pessoasDaAula.Count; i++)
            {
                var (idPessoa, _) = pessoasDaAula[i];
                var papel = i == 0 ? "PRINCIPAL" : "APOIO";
                pessoasTocadas.Add(idPessoa);
                var existente = ligadas.FirstOrDefault(l => l.IdPessoa == idPessoa);
                if (existente is null)
                {
                    var novo = await g.Cn.ExecuteScalarAsync<long>(
                        """
                        INSERT INTO hprof_uni_pessoacadastro (id_pessoa, id_cadastroprofessor, id_escola, papel, criado_por)
                        VALUES (@p, @c, @e, @papel, @u);
                        SELECT LAST_INSERT_ID();
                        """, new { p = idPessoa, c = idCadastro, e = escola, papel, u = g.Usuario }, g.Tx);
                    await g.RegistrarAsync("Professor ↔ cadastro", "hprof_uni_pessoacadastro", "INSERIR", novo, null, new { IdPessoa = idPessoa, IdCadastro = idCadastro, Papel = papel });
                }
                else if (existente.Papel != papel || !existente.Ativo)
                {
                    await g.Cn.ExecuteAsync("UPDATE hprof_uni_pessoacadastro SET papel = @papel, ativo = 1, atualizado_por = @u WHERE id = @id",
                        new { id = existente.Id, papel, u = g.Usuario }, g.Tx);
                    await g.RegistrarAsync("Professor ↔ cadastro", "hprof_uni_pessoacadastro", "ATUALIZAR", existente.Id, existente, new { Papel = papel, Ativo = true });
                }
            }
            foreach (var sobra in ligadas.Where(l => l.Ativo && pessoasDaAula.All(p => p.Id != l.IdPessoa)))
            {
                await g.Cn.ExecuteAsync("UPDATE hprof_uni_pessoacadastro SET ativo = 0, atualizado_por = @u WHERE id = @id", new { id = sobra.Id, u = g.Usuario }, g.Tx);
                await g.RegistrarAsync("Professor ↔ cadastro", "hprof_uni_pessoacadastro", "DESATIVAR", sobra.Id, sobra, new { Ativo = false });
                pessoasTocadas.Add(sobra.IdPessoa);
            }
        }

        // envio completo: atribuições deste curso que não vieram mais ficam com zero aulas (não são apagadas)
        if (!lote.Parcial)
        {
            var antigas = (await g.Cn.QueryAsync<CodigoId>(
                """
                SELECT m.codigo_externo AS Codigo, m.id_interno AS Id
                  FROM hprof_uni_integracaomapa m
                  JOIN hprof_cadastroprofessor c ON c.ProfessorId = m.id_interno
                 WHERE m.id_instituicao = @i AND m.entidade = 'ATRIBUICAO' AND m.ativo = 1
                   AND c.IdEscola = @e AND c.AnoLetivo = @a AND c.Periodo = @p
                """, new { i = g.IdInstituicao, e = escola, a = ano, p = periodo }, g.Tx))
                .Where(x => !codigosNoLote.Contains(x.Codigo)).ToList();
            foreach (var antiga in antigas)
            {
                var antes = await g.Cn.ExecuteScalarAsync<int>("SELECT QuantidadeAula FROM hprof_cadastroprofessor WHERE ProfessorId = @id", new { id = antiga.Id }, g.Tx);
                await g.Cn.ExecuteAsync("UPDATE hprof_cadastroprofessor SET QuantidadeAula = 0 WHERE ProfessorId = @id", new { id = antiga.Id }, g.Tx);
                await g.Cn.ExecuteAsync("UPDATE hprof_uni_integracaomapa SET ativo = 0, atualizado_por = @u WHERE id_instituicao = @i AND entidade = 'ATRIBUICAO' AND codigo_externo = @c",
                    new { i = g.IdInstituicao, c = antiga.Codigo, u = g.Usuario }, g.Tx);
                await g.RegistrarAsync("Atribuições (cadastro do professor)", "hprof_cadastroprofessor", "DESATIVAR", antiga.Id,
                    new { QuantidadeAula = antes }, new { QuantidadeAula = 0 });
                var pessoasDaAntiga = await g.Cn.QueryAsync<long>(
                    "SELECT id_pessoa FROM hprof_uni_pessoacadastro WHERE id_cadastroprofessor = @c", new { c = antiga.Id }, g.Tx);
                foreach (var p in pessoasDaAntiga) pessoasTocadas.Add(p);
                g.Mensagens.Add(new LoteMensagemDto
                {
                    Secao = "atribuicoes", Item = 0, Campo = Cortar(antiga.Codigo, 60), Severidade = "AVISO",
                    Mensagem = $"A atribuição {antiga.Codigo} não veio neste envio; ficou com 0 aulas no cadastro (não foi apagada)."
                });
            }
        }

        // ---------- 6. indisponibilidades (opções do professor) ----------
        var opcoesNoLote = new HashSet<(int Cadastro, int Linha, int Coluna, int Opcao)>();
        foreach (var x in lote.Indisponibilidades)
        {
            var dia = NomesUniversidade.DiaDoTexto(x.Dia);
            if (dia is < 1 or > 5) continue; // o motor antigo usa segunda a sexta
            var (idPessoa, _, _) = await IntegracaoValidador.ResolverPessoaComTransacaoAsync(g, x.Professor);
            if (idPessoa == 0) continue;
            var linhas = await g.Cn.QueryAsync<int>(
                "SELECT id_cadastroprofessor FROM hprof_uni_pessoacadastro WHERE id_pessoa = @p AND id_escola = @e AND papel = 'PRINCIPAL' AND ativo = 1",
                new { p = idPessoa, e = escola }, g.Tx);
            var sigla = (x.Sigla ?? "BL").Trim().ToUpperInvariant();
            var tipo = sigla switch { "AC" => "Acumulo Cargo", "HL" => "Hora Livre", "ATPC" => "ATPC", _ => "Bloqueio" };
            if (tipo == "Bloqueio") sigla = "BL";
            foreach (var idCadastro in linhas)
            {
                opcoesNoLote.Add((idCadastro, x.Aula, dia, x.Opcao));
                var existe = await g.Cn.ExecuteScalarAsync<int>(
                    """
                    SELECT COUNT(*) FROM hprof_opcaohorario
                     WHERE IdEscola = @e AND AnoLetivo = @a AND Periodo = @p AND IdProfessor_fk = @c
                       AND Linha = @l AND Coluna = @col AND Opcao = @o AND Ativo = 1
                    """, new { e = escola, a = ano, p = periodo, c = idCadastro, l = x.Aula, col = dia, o = x.Opcao }, g.Tx);
                if (existe > 0) { g.SemMudanca("Indisponibilidades (opções)", "hprof_opcaohorario"); continue; }
                var codigoDia = $"{NomesUniversidade.Dia3(dia)}{x.Aula}";
                var novo = await g.Cn.ExecuteScalarAsync<long>(
                    """
                    INSERT INTO hprof_opcaohorario (IdEscola, Periodo, AnoLetivo, IdProfessor_fk, Linha, Coluna, Opcao, OpcaoDaSemana, Tipo, Sigla, Ativo, DataCadastro, HoraCadastro)
                    VALUES (@e, @p, @a, @c, @l, @col, @o, @od, @t, @s, TRUE, CURDATE(), CURTIME());
                    SELECT LAST_INSERT_ID();
                    """, new { e = escola, p = periodo, a = ano, c = idCadastro, l = x.Aula, col = dia, o = x.Opcao, od = codigoDia, t = tipo, s = sigla }, g.Tx);
                await g.RegistrarAsync("Indisponibilidades (opções)", "hprof_opcaohorario", "INSERIR", novo, null,
                    new { IdProfessor = idCadastro, OpcaoDaSemana = codigoDia, Tipo = tipo, Sigla = sigla, x.Opcao });
            }
        }

        if (!lote.Parcial)
        {
            // só as opções que o próprio módulo criou podem sair; as lançadas pela tela antiga ficam
            var criadas = await g.Cn.QueryAsync<OpcaoCriada>(
                """
                SELECT o.Id, o.IdProfessor_fk AS Cadastro, o.Linha, o.Coluna, o.Opcao
                  FROM hprof_opcaohorario o
                 WHERE o.IdEscola = @e AND o.AnoLetivo = @a AND o.Periodo = @p AND o.Ativo = 1
                   AND o.Id IN (SELECT al.id_registro FROM hprof_uni_integracaoalteracao al
                                  JOIN hprof_uni_integracaolote l ON l.id = al.id_lote
                                 WHERE l.id_instituicao = @i AND l.status = 'APLICADO'
                                   AND al.tabela = 'hprof_opcaohorario' AND al.operacao = 'INSERIR')
                """, new { e = escola, a = ano, p = periodo, i = g.IdInstituicao }, g.Tx);
            foreach (var o in criadas.Where(c => !opcoesNoLote.Contains((c.Cadastro, c.Linha, c.Coluna, c.Opcao))))
            {
                await g.Cn.ExecuteAsync(
                    "UPDATE hprof_opcaohorario SET Ativo = 0, DataInativado = CURDATE(), HoraInativado = CURTIME() WHERE Id = @id",
                    new { id = o.Id }, g.Tx);
                await g.RegistrarAsync("Indisponibilidades (opções)", "hprof_opcaohorario", "DESATIVAR", o.Id,
                    new { o.Cadastro, o.Linha, o.Coluna, o.Opcao, Ativo = true }, new { Ativo = false });
            }
        }

        // ---------- 7. divisão da carga (vínculo com o curso) ----------
        foreach (var idPessoa in pessoasTocadas)
        {
            var soma = await g.Cn.ExecuteScalarAsync<int>(
                """
                SELECT COALESCE(SUM(c.QuantidadeAula), 0)
                  FROM hprof_uni_pessoacadastro pc
                  JOIN hprof_cadastroprofessor c ON c.ProfessorId = pc.id_cadastroprofessor
                 WHERE pc.id_pessoa = @p AND pc.id_escola = @e AND pc.ativo = 1 AND c.AnoLetivo = @a AND c.Periodo = @per
                """, new { p = idPessoa, e = escola, a = ano, per = periodo }, g.Tx);
            await GravarVinculoAsync(g, idPessoa, escola, ano, periodo, soma, curso.Ordem, substituir: false);
        }
        return g.Previa();
    }

    /// <summary>substituir=true grava o valor pedido; false só aumenta até a soma das atribuições.</summary>
    private static async Task GravarVinculoAsync(Gravacao g, long idPessoa, int idEscola, int ano, string periodo, int aulas, int ordem, bool substituir)
    {
        var atual = await g.Cn.QuerySingleOrDefaultAsync<VinculoAtual>(
            "SELECT id AS Id, aulas_reservadas AS AulasReservadas, ordem_processamento AS Ordem FROM hprof_uni_pessoavinculo WHERE id_pessoa = @p AND id_escola = @e AND ano_letivo = @a AND periodo = @per",
            new { p = idPessoa, e = idEscola, a = ano, per = periodo }, g.Tx);
        if (atual is null)
        {
            var novo = await g.Cn.ExecuteScalarAsync<long>(
                """
                INSERT INTO hprof_uni_pessoavinculo (id_pessoa, id_escola, ano_letivo, periodo, aulas_reservadas, ordem_processamento, criado_por)
                VALUES (@p, @e, @a, @per, @q, @o, @u);
                SELECT LAST_INSERT_ID();
                """, new { p = idPessoa, e = idEscola, a = ano, per = periodo, q = aulas, o = ordem, u = g.Usuario }, g.Tx);
            await g.RegistrarAsync("Divisão da carga por curso", "hprof_uni_pessoavinculo", "INSERIR", novo, null, new { IdPessoa = idPessoa, IdEscola = idEscola, AulasReservadas = aulas });
            return;
        }
        var desejado = substituir ? aulas : Math.Max(atual.AulasReservadas, aulas);
        var ordemFinal = substituir ? ordem : atual.Ordem;
        if (desejado == atual.AulasReservadas && ordemFinal == atual.Ordem) { g.SemMudanca("Divisão da carga por curso", "hprof_uni_pessoavinculo"); return; }
        await g.Cn.ExecuteAsync(
            "UPDATE hprof_uni_pessoavinculo SET aulas_reservadas = @q, ordem_processamento = @o, ativo = 1, atualizado_por = @u WHERE id = @id",
            new { id = atual.Id, q = desejado, o = ordemFinal, u = g.Usuario }, g.Tx);
        await g.RegistrarAsync("Divisão da carga por curso", "hprof_uni_pessoavinculo", "ATUALIZAR", atual.Id, atual, new { AulasReservadas = desejado, Ordem = ordemFinal });
    }

    /// <summary>Chave no mapa para códigos que só valem dentro do curso.</summary>
    public static string ChaveDoCurso(string codigoCurso, string codigo) => $"{codigoCurso.Trim()}:{codigo.Trim()}";

    private static string Cortar(string texto, int max) => texto.Length <= max ? texto : texto[..max];

    /// <summary>
    /// Nome curto gravado antes no formato com espaço ("RENATA WOLF") passa para o formato do motor ("RENATAWOLF").
    /// Devolve o nome curto válido; grava a troca com auditoria.
    /// </summary>
    private static async Task<string> AcertarNomeCurtoAsync(Gravacao g, long idPessoa, string atual, HashSet<string> usados)
    {
        var limpo = NomesUniversidade.NomeCurtoSemSeparador(atual);
        if (limpo.Length == 0 || limpo == atual) return atual;
        usados.Remove(atual);
        limpo = NomesUniversidade.Unico(limpo, usados, 40);
        await g.Cn.ExecuteAsync("UPDATE hprof_uni_pessoaprofessor SET nome_curto = @c, atualizado_por = @u WHERE id = @id",
            new { c = limpo, u = g.Usuario, id = idPessoa }, g.Tx);
        await g.RegistrarAsync("Professores (pessoas)", "hprof_uni_pessoaprofessor", "ATUALIZAR", idPessoa,
            new { NomeCurto = atual }, new { NomeCurto = limpo });
        return limpo;
    }

    private sealed class OpcaoCriada
    {
        public long Id { get; set; }
        public int Cadastro { get; set; }
        public int Linha { get; set; }
        public int Coluna { get; set; }
        public int Opcao { get; set; }
    }

    private sealed record DisciplinaGravada(long Id, string Sigla, string Nome, string SiglaTurma);

    private sealed class MateriaAtual
    {
        public string Sigla { get; set; } = "";
        public string Descricao { get; set; } = "";
        public int QuantidadeAulas { get; set; }
        public string? ModalidadeEscolar { get; set; }
    }

    private sealed class CadastroAtual
    {
        public string NomeProfessor { get; set; } = "";
        public string TurmasAtribuidas { get; set; } = "";
        public string Materia { get; set; } = "";
        public int QuantidadeAula { get; set; }
        public long? FkMateria { get; set; }
        public string? Modalidade { get; set; }
    }

    private sealed class LigacaoAtual
    {
        public long Id { get; set; }
        public long IdPessoa { get; set; }
        public string Papel { get; set; } = "";
        public bool Ativo { get; set; }
    }

    private sealed class VinculoAtual
    {
        public long Id { get; set; }
        public int AulasReservadas { get; set; }
        public int Ordem { get; set; }
    }

    private sealed class CodigoId
    {
        public string Codigo { get; set; } = "";
        public long Id { get; set; }
    }
}

/// <summary>Estado de uma gravação: conexão, transação, lote e contadores da prévia.</summary>
public sealed class Gravacao
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly Dictionary<string, PreviaLinhaDto> _linhas = new();

    public Gravacao(MySqlConnection cn, MySqlTransaction tx, long idLote, long idInstituicao, string usuario)
    {
        Cn = cn; Tx = tx; IdLote = idLote; IdInstituicao = idInstituicao; Usuario = usuario;
    }

    public MySqlConnection Cn { get; }
    public MySqlTransaction Tx { get; }
    public long IdLote { get; }
    public long IdInstituicao { get; }
    public string Usuario { get; }
    public List<LoteMensagemDto> Mensagens { get; } = new();

    private PreviaLinhaDto Linha(string onde, string tabela)
    {
        var chave = onde + "|" + tabela;
        if (!_linhas.TryGetValue(chave, out var l)) _linhas[chave] = l = new PreviaLinhaDto { Onde = onde, Tabela = tabela };
        return l;
    }

    public void SemMudanca(string onde, string tabela) => Linha(onde, tabela).SemMudanca++;

    private readonly Dictionary<string, int> _tamanhos = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Tamanho máximo de uma coluna de texto, lido do information_schema (só leitura).</summary>
    public async Task<int> TamanhoColunaAsync(string tabela, string coluna, int padrao)
    {
        var chave = tabela + "." + coluna;
        if (_tamanhos.TryGetValue(chave, out var t)) return t;
        var lido = await Cn.ExecuteScalarAsync<long?>(
            """
            SELECT CHARACTER_MAXIMUM_LENGTH FROM information_schema.COLUMNS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @t AND COLUMN_NAME = @c
            """, new { t = tabela, c = coluna }, Tx);
        t = lido is > 0 and < int.MaxValue ? (int)lido.Value : padrao;
        _tamanhos[chave] = t;
        return t;
    }

    public async Task RegistrarAsync(string onde, string tabela, string operacao, long idRegistro, object? antes, object? depois)
    {
        var l = Linha(onde, tabela);
        if (operacao == "INSERIR") l.Novos++;
        else if (operacao == "DESATIVAR") l.Desativados++;
        else l.Atualizados++;
        await Cn.ExecuteAsync(
            """
            INSERT INTO hprof_uni_integracaoalteracao (id_lote, tabela, id_registro, operacao, valor_antes, valor_depois)
            VALUES (@l, @t, @r, @o, @a, @d)
            """, new
            {
                l = IdLote, t = tabela, r = idRegistro, o = operacao,
                a = antes is null ? null : JsonSerializer.Serialize(antes, Json),
                d = depois is null ? null : JsonSerializer.Serialize(depois, Json)
            }, Tx);
    }

    public async Task<long?> MapaAsync(string entidade, string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        return await Cn.ExecuteScalarAsync<long?>(
            "SELECT id_interno FROM hprof_uni_integracaomapa WHERE id_instituicao = @i AND entidade = @e AND codigo_externo = @c",
            new { i = IdInstituicao, e = entidade, c = codigo.Trim() }, Tx);
    }

    public Task GravarMapaAsync(string entidade, string codigo, string tabela, long idInterno) =>
        Cn.ExecuteAsync(
            """
            INSERT INTO hprof_uni_integracaomapa (id_instituicao, entidade, codigo_externo, tabela_interna, id_interno, ativo, criado_por)
            VALUES (@i, @e, @c, @t, @id, 1, @u)
            ON DUPLICATE KEY UPDATE id_interno = VALUES(id_interno), tabela_interna = VALUES(tabela_interna), ativo = 1, atualizado_por = VALUES(criado_por)
            """, new { i = IdInstituicao, e = entidade, c = codigo.Trim(), t = tabela, id = idInterno, u = Usuario }, Tx);

    public PreviaDto Previa() => new() { Linhas = _linhas.Values.OrderBy(l => l.Onde).ToList() };
}
