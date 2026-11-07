namespace API_HorarioProfessor.Universidade.Contratos;

// ============================================================================
// Contratos da integração (API da universidade, planilha colada e telas).
// Todo envio vira um "lote" guardado como chegou, validado, aprovado e gravado.
// ============================================================================

public static class TiposLote
{
    public const string Pessoas = "PESSOAS";
    public const string Curso = "CURSO";
}

public static class StatusLote
{
    public const string Recebido = "RECEBIDO";
    public const string Validado = "VALIDADO";
    public const string ComErros = "COM_ERROS";
    public const string Rejeitado = "REJEITADO";
    public const string Aplicado = "APLICADO";
}

public static class OrigensLote
{
    public const string Api = "API";
    public const string Planilha = "PLANILHA";
    public const string Tela = "TELA";
}

// ---------- conteúdo dos envios ----------

public sealed class LoteProfessor
{
    public string Codigo { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Documento { get; set; } = "";
    public int CargaMaxima { get; set; }
    public bool? Ativo { get; set; }
}

public sealed class LoteVinculo
{
    public string Professor { get; set; } = "";
    public string Curso { get; set; } = "";
    public int AulasReservadas { get; set; }
    public int Ordem { get; set; } = 1;
}

public sealed class LotePessoasConteudo
{
    public string CodigoEnvio { get; set; } = "";
    public int AnoLetivo { get; set; }
    public string Periodo { get; set; } = "Manha";
    public List<LoteProfessor> Professores { get; set; } = new();
    public List<LoteVinculo> Vinculos { get; set; } = new();
}

public sealed class LoteCurso
{
    public string Codigo { get; set; } = "";
    public string Nome { get; set; } = "";
    public string? Turno { get; set; }
    public List<string> Dias { get; set; } = new();
}

public sealed class LoteHorario
{
    public int Aula { get; set; }
    public string Inicio { get; set; } = "";
    public string Fim { get; set; } = "";
}

public sealed class LoteIntervalo
{
    public string Nome { get; set; } = "";
    public string Inicio { get; set; } = "";
    public string Fim { get; set; } = "";
}

public sealed class LoteTurma
{
    public string Codigo { get; set; } = "";
    public string Nome { get; set; } = "";
}

public sealed class LoteDisciplina
{
    public string Codigo { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Turma { get; set; } = "";
    public int AulasSemana { get; set; }
}

public sealed class LoteAtribuicao
{
    public string Codigo { get; set; } = "";
    public string Disciplina { get; set; } = "";
    public List<string> Professores { get; set; } = new();
    public int AulasSemana { get; set; }
    public int BlocoAulas { get; set; } = 2;
}

public sealed class LoteIndisponibilidade
{
    public string Professor { get; set; } = "";
    public string Dia { get; set; } = "";
    public int Aula { get; set; }
    public string? Sigla { get; set; }
    public int Opcao { get; set; } = 1;
}

public sealed class LoteCursoConteudo
{
    public string CodigoEnvio { get; set; } = "";
    public int AnoLetivo { get; set; }
    public string Periodo { get; set; } = "Manha";
    /// <summary>true = só acrescenta/atualiza (planilha, tela); não exige horários nem turmas.</summary>
    public bool Parcial { get; set; }
    public LoteCurso Curso { get; set; } = new();
    public List<LoteHorario> HorariosAula { get; set; } = new();
    public List<LoteIntervalo> Intervalos { get; set; } = new();
    public List<LoteTurma> Turmas { get; set; } = new();
    public List<LoteDisciplina> Disciplinas { get; set; } = new();
    public List<LoteAtribuicao> Atribuicoes { get; set; } = new();
    public List<LoteIndisponibilidade> Indisponibilidades { get; set; } = new();
}

// ---------- respostas ----------

public sealed class LoteResumoDto
{
    public long Id { get; set; }
    public string CodigoEnvio { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Origem { get; set; } = "";
    public string Status { get; set; } = "";
    public int AnoLetivo { get; set; }
    public string Periodo { get; set; } = "";
    public int TotalItens { get; set; }
    public int TotalErros { get; set; }
    public int TotalAvisos { get; set; }
    public string EnviadoPor { get; set; } = "";
    public DateTime RecebidoEm { get; set; }
    public string? RecebidoDeIp { get; set; }
    public string? AprovadoPor { get; set; }
    public DateTime? AprovadoEm { get; set; }
    public DateTime? AplicadoEm { get; set; }
    public string? MotivoRejeicao { get; set; }
}

public sealed class LoteMensagemDto
{
    public string Secao { get; set; } = "";
    public int Item { get; set; }
    public string? Campo { get; set; }
    public string Mensagem { get; set; } = "";
    public string Severidade { get; set; } = "ERRO";
}

public sealed class AuditoriaDto
{
    public string Acao { get; set; } = "";
    public string Usuario { get; set; } = "";
    public string TipoAtor { get; set; } = "";
    public string? Ip { get; set; }
    public DateTime OcorridoEm { get; set; }
    public string? Detalhe { get; set; }
}

public sealed class PreviaLinhaDto
{
    public string Onde { get; set; } = "";
    public string Tabela { get; set; } = "";
    public int Novos { get; set; }
    public int Atualizados { get; set; }
    public int Desativados { get; set; }
    public int SemMudanca { get; set; }
}

public sealed class PreviaDto
{
    public List<PreviaLinhaDto> Linhas { get; set; } = new();
    public int TotalGravacoes => Linhas.Sum(l => l.Novos + l.Atualizados + l.Desativados);
}

public sealed class AlteracaoDto
{
    public string Tabela { get; set; } = "";
    public long IdRegistro { get; set; }
    public string Operacao { get; set; } = "";
    public string? ValorAntes { get; set; }
    public string? ValorDepois { get; set; }
    public DateTime GravadoEm { get; set; }
}

public sealed class LoteDetalheDto
{
    public LoteResumoDto Lote { get; set; } = new();
    public List<LoteMensagemDto> Mensagens { get; set; } = new();
    public List<AuditoriaDto> Auditoria { get; set; } = new();
    public PreviaDto? Previa { get; set; }
    public List<AlteracaoDto> Alteracoes { get; set; } = new();
    public string ConteudoJson { get; set; } = "";
}

public sealed class ReceberLoteResposta
{
    public long IdLote { get; set; }
    public string CodigoEnvio { get; set; } = "";
    public string Status { get; set; } = "";
    public int TotalErros { get; set; }
    public int TotalAvisos { get; set; }
    public bool JaRecebido { get; set; }
    public string Mensagem { get; set; } = "";
    public List<LoteMensagemDto> Mensagens { get; set; } = new();
}

public sealed class DecisaoLotePedido
{
    public string Usuario { get; set; } = "";
    public string? Motivo { get; set; }
}

public sealed class ClienteIntegracao
{
    public long Id { get; set; }
    public long IdInstituicao { get; set; }
    public string Sigla { get; set; } = "";
    public bool AplicacaoAutomatica { get; set; }
}

// ---------- planilha colada ----------

public sealed class PlanilhaPedido
{
    public int IdEscola { get; set; }
    public int AnoLetivo { get; set; }
    public string Periodo { get; set; } = "Manha";
    public string Texto { get; set; } = "";
    public string Usuario { get; set; } = "";
}

public sealed class PlanilhaLinhaDto
{
    public int Linha { get; set; }
    public string Documento { get; set; } = "";
    public string Professor { get; set; } = "";
    public string Fase { get; set; } = "";
    public string Disciplina { get; set; } = "";
    public int Aulas { get; set; }
    public string? Apoio { get; set; }
    public bool Ok { get; set; }
    public string Mensagem { get; set; } = "";
}

public sealed class PlanilhaLeituraDto
{
    public string Curso { get; set; } = "";
    public List<PlanilhaLinhaDto> Linhas { get; set; } = new();
    public int Prontas => Linhas.Count(l => l.Ok);
    public int ComErro => Linhas.Count(l => !l.Ok);
    public List<string> ColunasReconhecidas { get; set; } = new();
}
