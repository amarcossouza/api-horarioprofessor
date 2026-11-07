using API_HorarioProfessor.Dto;
using API_HorarioProfessor.Services;
using API_HorarioProfessor.Universidade.Servicos;

namespace API_HorarioProfessor.Universidade.Motores;

/// <summary>
/// Envolve o retrato da grade enviado ao motor novo (api-cenarios / CP-SAT) sem alterá-lo.
/// Para cursos de universidade, acrescenta regras de 1ª opção com sigla AC (nunca cedidas pelo solver)
/// nos horários em que a pessoa já está em outro curso publicado.
/// </summary>
public sealed class GradeOrigemSnapshotUniversidade : IGradeOrigemSnapshotService
{
    private readonly IGradeOrigemSnapshotService _original;
    private readonly IOcupacaoUniversidadeService _ocupacao;
    private readonly ILogger<GradeOrigemSnapshotUniversidade> _logger;

    public GradeOrigemSnapshotUniversidade(IGradeOrigemSnapshotService original, IOcupacaoUniversidadeService ocupacao, ILogger<GradeOrigemSnapshotUniversidade> logger)
    {
        _original = original; _ocupacao = ocupacao; _logger = logger;
    }

    public async Task<GradeOrigemSnapshotDto> ObterAsync(int idEscola, int anoLetivo, string periodo, int quantidadeHorarios)
    {
        var snapshot = await _original.ObterAsync(idEscola, anoLetivo, periodo, quantidadeHorarios);
        try
        {
            var bloqueios = await _ocupacao.ObterBloqueiosAsync(idEscola, anoLetivo, periodo);
            var idOrigem = -1;
            foreach (var b in bloqueios.Where(x => x.Aula <= quantidadeHorarios))
            {
                snapshot.Regras.Add(new RegraGradeSnapshotDto
                {
                    IdOrigem = idOrigem--,
                    Categoria = "BLOQUEIO",
                    IdProfessor = b.IdCadastro,
                    NomeProfessor = b.NomeProfessor,
                    DiaIndex = b.Dia - 1,
                    Slot = b.Aula - 1,
                    DiaOrigem = $"{NomesUniversidade.Dia3(b.Dia)}{b.Aula}",
                    LinhaOrigem = b.Aula,
                    ColunaOrigem = b.Dia,
                    Tipo = "Acumulo Cargo",
                    Sigla = "AC",
                    Prioridade = 1,
                    Obrigatoria = true,
                    Ativa = true,
                    Origem = "UNIVERSIDADE_OCUPACAO",
                    Descricao = b.Descricao
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Universidade: não foi possível acrescentar a ocupação ao retrato da escola {Escola}.", idEscola);
        }
        return snapshot;
    }
}
