namespace API_HorarioProfessor.Dto;

public sealed class GradeOrigemSnapshotDto
{
    public string FormatoVersao { get; init; } = "1.0";
    public ContextoGradeSnapshotDto Contexto { get; init; } = new();
    public List<string> Turmas { get; init; } = new();
    public List<TurmaGradeSnapshotDto> TurmasDetalhes { get; init; } = new();
    public List<ProfessorGradeSnapshotDto> Professores { get; init; } = new();
    public List<DisciplinaGradeSnapshotDto> Disciplinas { get; init; } = new();
    public List<RegraGradeSnapshotDto> Regras { get; init; } = new();
    public GradeMatrizesSnapshotDto Matrizes { get; init; } = new();
}

public sealed class ContextoGradeSnapshotDto
{
    public int IdEscola { get; init; }
    public int AnoLetivo { get; init; }
    public string Periodo { get; init; } = "";
    public int QuantidadeHorarios { get; init; }
    public DateTime GeradoEm { get; init; }
    public string Origem { get; init; } = "API_HORARIO_PROFESSOR";
}

public sealed class TurmaGradeSnapshotDto
{
    public int IdTurma { get; init; }
    public string Nome { get; init; } = "";
    public bool Polivalente { get; init; }
}

public sealed class ProfessorGradeSnapshotDto
{
    public int IdProfessor { get; init; }
    public string Nome { get; init; } = "";
    public int CargaTotal { get; init; }
    public List<AtribuicaoProfessorSnapshotDto> Atribuicoes { get; init; } = new();
}

public sealed class AtribuicaoProfessorSnapshotDto
{
    public string Turma { get; init; } = "";
    public string Disciplina { get; init; } = "";
    public string Sigla { get; init; } = "";
    public int QuantidadeAulas { get; init; }
}

public sealed class DisciplinaGradeSnapshotDto
{
    public int IdOrigem { get; init; }
    public string Descricao { get; init; } = "";
    public string Sigla { get; init; } = "";
    public int AnoEscolar { get; init; }
    public bool TodosOsAnos { get; init; }
    public int QuantidadeAulas { get; init; }
}

public sealed class RegraGradeSnapshotDto
{
    public int IdOrigem { get; init; }
    public string Categoria { get; init; } = "";
    public int? IdProfessor { get; init; }
    public string? NomeProfessor { get; init; }
    public int? DiaIndex { get; init; }
    public int? Slot { get; init; }
    public string? Turma { get; init; }
    public string? DiaOrigem { get; init; }
    public int? LinhaOrigem { get; init; }
    public int? ColunaOrigem { get; init; }
    public string? Tipo { get; init; }
    public string? Sigla { get; init; }
    public int Prioridade { get; init; }
    public bool Obrigatoria { get; init; }
    public bool Ativa { get; init; }
    public string Origem { get; init; } = "CADASTRO";
    public string? Descricao { get; init; }
}

public sealed class GradeMatrizesSnapshotDto
{
    public List<List<string>> Segunda { get; init; } = new();
    public List<List<string>> Terca { get; init; } = new();
    public List<List<string>> Quarta { get; init; } = new();
    public List<List<string>> Quinta { get; init; } = new();
    public List<List<string>> Sexta { get; init; } = new();
}
