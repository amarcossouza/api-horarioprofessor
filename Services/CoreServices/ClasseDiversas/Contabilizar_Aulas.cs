using API_HorarioProfessor.Services.CoreServices.Classes;

namespace API_HorarioProfessor.Services.CoreServices.ClasseDiversas
{
    public class Contabilizar_Aulas
    {
        private readonly AppState _appState;

        public Contabilizar_Aulas(AppState appState)
        {
            _appState = appState; // Injetando o AppState
        }

        public List<ContagemAulasProfessor> ProcessarMatrizesEContabilizarAulas(string[][,] matrizes)
        {
            List<ContagemAulasProfessor> contagens = new List<ContagemAulasProfessor>();

            var ContarKATIA = 0;
            foreach (var matriz in matrizes)
            {
                for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
                {
                    string nomeTurma = GetTurmaNameByIndex(coluna);
                    Dictionary<string, int> contagemAulas = new Dictionary<string, int>();

                    for (int linha = 0; linha < matriz.GetLength(0); linha++)
                    {                        
                        string nomeProfessor = matriz[linha, coluna].Trim();
                        if (!string.IsNullOrWhiteSpace(nomeProfessor))
                        {
                            if (!contagemAulas.ContainsKey(nomeProfessor))
                                contagemAulas[nomeProfessor] = 0;
                            contagemAulas[nomeProfessor]++;
                        }
                    }

                    foreach (var item in contagemAulas)
                    {
                        if (_appState.Professores.TryGetValue(item.Key, out var professorInfo))
                        {


                            string siglaMateria = professorInfo.DistribuicaoTurmas.FirstOrDefault().Key; // Assume the first entry is the correct one
                            int totalAulasPorMateria = professorInfo.AulasPorTurma;
                            int aulasRestantes = totalAulasPorMateria - item.Value;

                            ContagemAulasProfessor contagemExistente = contagens.FirstOrDefault(c => c.NomeProfessor == item.Key && c.Turma == nomeTurma);
                            if (contagemExistente != null)
                            {
                                contagemExistente.QtdAulasAgendadas += item.Value;
                                contagemExistente.QtdAulasRestantes = Math.Max(totalAulasPorMateria - contagemExistente.QtdAulasAgendadas, 0);
                            }
                            else
                            {
                                if (item.Key == "ELENICE")
                                {
                                    var ACHEI = "ACHEI A ALESSANDRA";
                                    if (nomeTurma == "6X")
                                        ContarKATIA++;
                                }

                                contagens.Add(new ContagemAulasProfessor(
                                    item.Key,
                                    nomeTurma,
                                    item.Value,
                                    Math.Max(aulasRestantes, 0), // Ensure no negative numbers
                                    siglaMateria,
                                    0  // Example: Year level can be defined if necessary
                                ));
                            }
                        }
                    }
                }
            }

            return contagens;
        }

        public string GetTurmaNameByIndex(int index)
        {
            var turma = _appState.ListTurmasProf.FirstOrDefault(t => t.TurmaIndex == index);
            return turma != null ? turma.Nome : "Nome da Turma Não Encontrado";
        }



