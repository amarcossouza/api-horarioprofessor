using API_HorarioProfessor.Universidade.Contratos;
using Dapper;
using MySqlConnector;

namespace API_HorarioProfessor.Universidade.Servicos;

/// <summary>
/// Confere um envio antes de qualquer gravação. ERRO bloqueia a aprovação; AVISO só informa.
/// Só lê o banco.
/// </summary>
public sealed class IntegracaoValidador
{
    private const int MaxAulasDia = 14;

    public async Task<List<LoteMensagemDto>> ValidarPessoasAsync(MySqlConnection cn, long idInstituicao, LotePessoasConteudo lote)
    {
        var m = new List<LoteMensagemDto>();
        ValidarCabecalho(m, lote.AnoLetivo, lote.Periodo);
        if (lote.Professores.Count == 0 && lote.Vinculos.Count == 0)
            Erro(m, "professores", 0, null, "O envio não tem professores nem vínculos.");

        var documentos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var codigos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < lote.Professores.Count; i++)
        {
            var p = lote.Professores[i];
            var item = i + 1;
            if (string.IsNullOrWhiteSpace(p.Nome)) Erro(m, "professores", item, "nome", "Informe o nome do professor.");
            if (string.IsNullOrWhiteSpace(p.Documento)) Erro(m, "professores", item, "documento", $"Informe a matrícula ou CPF de {p.Nome}.");
            else if (!documentos.Add(p.Documento.Trim())) Erro(m, "professores", item, "documento", $"Matrícula {p.Documento} repetida no envio.");
            if (!string.IsNullOrWhiteSpace(p.Codigo) && !codigos.Add(p.Codigo.Trim())) Erro(m, "professores", item, "codigo", $"Código {p.Codigo} repetido no envio.");
            if (p.CargaMaxima is < 1 or > 80) Erro(m, "professores", item, "cargaMaxima", $"Carga de {p.Nome} deve ficar entre 1 e 80 aulas por semana.");
        }

