namespace API_HorarioProfessor.Universidade.Contratos;

// ============================================================================
// Contratos do módulo universidade (respostas e pedidos da área Universidade).
// As propriedades são públicas com set para o Dapper preencher pelos aliases do SQL.
// ============================================================================

public sealed class InstituicaoDto
{
    public long Id { get; set; }
    public string Sigla { get; set; } = "";
    public string Nome { get; set; } = "";
    public int IdEscolaPrincipal { get; set; }
}

public sealed class CursoDto
{
    public long Id { get; set; }
    public long IdInstituicao { get; set; }
    public int IdEscola { get; set; }
    public string Codigo { get; set; } = "";
    public string Nome { get; set; } = "";
    public string? Turno { get; set; }
    public string Dias { get; set; } = "SEG,TER,QUA,QUI,SEX";
    public string? IntervalosJson { get; set; }
    public int Ordem { get; set; }
    public int QuantidadeHorarios { get; set; }
    public string NomeEscola { get; set; } = "";
}

public sealed class ContextoUniversidadeDto
{
    public InstituicaoDto Instituicao { get; set; } = new();
    public List<CursoDto> Cursos { get; set; } = new();
    public int IdEscolaAtual { get; set; }
    public CursoDto? CursoAtual { get; set; }
}

public sealed class PessoaDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = "";
    public string NomeCurto { get; set; } = "";
    public string Documento { get; set; } = "";
    public int CargaMaxima { get; set; }
    public bool Ativo { get; set; }
}

public sealed class CargaCursoDto
{
    public int IdEscola { get; set; }
    public string Curso { get; set; } = "";
    public string CodigoCurso { get; set; } = "";
    public int AulasReservadas { get; set; }
    public int AulasAtribuidas { get; set; }
    public int Ordem { get; set; }
}

public sealed class PainelLinhaDto
{
    public long IdPessoa { get; set; }
    public string Nome { get; set; } = "";
    public string Documento { get; set; } = "";
    public int CargaMaxima { get; set; }
    public List<CargaCursoDto> Cursos { get; set; } = new();
    public int TotalAtribuido { get; set; }
    public int Livre { get; set; }
    public int QuantidadeCursos { get; set; }
    /// <summary>OK, ACIMA_DA_CARGA, ACIMA_DO_RESERVADO ou SEM_AULAS.</summary>
    public string Situacao { get; set; } = "OK";
}

public sealed class AtribuicaoDto
{
    public int IdCadastro { get; set; }
    public int IdEscola { get; set; }
    public string Curso { get; set; } = "";
    public string NomeProfessorMotor { get; set; } = "";
    public long IdPessoa { get; set; }
    public string Pessoa { get; set; } = "";
    public string Papel { get; set; } = "PRINCIPAL";
    public string Turma { get; set; } = "";
    public string TurmaNome { get; set; } = "";
    public string Sigla { get; set; } = "";
    public string Disciplina { get; set; } = "";
    public int Aulas { get; set; }
}

public sealed class OcupacaoDto
{
    public long IdPessoa { get; set; }
    public string Pessoa { get; set; } = "";
    public int IdEscolaOrigem { get; set; }
    public string Curso { get; set; } = "";
    public int Dia { get; set; }
    public TimeSpan Inicio { get; set; }
    public TimeSpan Fim { get; set; }
    public string? Descricao { get; set; }
}

public sealed class PessoaDetalheDto
{
    public PessoaDto Pessoa { get; set; } = new();
    public List<CargaCursoDto> Vinculos { get; set; } = new();
    public List<AtribuicaoDto> Atribuicoes { get; set; } = new();
    public int TotalAtribuido { get; set; }
}

public sealed class VinculoPedido
{
    public int IdEscola { get; set; }
    public int AulasReservadas { get; set; }
    public int Ordem { get; set; } = 1;
}