        public  List<ContagemAulasProfessor> ProcessarMatrizesEIdentificarTurmasFaltantes(string[][,] matrizes)
        {
            List<ContagemAulasProfessor> contagens = new List<ContagemAulasProfessor>();

            // Processar cada matriz
            foreach (var matriz in matrizes)
            {
                for (int coluna = 0; coluna < matriz.GetLength(1); coluna++)
                {
                    string nomeTurma = GetTurmaNameByIndex(coluna);
                    Dictionary<string, int> contagemAulas = new Dictionary<string, int>();

                    // Contar aulas por professor na coluna específica
                    for (int linha = 0; linha < matriz.GetLength(0); linha++)
                    {
                        string nomeProfessor = matriz[linha, coluna].Trim();
                        if (!string.IsNullOrWhiteSpace(nomeProfessor))
                        {
                            if (!contagemAulas.ContainsKey(nomeProfessor))
                                contagemAulas[nomeProfessor] = 0;
                            contagemAulas[nomeProfessor]++;
                        }
                    }

                    // Comparar com a distribuição esperada
                    foreach (var item in contagemAulas)
                    {
                        if (_appState.Professores.TryGetValue(item.Key, out var professorInfo))
                        {
                            foreach (var turmaMateria in professorInfo.DistribuicaoTurmas)
                            {
                                if (turmaMateria.Value.Contains(nomeTurma))
                                {
                                    string siglaMateria = turmaMateria.Key;
                                    int totalAulasPorTurma = professorInfo.AulasPorTurma;

                                    ContagemAulasProfessor contagemExistente = contagens.FirstOrDefault(c => c.NomeProfessor == item.Key && c.Turma == nomeTurma && c.SiglaMateria == siglaMateria);
                                    if (contagemExistente != null)
                                    {
                                        contagemExistente.QtdAulasAgendadas += item.Value;
                                        contagemExistente.QtdAulasRestantes = Math.Max(totalAulasPorTurma - contagemExistente.QtdAulasAgendadas, 0);
                                    }
                                    else
                                    {
                                        contagens.Add(new ContagemAulasProfessor(
                                            item.Key,
                                            nomeTurma,
                                            item.Value,
                                            Math.Max(totalAulasPorTurma - item.Value, 0), // Garante que não haja números negativos
                                            siglaMateria,
                                            0  // Exemplo: o nível do ano pode ser definido se necessário
                                        ));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return contagens;
        }





        public void ImprimirRelatorioDeContagemDeAulas(List<ContagemAulasProfessor> contagens)
        {
            Console.WriteLine("######### Relatório de Contagem de Aulas por Professor e Turma #########");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($"{"Professor",-20} {"Turma",-10} {"Sigla Materia",-15} {"Aulas Agendadas",-15} {"Aulas Restantes",-15} {"Ano Escolar",-12}");
            Console.WriteLine(new string('-', 85)); // Linha divisória

            int totalAulasAgendadas = 0;
            int totalAulasRestantes = 0;

            // Ordenando a lista de contagens pelo nome do professor antes de imprimir
            var sortedContagens = contagens.OrderBy(c => c.NomeProfessor).ToList();

            foreach (var contagem in sortedContagens)
            {
                // Calcula as aulas restantes de maneira segura, assegurando que não seja negativo
                int totalAulasPorMateria = _appState.materiaAulasPorAno[contagem.SiglaMateria].GetValueOrDefault(contagem.AnoEscolar, 0);
                int aulasRestantes = Math.Max(totalAulasPorMateria - contagem.QtdAulasAgendadas, 0);

                Console.WriteLine($"{contagem.NomeProfessor,-20} {contagem.Turma,-10} {contagem.SiglaMateria,-15} {contagem.QtdAulasAgendadas,-15} {aulasRestantes,-15} {contagem.AnoEscolar,-12}");

                totalAulasAgendadas += contagem.QtdAulasAgendadas;
                totalAulasRestantes += aulasRestantes;
            }

            Console.WriteLine(new string('-', 85));
            Console.WriteLine($"Total de Aulas Agendadas: {totalAulasAgendadas}");
            Console.WriteLine($"Total de Aulas Restantes: {totalAulasRestantes}");
            Console.WriteLine("Fim do Relatório.");
            Console.WriteLine();
        }




        public List<ContagemAulasProfessor> PreencherTabelaContagemAulasProfessor()
        {
            List<ContagemAulasProfessor> contagens = new List<ContagemAulasProfessor>();

            foreach (var professor in _appState.Professores)
            {
                string nomeProfessor = professor.Key;
                var info = professor.Value;

                int aulasPorTurma = info.AulasPorTurma;
                var distribuicaoTurmas = info.DistribuicaoTurmas;

                foreach (var materia in distribuicaoTurmas)
                {
                    string siglaMateria = materia.Key;
                    var turmas = materia.Value;

                    foreach (string turma in turmas)
                    {
                        // Extrair o ano escolar do nome da turma, assumindo que ele está no final do nome da turma
                        int anoEscolar = turma.Any(char.IsDigit) ? int.Parse(turma.Where(char.IsDigit).Last().ToString()) : 0;

                        // Determinar a quantidade de aulas agendadas com base na sigla e no ano escolar
                        int qtdAulasAgendadas = 0;
                        if (_appState.materiaAulasPorAno.ContainsKey(siglaMateria))
                        {
                            if (_appState.materiaAulasPorAno[siglaMateria].ContainsKey(anoEscolar))
                            {
                                qtdAulasAgendadas = _appState.materiaAulasPorAno[siglaMateria][anoEscolar];
                            }
                            else if (_appState.materiaAulasPorAno[siglaMateria].ContainsKey(0))
                            {
                                qtdAulasAgendadas = _appState.materiaAulasPorAno[siglaMateria][0];
                            }
                        }

                        // Adicionar ou atualizar a entrada na lista
                        ContagemAulasProfessor contagemExistente = contagens.FirstOrDefault(c => c.NomeProfessor == nomeProfessor && c.Turma == turma);
                        if (contagemExistente != null)
                        {
                            contagemExistente.QtdAulasAgendadas += qtdAulasAgendadas;
                            contagemExistente.QtdAulasRestantes = Math.Max(aulasPorTurma - contagemExistente.QtdAulasAgendadas, 0);
                        }
                        else
                        {
                            contagens.Add(new ContagemAulasProfessor(
                                nomeProfessor,
                                turma,
                                qtdAulasAgendadas,
                                Math.Max(aulasPorTurma - qtdAulasAgendadas, 0), // Ensure no negative numbers
                                siglaMateria,
                                anoEscolar  // Use the extracted school year
                            ));
                        }
                    }
                }
            }

            return contagens;
        }


        // corrigindo o codigo 18/10/2024 é novo este codigo 
        //
        //
        //////////////////////public  List<ContagemAulasProfessor> IdentificarAgendamentosFaltantes(
        //////////////////////    List<ContagemAulasProfessor> todasAulasProfessor,
        //////////////////////    List<ContagemAulasProfessor> aulasAgendadas)
        //////////////////////{
        //////////////////////    List<ContagemAulasProfessor> agendamentosFaltantes = new List<ContagemAulasProfessor>();

        //////////////////////    // Iterar sobre a lista completa para garantir que todas as aulas necessárias sejam consideradas
        //////////////////////    foreach (var aulaTotal in todasAulasProfessor)
        //////////////////////    {
        //////////////////////        // Encontrar o agendamento correspondente na lista de aulas agendadas
        //////////////////////        var agendamentoCorrespondente = aulasAgendadas.FirstOrDefault(a =>
        //////////////////////            a.NomeProfessor == aulaTotal.NomeProfessor &&
        //////////////////////            a.Turma == aulaTotal.Turma);

        //////////////////////        // Calcular a quantidade de aulas que ainda precisam ser agendadas
        //////////////////////        int aulasFaltantes = aulaTotal.QtdAulasAgendadas - (agendamentoCorrespondente?.QtdAulasAgendadas ?? 0);

        //////////////////////        // Se as aulas faltantes forem positivas, adiciona à lista de agendamentos faltantes
        //////////////////////        if (aulasFaltantes > 0)
        //////////////////////        {
        //////////////////////            agendamentosFaltantes.Add(new ContagemAulasProfessor(
        //////////////////////                aulaTotal.NomeProfessor,
        //////////////////////                aulaTotal.Turma,
        //////////////////////                aulasFaltantes,
        //////////////////////                aulaTotal.QtdAulasRestantes + aulasFaltantes,
        //////////////////////                aulaTotal.SiglaMateria.Trim(),
        //////////////////////                aulaTotal.AnoEscolar
        //////////////////////            ));
        //////////////////////        }
        //////////////////////    }

        //////////////////////    return agendamentosFaltantes;
        //////////////////////}


        public List<ContagemAulasProfessor> IdentificarAgendamentosFaltantes(
            List<ContagemAulasProfessor> todasAulasProfessor,
            List<ContagemAulasProfessor> aulasAgendadas)
        {
            List<ContagemAulasProfessor> agendamentosFaltantes = new List<ContagemAulasProfessor>();

            // Primeiro, percorrer a lista completa para garantir que todas as aulas necessárias sejam consideradas
            foreach (var aulaTotal in todasAulasProfessor)
            {
                // Encontrar o agendamento correspondente na lista de aulas agendadas
                var agendamentoCorrespondente = aulasAgendadas.FirstOrDefault(a =>
                    a.NomeProfessor == aulaTotal.NomeProfessor &&
                    a.Turma == aulaTotal.Turma);

                // Calcular a quantidade de aulas que ainda precisam ser agendadas
                int aulasFaltantes = aulaTotal.QtdAulasAgendadas - (agendamentoCorrespondente?.QtdAulasAgendadas ?? 0);

                // Se as aulas faltantes forem positivas, adiciona à lista de agendamentos faltantes
                if (aulasFaltantes > 0)
                {
                    agendamentosFaltantes.Add(new ContagemAulasProfessor(
                        aulaTotal.NomeProfessor,
                        aulaTotal.Turma,
                        aulasFaltantes,
                        aulaTotal.QtdAulasRestantes + aulasFaltantes,
                        aulaTotal.SiglaMateria.Trim(),
                        aulaTotal.AnoEscolar
                    ));
                }
            }

            // Agora, percorrer a lista de aulas agendadas para garantir que não há agendamentos não cobertos em 'todasAulasProfessor'
            foreach (var aulaAgendada in aulasAgendadas)
            {
                // Verificar se o agendamento está faltando na lista completa de todas as aulas do professor
                var aulaCorrespondente = todasAulasProfessor.FirstOrDefault(a =>
                    a.NomeProfessor == aulaAgendada.NomeProfessor &&
                    a.Turma == aulaAgendada.Turma);

                // Se a aula correspondente não existir na lista completa, significa que está faltando na programação
                if (aulaCorrespondente == null)
                {
                    agendamentosFaltantes.Add(new ContagemAulasProfessor(
                        aulaAgendada.NomeProfessor,
                        aulaAgendada.Turma,
                        aulaAgendada.QtdAulasAgendadas, // Todas as aulas estão faltando
                        aulaAgendada.QtdAulasRestantes,
                        aulaAgendada.SiglaMateria.Trim(),
                        aulaAgendada.AnoEscolar
                    ));
                }
            }

            return agendamentosFaltantes;
        }





        public Dictionary<string, NecessidadeProfessor> PreencherNecessidadesProfessores(List<ContagemAulasProfessor> agendamentosFaltantes)
        {
            Dictionary<string, NecessidadeProfessor> necessidadesProfessoresLocal = new Dictionary<string, NecessidadeProfessor>();

            foreach (var agendamento in agendamentosFaltantes)
            {
                string chave = $"{agendamento.NomeProfessor}-{agendamento.Turma.Trim()}";
                if (necessidadesProfessoresLocal.TryGetValue(chave, out NecessidadeProfessor necessidadeExistente))
                {
                    // Se a entrada já existe, atualiza a quantidade de aulas necessárias
                    necessidadeExistente.QtdAula += agendamento.QtdAulasAgendadas;
                }
                else
                {
                    // Se não existe, cria uma nova entrada
                    necessidadesProfessoresLocal.Add(chave, new NecessidadeProfessor(
                        agendamento.NomeProfessor,
                        agendamento.Turma,
                        agendamento.SiglaMateria,
                        agendamento.AnoEscolar,
                        agendamento.QtdAulasAgendadas,
                        0, // DiaDaSemana como 0 por default, assumindo que você ajustará isso conforme a necessidade
                        0, // Horario como 0 por default
                        0,  // TentativasDeRealocacao inicializadas como 0
                        _appState
                    ));
                }
            }

            return necessidadesProfessoresLocal;
        }

    }



}