        if (lote.Vinculos.Count > 0)
        {
            var cargaPorPessoa = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in lote.Professores)
            {
                if (!string.IsNullOrWhiteSpace(p.Codigo)) cargaPorPessoa[p.Codigo.Trim()] = p.CargaMaxima;
                if (!string.IsNullOrWhiteSpace(p.Documento)) cargaPorPessoa[p.Documento.Trim()] = p.CargaMaxima;
            }
            var soma = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < lote.Vinculos.Count; i++)
            {
                var v = lote.Vinculos[i];
                var item = i + 1;
                var idCurso = await cn.ExecuteScalarAsync<long?>(
                    "SELECT id FROM hprof_uni_curso WHERE id_instituicao = @i AND codigo = @c AND ativo = 1",
                    new { i = idInstituicao, c = v.Curso });
                if (idCurso is null) Erro(m, "vinculos", item, "curso", $"Curso {v.Curso} não existe nesta instituição. Envie o curso antes.");
                if (v.AulasReservadas < 0) Erro(m, "vinculos", item, "aulasReservadas", "Aulas reservadas não pode ser negativo.");
                var chave = v.Professor.Trim();
                if (!cargaPorPessoa.ContainsKey(chave))
                {
                    var carga = await cn.ExecuteScalarAsync<int?>(
                        """
                        SELECT p.carga_maxima FROM hprof_uni_pessoaprofessor p
                         WHERE p.id_instituicao = @i AND (p.documento = @r OR p.id IN (
                               SELECT m.id_interno FROM hprof_uni_integracaomapa m
                                WHERE m.id_instituicao = @i AND m.entidade = 'PESSOA' AND m.codigo_externo = @r))
                         LIMIT 1
                        """, new { i = idInstituicao, r = chave });
                    if (carga is null) { Erro(m, "vinculos", item, "professor", $"Professor {v.Professor} não encontrado."); continue; }
                    cargaPorPessoa[chave] = carga.Value;
                }
                soma[chave] = soma.GetValueOrDefault(chave) + v.AulasReservadas;
            }
            foreach (var (pessoa, total) in soma)
            {
                if (cargaPorPessoa.TryGetValue(pessoa, out var carga) && total > carga)
                    Erro(m, "vinculos", 0, "aulasReservadas", $"Professor {pessoa}: a divisão soma {total} aulas e a carga é {carga}.");
            }
        }
        return m;
    }

    public async Task<List<LoteMensagemDto>> ValidarCursoAsync(MySqlConnection cn, long idInstituicao, LoteCursoConteudo lote)
    {
        var m = new List<LoteMensagemDto>();
        ValidarCabecalho(m, lote.AnoLetivo, lote.Periodo);
        if (string.IsNullOrWhiteSpace(lote.Curso.Codigo)) { Erro(m, "curso", 0, "codigo", "Informe o código do curso."); return m; }

        var curso = await cn.QuerySingleOrDefaultAsync<CursoDto>(
            "SELECT id AS Id, id_escola AS IdEscola, codigo AS Codigo, nome AS Nome FROM hprof_uni_curso WHERE id_instituicao = @i AND codigo = @c",
            new { i = idInstituicao, c = lote.Curso.Codigo.Trim() });

        // ---- curso, horários e dias ----
        var quantidadeAulasDia = 0;
        if (!lote.Parcial)
        {
            if (string.IsNullOrWhiteSpace(lote.Curso.Nome)) Erro(m, "curso", 0, "nome", "Informe o nome do curso.");
            if (lote.HorariosAula.Count is < 1 or > MaxAulasDia)
                Erro(m, "horariosAula", 0, null, $"Informe de 1 a {MaxAulasDia} horários de aula por dia.");
            TimeSpan anterior = TimeSpan.Zero;
            for (var i = 0; i < lote.HorariosAula.Count; i++)
            {
                var h = lote.HorariosAula[i];
                if (!NomesUniversidade.TentarHora(h.Inicio, out var ini) || !NomesUniversidade.TentarHora(h.Fim, out var fim) || fim <= ini)
                { Erro(m, "horariosAula", i + 1, "inicio", $"Horário da aula {h.Aula} inválido ({h.Inicio}–{h.Fim}). Use HH:MM."); continue; }
                if (ini < anterior) Erro(m, "horariosAula", i + 1, "inicio", $"A aula {h.Aula} começa antes do fim da aula anterior.");
                anterior = fim;
            }
            quantidadeAulasDia = lote.HorariosAula.Count;
            if (lote.Curso.Dias.Count == 0) Aviso(m, "curso", 0, "dias", "Dias não informados; serão usados segunda a sexta.");
            foreach (var d in lote.Curso.Dias)
            {
                var dia = NomesUniversidade.DiaDoTexto(d);
                if (dia == 0) Erro(m, "curso", 0, "dias", $"Dia '{d}' não reconhecido. Use SEG, TER, QUA, QUI, SEX ou SAB.");
                if (dia == 6) Aviso(m, "curso", 0, "dias", "O curso tem aula no sábado. O motor antigo monta segunda a sexta; aulas de sábado ficam para o CP-SAT.");
            }
            if (lote.Turmas.Count == 0) Erro(m, "turmas", 0, null, "Informe as turmas (fases) do curso.");
        }
        else
        {
            if (curso is null) Erro(m, "curso", 0, "codigo", $"Curso {lote.Curso.Codigo} ainda não existe. Envie o curso completo antes.");
            else quantidadeAulasDia = await cn.ExecuteScalarAsync<int>(
                "SELECT QuantidadeHorarios FROM hprof_escolas WHERE EscolaId = @e", new { e = curso.IdEscola });
        }

        // ---- turmas ----
        var turmas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < lote.Turmas.Count; i++)
        {
            var t = lote.Turmas[i];
            if (string.IsNullOrWhiteSpace(t.Codigo) || string.IsNullOrWhiteSpace(t.Nome)) Erro(m, "turmas", i + 1, "codigo", "Turma sem código ou sem nome.");
            else if (!turmas.Add(t.Codigo.Trim())) Erro(m, "turmas", i + 1, "codigo", $"Turma {t.Codigo} repetida.");
        }

        // ---- disciplinas ----
        var disciplinas = new Dictionary<string, LoteDisciplina>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < lote.Disciplinas.Count; i++)
        {
            var d = lote.Disciplinas[i];
            var item = i + 1;
            if (string.IsNullOrWhiteSpace(d.Codigo) || string.IsNullOrWhiteSpace(d.Nome)) { Erro(m, "disciplinas", item, "codigo", "Disciplina sem código ou sem nome."); continue; }
            if (!disciplinas.TryAdd(d.Codigo.Trim(), d)) Erro(m, "disciplinas", item, "codigo", $"Disciplina {d.Codigo} repetida.");
            if (d.AulasSemana is < 1 or > 20) Erro(m, "disciplinas", item, "aulasSemana", $"{d.Nome}: aulas por semana deve ficar entre 1 e 20.");
            if (!turmas.Contains(d.Turma.Trim()) && !await ExisteMapaAsync(cn, idInstituicao, "TURMA", IntegracaoAplicador.ChaveDoCurso(lote.Curso.Codigo, d.Turma)))
                Erro(m, "disciplinas", item, "turma", $"{d.Nome}: turma {d.Turma} não existe.");
        }

        // ---- atribuições e carga ----
        var pessoas = new Dictionary<string, (long Id, string Nome, int Carga)>(StringComparer.OrdinalIgnoreCase);
        var aulasNoLote = new Dictionary<long, int>();
        var codigosAtribuicao = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var somaPorDisciplina = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var coDocencia = 0;
        for (var i = 0; i < lote.Atribuicoes.Count; i++)
        {
            var a = lote.Atribuicoes[i];
            var item = i + 1;
            if (string.IsNullOrWhiteSpace(a.Codigo)) { Erro(m, "atribuicoes", item, "codigo", "Atribuição sem código."); continue; }
            if (!codigosAtribuicao.Add(a.Codigo.Trim())) Erro(m, "atribuicoes", item, "codigo", $"Atribuição {a.Codigo} repetida.");
            if (!disciplinas.ContainsKey(a.Disciplina.Trim()) && !await ExisteMapaAsync(cn, idInstituicao, "DISCIPLINA", IntegracaoAplicador.ChaveDoCurso(lote.Curso.Codigo, a.Disciplina)))
                Erro(m, "atribuicoes", item, "disciplina", $"Disciplina {a.Disciplina} não existe.");
            if (a.AulasSemana is < 1 or > 20) Erro(m, "atribuicoes", item, "aulasSemana", "Aulas por semana deve ficar entre 1 e 20.");
            if (a.Professores.Count == 0) Erro(m, "atribuicoes", item, "professores", $"Atribuição {a.Codigo} sem professor.");
            if (a.Professores.Count > 1) coDocencia++;
            if (a.AulasSemana % 2 == 1) Aviso(m, "atribuicoes", item, "aulasSemana", $"{NomeDisciplina(disciplinas, a.Disciplina)} tem {a.AulasSemana} aula(s): número ímpar, terá aula avulsa.");
            somaPorDisciplina[a.Disciplina.Trim()] = somaPorDisciplina.GetValueOrDefault(a.Disciplina.Trim()) + a.AulasSemana;
            foreach (var referencia in a.Professores.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!pessoas.TryGetValue(referencia, out var pessoa))
                {
                    pessoa = await ResolverPessoaAsync(cn, idInstituicao, referencia);
                    if (pessoa.Id == 0) { Erro(m, "atribuicoes", item, "professores", $"Professor {referencia} não está cadastrado. Envie os professores antes."); continue; }
                    pessoas[referencia] = pessoa;
                }
                aulasNoLote[pessoa.Id] = aulasNoLote.GetValueOrDefault(pessoa.Id) + a.AulasSemana;
            }
        }
        if (coDocencia > 0)
            Aviso(m, "atribuicoes", 0, "professores", $"{coDocencia} atribuição(ões) com dois professores. O motor antigo distribui o primeiro; o segundo fica ocupado no mesmo horário quando a grade for publicada.");
        foreach (var d in disciplinas.Values)
        {
            var soma = somaPorDisciplina.GetValueOrDefault(d.Codigo.Trim());
            if (soma == 0) Aviso(m, "disciplinas", 0, d.Codigo, $"{d.Nome} não tem professor atribuído no envio.");
            else if (soma != d.AulasSemana && lote.Disciplinas.Count > 0)
                Aviso(m, "disciplinas", 0, d.Codigo, $"{d.Nome}: disciplina com {d.AulasSemana} aulas e atribuições somando {soma}.");
        }

        if (aulasNoLote.Count > 0)
        {
            var idsMantidosDoCurso = new List<int>();
            if (curso is not null && lote.Parcial)
            {
                var mapa = (await cn.QueryAsync<CodigoId>(
                    "SELECT codigo_externo AS Codigo, id_interno AS Id FROM hprof_uni_integracaomapa WHERE id_instituicao = @i AND entidade = 'ATRIBUICAO' AND ativo = 1",
                    new { i = idInstituicao })).ToList();
                var substituidos = mapa.Where(x => codigosAtribuicao.Contains(x.Codigo.StartsWith(lote.Curso.Codigo.Trim() + ":") ? x.Codigo[(lote.Curso.Codigo.Trim().Length + 1)..] : x.Codigo))
                    .Select(x => (int)x.Id).ToHashSet();
                idsMantidosDoCurso = (await cn.QueryAsync<int>(
                    "SELECT ProfessorId FROM hprof_cadastroprofessor WHERE IdEscola = @e AND AnoLetivo = @a AND Periodo = @p",
                    new { e = curso.IdEscola, a = lote.AnoLetivo, p = lote.Periodo })).Where(id => !substituidos.Contains(id)).ToList();
            }
            foreach (var pessoa in pessoas.Values.DistinctBy(p => p.Id))
            {
                var outros = await cn.ExecuteScalarAsync<int>(
                    """
                    SELECT COALESCE(SUM(c.QuantidadeAula), 0)
                      FROM hprof_uni_pessoacadastro pc
                      JOIN hprof_cadastroprofessor c ON c.ProfessorId = pc.id_cadastroprofessor
                     WHERE pc.id_pessoa = @p AND pc.ativo = 1 AND c.AnoLetivo = @a AND pc.id_escola <> @e
                    """, new { p = pessoa.Id, a = lote.AnoLetivo, e = curso?.IdEscola ?? -1 });
                var mantidos = idsMantidosDoCurso.Count == 0 ? 0 : await cn.ExecuteScalarAsync<int>(
                    """
                    SELECT COALESCE(SUM(c.QuantidadeAula), 0)
                      FROM hprof_uni_pessoacadastro pc
                      JOIN hprof_cadastroprofessor c ON c.ProfessorId = pc.id_cadastroprofessor
                     WHERE pc.id_pessoa = @p AND pc.ativo = 1 AND pc.id_cadastroprofessor IN @ids
                    """, new { p = pessoa.Id, ids = idsMantidosDoCurso });
                var total = outros + mantidos + aulasNoLote[pessoa.Id];
                if (total > pessoa.Carga)
                    Erro(m, "atribuicoes", 0, "professores",
                        $"{pessoa.Nome} ficaria com {total} de {pessoa.Carga} aulas ({outros + mantidos} já atribuídas + {aulasNoLote[pessoa.Id]} deste envio).");
            }
        }

        // ---- indisponibilidades ----
        for (var i = 0; i < lote.Indisponibilidades.Count; i++)
        {
            var x = lote.Indisponibilidades[i];
            var item = i + 1;
            var dia = NomesUniversidade.DiaDoTexto(x.Dia);
            if (dia == 0) Erro(m, "indisponibilidades", item, "dia", $"Dia '{x.Dia}' não reconhecido.");
            else if (dia == 6) Aviso(m, "indisponibilidades", item, "dia", "Indisponibilidade no sábado não é usada pelo motor antigo.");
            if (x.Aula < 1 || (quantidadeAulasDia > 0 && x.Aula > quantidadeAulasDia))
                Erro(m, "indisponibilidades", item, "aula", $"Aula {x.Aula} fora da grade do curso (1 a {quantidadeAulasDia}).");
            if (x.Opcao is not (1 or 2)) Erro(m, "indisponibilidades", item, "opcao", "Opção deve ser 1 ou 2.");
            if (!pessoas.ContainsKey(x.Professor))
            {
                var pessoa = await ResolverPessoaAsync(cn, idInstituicao, x.Professor);
                if (pessoa.Id == 0) Erro(m, "indisponibilidades", item, "professor", $"Professor {x.Professor} não encontrado.");
                else pessoas[x.Professor] = pessoa;
            }
        }
        return m;
    }

    internal static async Task<(long Id, string Nome, int Carga)> ResolverPessoaAsync(MySqlConnection cn, long idInstituicao, string referencia)
    {
        var r = await cn.QueryFirstOrDefaultAsync<PessoaReferencia>(
            """
            SELECT p.id AS Id, p.nome AS Nome, p.carga_maxima AS Carga
              FROM hprof_uni_pessoaprofessor p
             WHERE p.id_instituicao = @i AND p.ativo = 1
               AND (p.documento = @r OR p.id = (SELECT m.id_interno FROM hprof_uni_integracaomapa m
                                                 WHERE m.id_instituicao = @i AND m.entidade = 'PESSOA'
                                                   AND m.codigo_externo = @r LIMIT 1))
             LIMIT 1
            """, new { i = idInstituicao, r = referencia.Trim() });
        return r is null ? (0, "", 0) : (r.Id, r.Nome, r.Carga);
    }

    private sealed class PessoaReferencia
    {
        public long Id { get; set; }
        public string Nome { get; set; } = "";
        public int Carga { get; set; }
    }

    private sealed class CodigoId
    {
        public string Codigo { get; set; } = "";
        public long Id { get; set; }
    }

    /// <summary>Mesma busca de pessoa, dentro da transação da gravação (vê pessoas recém-inseridas).</summary>
    internal static async Task<(long Id, string Nome, int Carga)> ResolverPessoaComTransacaoAsync(Gravacao g, string referencia)
    {
        var r = await g.Cn.QueryFirstOrDefaultAsync<PessoaReferencia>(
            """
            SELECT p.id AS Id, p.nome AS Nome, p.carga_maxima AS Carga
              FROM hprof_uni_pessoaprofessor p
             WHERE p.id_instituicao = @i
               AND (p.documento = @r OR p.id = (SELECT m.id_interno FROM hprof_uni_integracaomapa m
                                                 WHERE m.id_instituicao = @i AND m.entidade = 'PESSOA'
                                                   AND m.codigo_externo = @r LIMIT 1))
             LIMIT 1
            """, new { i = g.IdInstituicao, r = referencia.Trim() }, g.Tx);
        return r is null ? (0, "", 0) : (r.Id, r.Nome, r.Carga);
    }

    private static async Task<bool> ExisteMapaAsync(MySqlConnection cn, long idInstituicao, string entidade, string codigo) =>
        await cn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM hprof_uni_integracaomapa WHERE id_instituicao = @i AND entidade = @e AND codigo_externo = @c",
            new { i = idInstituicao, e = entidade, c = codigo.Trim() }) > 0;

    private static string NomeDisciplina(Dictionary<string, LoteDisciplina> d, string codigo) =>
        d.TryGetValue(codigo.Trim(), out var x) ? x.Nome : codigo;

    private static void ValidarCabecalho(List<LoteMensagemDto> m, int ano, string periodo)
    {
        if (ano is < 2000 or > 2100) Erro(m, "envio", 0, "anoLetivo", "Ano letivo inválido.");
        if (string.IsNullOrWhiteSpace(periodo)) Erro(m, "envio", 0, "periodo", "Informe o período (ex.: Manha).");
    }

    internal static void Erro(List<LoteMensagemDto> m, string secao, int item, string? campo, string msg) =>
        m.Add(new LoteMensagemDto { Secao = secao, Item = item, Campo = campo, Mensagem = msg, Severidade = "ERRO" });

    internal static void Aviso(List<LoteMensagemDto> m, string secao, int item, string? campo, string msg) =>
        m.Add(new LoteMensagemDto { Secao = secao, Item = item, Campo = campo, Mensagem = msg, Severidade = "AVISO" });
}
