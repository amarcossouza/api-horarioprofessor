# Fluxo Atual do Motor de Distribuição de Horários

Documentação técnica do fluxo real encontrado no código-fonte.

**Criado em:** 31/08/2026  
**Baseado em:** `PrincipalExecucaoController.cs`, `PrincipalService.cs`, `AppState.cs`, `WorkflowManager.cs`  
**Bloco 1 — Etapa 0 do Planejamento:** Entender antes de mexer

---

## 1. Arquitetura do Motor (como está hoje)

```
Frontend (MVC:5051)
    ↓ HTTP GET/POST
PrincipalExecucaoController (API:5000)
    ↓
PrincipalService.Executar()
    ↓
AppState.Main()
    ↓
WorkflowManager.Execute()  ← encadeia as rotinas na ordem definida
    ↓
Rotinas de distribuição, reagendamento, realocação, trocas
    ↓
5 matrizes preenchidas (Seg/Ter/Qua/Qui/Sex) → retornadas como HorariosDto
```

---

## 2. Endpoints Principais da API

### `GET /api/PrincipalExecucao/horarios`
- **Parâmetros:** `idEscola`, `periodo`, `ativo`, `anoLetivo`, `QuantidadeHorarios`
- **⚠️ EFEITO COLATERAL:** Este GET **modifica o banco** — apaga opções e copia antes de processar
- **Fluxo:**
  1. Carrega lista de professores do banco
  2. `ApagarOpcoesPorFiltrosAsync` — apaga registros da tabela de opções
  3. `CopiarOpcoesParaProcessamentoAsync` — copia opção 1 para processamento
  4. `PrincipalService.Executar()` — executa o motor completo
  5. Monta relatório de professores não distribuídos
  6. Retorna `HorariosDto` com 5 matrizes + turmas + pendências

### `GET /api/PrincipalExecucao/horariosFechamento`
- **Mesmos parâmetros** + `Fechamento`
- **Fluxo adicional:**
  1. Processa primeiro com `Fechamento = 0` (distribuição normal)
  2. Coleta professores não distribuídos (`necessidadesProfessores`)
  3. Reprocessa com `Fechamento = 1` (tenta inserir os faltantes)
  4. Loop adicional de até 2 iterações tentando resolver pendências
  5. Verifica quais professores quebraram regras de bloqueio
  6. Se zerou pendências **e** não há células vazias → salva automaticamente no banco

### `POST /api/PrincipalExecucao/executar`
- Endpoint direto sem tratamento de opções — chama `PrincipalService.Executar`

### `POST /api/PrincipalExecucao/SalvarMatrizComoJson`
- Salva as 5 matrizes como JSON na tabela `hprof_horariodistribuicaojson`

### `GET /api/PrincipalExecucao/RecuperarHorarios`
- Recupera matrizes previamente salvas em JSON

---

## 3. `PrincipalService.cs` — O Orquestrador

Classe simples que delega tudo para `AppState.Main()`:

```csharp
public async Task Executar(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2, QuantidadeHorarios)
{
    await _appState.Main(idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2, QuantidadeHorarios);
}
```

---

## 4. `AppState.cs` — O Coração do Sistema

### 4.1 Dados de Estado (propriedades principais)

| Propriedade | Tipo | Descrição |
|---|---|---|
| `matrizHorariosSegunda..Sexta` | `string[,]` | 5 matrizes [linhas × turmas] com nome do professor |
| `Turmas` | `List<string>` | Nomes das turmas (ex: "6W","6X"...) |
| `Professores` | `Dictionary<string, Tupla>` | Dados de cada professor (aulas, turmas, bloqueios, etc.) |
| `movimentacoes` | `List<MovimentacaoTurma>` | Controle de aulas distribuídas vs programadas |
| `necessidadesProfessores` | `Dictionary<string, NecessidadeProfessor>` | Professores que faltam distribuir |
| `materiaAulasPorAno` | `Dictionary<string, Dictionary<int,int>>` | Qtd de aulas por matéria/ano escolar |
| `Fechamento` | `int` | 0 = normal, 1 = fechamento especial |
| `ListaMovimentacaoProfessorFechamento` | `List<>` | Registro das trocas feitas no fechamento |
| `QuantidadeHorarios` | `int` | Número de linhas por matriz (ex: 6) |

### 4.2 Tupla do Professor

```csharp
(int AulasPorTurma, 
 Dictionary<string, List<string>> DistribuicaoTurmas,  // Ex: {"MAT": ["6W","6X"]}
 string DiasNaoTrabalha,           // Ex: "SEG1,TER5,TER6"
 string NomeAtividadeDescolar, 
 bool DobrarAula,                  // Permite dobradinha?
 string Restricoes, 
 string AtividadeAtpc, 
 string DiasAtpc, 
 bool Prioridade)
```

### 4.3 ⚠️ Construtor com Chamada Assíncrona

```csharp
public AppState(IServiceScopeFactory scopeFactory, ...)
{
    // PROBLEMA: Chama Main() no construtor, que é async
    Main(idEscola, periodo, ativo, anoLetivo, ...);
    // Valores hardcoded: idEscola=1, periodo="Manha", ativo=1, anoLetivo=2024
}
```