public sealed class SalvarPessoaPedido
{
    public long? Id { get; set; }
    public string Nome { get; set; } = "";
    public string Documento { get; set; } = "";
    public int CargaMaxima { get; set; }
    public bool Ativo { get; set; } = true;
    public int AnoLetivo { get; set; }
    public string Periodo { get; set; } = "Manha";
    public string Usuario { get; set; } = "";
    public List<VinculoPedido> Vinculos { get; set; } = new();
}

public sealed class NovaAtribuicaoPedido
{
    public int AnoLetivo { get; set; }
    public string Periodo { get; set; } = "Manha";
    public string Usuario { get; set; } = "";
    public long IdPessoa { get; set; }
    public long? IdPessoaApoio { get; set; }
    public string TurmaSigla { get; set; } = "";
    public string Disciplina { get; set; } = "";
    public int Aulas { get; set; }
}

public sealed class PlanilhaSalvaDto
{
    public int Id { get; set; }
    public string? NomePlanilha { get; set; }
    public string? NomePlanilhaEscola { get; set; }
    public DateTime? DataCadastro { get; set; }
    public bool Publicada { get; set; }
}

public sealed class HorarioAulaDto
{
    public int Aula { get; set; }
    public string Inicio { get; set; } = "";
    public string Fim { get; set; } = "";
}

public sealed class IntervaloDto
{
    public string Nome { get; set; } = "";
    public string Inicio { get; set; } = "";
    public string Fim { get; set; } = "";
}

public sealed class TurmaUniversidadeDto
{
    public string Sigla { get; set; } = "";
    public string Nome { get; set; } = "";
    public int Ordem { get; set; }
}

public sealed class CelulaGradeDto
{
    public int Dia { get; set; }
    public int Aula { get; set; }
    public string Turma { get; set; } = "";
    public string NomeMotor { get; set; } = "";
    public string Sigla { get; set; } = "";
    public string Disciplina { get; set; } = "";
    public List<string> Professores { get; set; } = new();
}

public sealed class GradeCursoDto
{
    public CursoDto Curso { get; set; } = new();
    public string NomeInstituicao { get; set; } = "";
    public int? IdPlanilha { get; set; }
    public string? NomePlanilha { get; set; }
    public bool Publicada { get; set; }
    public List<HorarioAulaDto> Horarios { get; set; } = new();
    public List<IntervaloDto> Intervalos { get; set; } = new();
    public List<string> Dias { get; set; } = new();
    public List<TurmaUniversidadeDto> Turmas { get; set; } = new();
    public List<CelulaGradeDto> Celulas { get; set; } = new();
    public string Mensagem { get; set; } = "";
}

public sealed class PublicarPedido
{
    public int AnoLetivo { get; set; }
    public string Periodo { get; set; } = "Manha";
    public int IdPlanilha { get; set; }
    public string Usuario { get; set; } = "";
}

public sealed class PublicacaoResultadoDto
{
    public long IdPublicacao { get; set; }
    public int TotalAulas { get; set; }
    public int TotalOcupacoes { get; set; }
    public int CelulasSemProfessorConhecido { get; set; }
    public string Mensagem { get; set; } = "";
}

public sealed class SemanaProfessorDto
{
    public PessoaDto Pessoa { get; set; } = new();
    public List<OcupacaoDto> Aulas { get; set; } = new();
    public int TotalAulasPublicadas { get; set; }
}

public sealed class ConflitoDto
{
    public string Pessoa { get; set; } = "";
    public int Dia { get; set; }
    public OcupacaoDto Primeira { get; set; } = new();
    public OcupacaoDto Segunda { get; set; } = new();
}

public sealed class ConferenciaDto
{
    public int TotalPessoas { get; set; }
    public int PessoasEmMaisDeUmCurso { get; set; }
    public int TotalOcupacoes { get; set; }
    public List<ConflitoDto> Conflitos { get; set; } = new();
    public List<PainelLinhaDto> AcimaDaCarga { get; set; } = new();
    public List<string> CursosSemPublicacao { get; set; } = new();
}
