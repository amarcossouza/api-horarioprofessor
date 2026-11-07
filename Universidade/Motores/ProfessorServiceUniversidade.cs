using API_HorarioProfessor.Dto;
using API_HorarioProfessor.Models;
using API_HorarioProfessor.Services;
using API_HorarioProfessor.Universidade.Servicos;

namespace API_HorarioProfessor.Universidade.Motores;

/// <summary>
/// Envolve o ProfessorService do motor antigo sem alterá-lo.
/// Para cursos de universidade, acrescenta aos bloqueios do professor (DiasNaoTrabalha, formato "TER1")
/// os horários em que a mesma pessoa já está em outro curso publicado (AC automático).
/// Para as escolas comuns devolve exatamente o resultado original.
/// Se o módulo universidade falhar (por exemplo, tabelas ainda não criadas), o motor segue sem os bloqueios.
/// </summary>
public sealed class ProfessorServiceUniversidade : IProfessorService
{
    private readonly ProfessorService _original;
    private readonly IOcupacaoUniversidadeService _ocupacao;
    private readonly ILogger<ProfessorServiceUniversidade> _logger;

    public ProfessorServiceUniversidade(ProfessorService original, IOcupacaoUniversidadeService ocupacao, ILogger<ProfessorServiceUniversidade> logger)
    {
        _original = original; _ocupacao = ocupacao; _logger = logger;
    }

    public async Task<List<Professor>> GetProfessoresAsync(int idEscola, string periodo, int ativo, int anoLetivo, int OrdenarMaiorNumeroOpcao1OuOpcao2)
    {
        var professores = await _original.GetProfessoresAsync(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2);
        try
        {
            var bloqueios = await _ocupacao.ObterBloqueiosAsync(idEscola, anoLetivo, periodo);
            if (bloqueios.Count == 0) return professores;
            var porCadastro = bloqueios.GroupBy(b => b.IdCadastro).ToDictionary(g => g.Key, g => g.ToList());
            var total = 0;
            foreach (var professor in professores)
            {
                if (!porCadastro.TryGetValue(professor.Id, out var lista)) continue;
                var atuais = (professor.DiasNaoTrabalha ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                foreach (var b in lista)
                {
                    var codigo = $"{NomesUniversidade.Dia3(b.Dia)}{b.Aula}";
                    if (atuais.Contains(codigo, StringComparer.OrdinalIgnoreCase)) continue;
                    atuais.Add(codigo);
                    professor.OpcoesProfessor?.AdicionarOpcao(professor.Id, 1, b.Aula, b.Dia, "Acumulo Cargo");
                    total++;
                }
                professor.DiasNaoTrabalha = string.Join(",", atuais);
            }
            _logger.LogInformation("Universidade: {Total} horário(s) bloqueado(s) por ocupação em outro curso na escola {Escola}.", total, idEscola);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Universidade: não foi possível aplicar o AC automático na escola {Escola}. O processamento segue sem ele.", idEscola);
        }
        return professores;
    }

    public Task<List<OpcaoHorarioDto>> SelecionarOpcoesPorFiltros(int anoLetivo, string periodo, bool ativo, int idEscola, int idProfessor) =>
        _original.SelecionarOpcoesPorFiltros(anoLetivo, periodo, ativo, idEscola, idProfessor);

    public Task<List<OpcaoHorarioDto>> SelecionarTodasOpcoesProfessor(int anoLetivo, string periodo, int ativo, int idEscola) =>
        _original.SelecionarTodasOpcoesProfessor(anoLetivo, periodo, ativo, idEscola);
}