**Risco:** `Main()` é `async Task` mas é chamado sem `await` no construtor → pode causar problemas de concorrência.

---

## 5. `AppState.Main()` — Pipeline Completo

### Fase de Preparação (antes do Workflow)

```
1. CarregarTurmasAsync()     → lê turmas do banco → preenche Turmas[] e ListTurmasProf[]
2. ObterMateriaAulasPorAno() → lê regras de qtd aulas/matéria → preenche materiaAulasPorAno
3. InicializarMatriz()       → cria matrizes vazias [QuantidadeHorarios × Turmas.Count]
4. InicializarProfessores()  → lê professores do banco via CSharpScript (geração dinâmica de código!)
5. InicializarMovimentacoes()→ calcula aulas necessárias por professor/turma
6. Remove professores REG/POL (polivalentes) das movimentações
```

### ⚠️ InicializarProfessores — Código Dinâmico

O sistema **gera código C# como string** e **executa em tempo de execução** com `CSharpScript`:

```csharp
var codigoProfessores = new StringBuilder();
codigoProfessores.AppendLine("var Professores = new Dictionary<...>();");
foreach (var prof in listaProfessores)
{
    codigoProfessores.AppendLine($"Professores.Add(\"{prof.Nome.ToUpper()}\", ...);");
}
var script = CSharpScript.RunAsync(codigoProfessores.ToString(), options).Result;
Professores = script.GetVariable("Professores").Value as Dictionary<...>;
```

**Risco:** Overhead de compilação em tempo de execução. Possível substituir por inicialização direta.

---

## 6. Pipeline do WorkflowManager (Fechamento = 0)

O `WorkflowManager` executa as seguintes tarefas **em sequência**:

```
Etapa │ Nome                                          │ Método
──────┼───────────────────────────────────────────────┼────────────────────────────────
  1   │ Distribuir Horários                           │ DistribuirHorarios()
  2   │ Reagendar Horários Não Distribuídos           │ ReagendarHorariosNaoDistribuidos()
  3   │ Identificar Turmas Sem Agendamento            │ IdentificarTurmasSemAgendamentoCompleto()
  4   │ Processar Realocações por Coluna              │ ProcessarRealoacoesColuna()
  5   │ Identificar Turmas Sem Agendamento (2ª vez)   │ IdentificarTurmasSemAgendamentoCompleto()
  6   │ Processar Realocações                         │ ProcessarRealoacoes()
  7   │ Processar Realocações por Coluna (2ª vez)     │ ProcessarRealoacoesColuna()
  8   │ Reagendar Horários Não Distribuídos (2ª vez)  │ ReagendarHorariosNaoDistribuidos()
  9   │ Troca entre Três Professores                  │ trocaTresProf.RealizarTrocas...()
 10   │ Processar Realocações                         │ ProcessarRealoacoes()
 11   │ Inserir Professor Vertical e Horizontal       │ IncluirProfessorVertical_MoverHorizontal()
 12   │ Processar Realocações por Coluna (3ª vez)     │ ProcessarRealoacoesColuna()
```

### Pipeline Fechamento = 1

```
Etapa │ Nome                                          │ Método
──────┼───────────────────────────────────────────────┼────────────────────────────────
  1   │ Inserir Professor Vertical e Horizontal       │ IncluirProfessorVertical_MoverHorizontal()
```

---

## 7. Detalhamento das Rotinas Principais

### 7.1 `DistribuirHorarios()`
- Ordena professores alfabeticamente, depois por aulas ímpares, depois por quantidade
- Para cada professor, tenta alocar nas matrizes de Seg a Sex
- Se `DobrarAula = true`, processa 2 vezes antes (dobradinha)
- Usa `ValidadorDeAgendamento` para verificar bloqueios

### 7.2 `ReagendarHorariosNaoDistribuidos()`
- Varre `necessidadesProfessores` (quem faltou distribuir)
- Procura horários vagos em todas as matrizes
- Aloca se passar nas validações

### 7.3 `ProcessarRealoacoes()`
- Contabiliza aulas agendadas vs necessárias
- Gera `necessidadesProfessores` atualizado
- Loop de até 100 iterações chamando `TentarRemoverERealocarGlobal()`
- Remove professor de uma célula e coloca outro, tentando realocar o removido em vaga vazia

### 7.4 `ProcessarRealoacoesColuna()`
- Usa `HorarioRealocacaoPorTurma` para trocas mais complexas por coluna
- Máximo de 1 iteração

### 7.5 `IncluirProfessorVertical_MoverHorizontal()`
- Identifica professores que precisam entrar
- Gera possibilidades de troca (vertical + horizontal)
- Simula em matrizes de cópia antes de aplicar
- Valida bloqueios de opções
- Aplica a primeira troca válida encontrada
- Até 10 iterações

### 7.6 `UnirProfessoresAgendadoDeHorarios()`
- Identifica professores que aparecem na mesma coluna mas em linhas não consecutivas ("sai e volta")
- Tenta trocar para juntar as aulas consecutivamente (dobradinha)
- Até 100 iterações

### 7.7 `ResolverSairVoltaAula_MoverHorizontal()`
- Similar ao anterior mas com trocas horizontais entre matrizes diferentes

---

## 8. Validações Encontradas

| Validação | Método | Descrição |
|---|---|---|
| Professor no mesmo horário | `IsProfessorAgendadoNoHorario()` | Mesmo professor em 2 turmas no mesmo slot |
| Professor na mesma turma | `IsProfessorAgendadoNaTurma()` | Máximo 2 aulas por turma por dia |
| Horário disponível | `IsHorarioDisponivel()` | Célula vazia na matriz |
| Aulas restantes | `ProfessorTemAulasRestantes()` | Controle via movimentações |
| Limite por coluna | `PodeAgendarProfessorNaColuna()` | Máx 2 vezes na mesma coluna |
| Capacidade por turma | `VerificarCapacidadeAulasPorTurma()` | Total em todas as matrizes vs `materiaAulasPorAno` |
| Bloqueios de opção | `ValidadorDeAgendamento.ValidarAgendamento()` | Verifica `DiasNaoTrabalha` do professor |

---

## 9. Regras de Negócio Identificadas no Código

| # | Regra | Local |
|---|---|---|
| R1 | Professor não pode dar aula em 2 turmas no mesmo horário | `IsProfessorAgendadoNoHorario` |
| R2 | Máximo 2 aulas do mesmo professor na mesma turma por dia | `IsProfessorAgendadoNaTurma`, `PodeAgendarProfessorNaColuna` |
| R3 | Respeitar bloqueios de dia/horário (opções do professor) | `ValidadorDeAgendamento` |
| R4 | Quantidade de aulas por matéria/ano escolar controlada | `VerificarCapacidadeAulasPorTurma`, `materiaAulasPorAno` |
| R5 | Dobradinha (2 aulas consecutivas) quando `DobrarAula = true` | `DistribuirHorarios`, `ProcessarTurmasDoProfessor` |
| R6 | Professores POL/REG removidos antes do processamento | Filtro no `Main()` |
| R7 | Salva automaticamente se distribuição completa sem pendências | Final do `Main()` |
| R8 | Limite de iterações em loops de realocação (20-100) | Diversos métodos |

---

## 10. Tabelas do Banco Usadas pelo Motor

| Tabela | Operação | Usado por |
|---|---|---|
| `hprof_cadastroprofessor` | SELECT | `ObterListaProfessorAtribuidoTurmaAsync`, `AtualizarQuantidadeOpcoesAsync` |
| `hprof_opcaohorario` | SELECT | `ObterOpcoesPorProfessoresAsync` |
| `hprof_opcaohorarioprocessamento` | DELETE + INSERT | `ApagarOpcoesPorFiltrosAsync`, `CopiarOpcoesParaProcessamentoAsync` |
| `hprof_horariodistribuicaojson` | INSERT + SELECT | `SalvarMatrizComoJson`, `RecuperarHorarios` |
| `hprof_turmapadraoescola` | SELECT | `CarregarTurmasAsync` via `TurmaPadraoEscolaService` |
| `hprof_materiaaulasporano` | SELECT | `ObterMateriaAulasPorAno` via `GetMaterialAulasPorAno` |

---

## 11. Problemas e Débitos Técnicos Identificados

| # | Problema | Severidade | Localização |
|---|---|---|---|
| 1 | **GET modifica banco** — apaga e copia registros | 🔴 Alta | `GetHorarios`, `GetHorariosFechamento` |
| 2 | **`Main()` chamado no construtor** sem await | 🔴 Alta | `AppState` construtor |
| 3 | **CSharpScript para montar professores** — overhead | 🟡 Média | `InicializarProfessores` |
| 4 | **Valores hardcoded** no construtor (idEscola=1, 2024) | 🟡 Média | `AppState` construtor |
| 5 | **Código de debug com nomes reais** (LUIS, ELENICE, etc.) | 🟡 Média | Diversos métodos |
| 6 | **Blocos comentados extensos** — versões antigas | 🟡 Média | `GetHorarios`, `Main()` |
| 7 | **`Parallel.For` sem lock** em `ConvertMatrixToList` | 🟡 Média | `ConvertMatrixToList` |
| 8 | **`Task.Delay(500)`** entre operações de banco | 🟡 Média | `GetHorarios` |
| 9 | **AppState como classe com estado mutável** injetada no DI | 🟡 Média | Arquitetura geral |
| 10 | **Sem transações** — apaga opções antes de copiar | 🔴 Alta | `GetHorarios` |

---

## 12. Próximo Passo

> ✅ **BLOCO 1 CONCLUÍDO** — Fluxo atual documentado  
> Aguardando autorização para iniciar o **Bloco 2**: Mapear regras de negócio em detalhe (`docs/REGRAS_NEGOCIO.md`)

---

*Documento gerado pela análise direta do código-fonte em 31/08/2026*
