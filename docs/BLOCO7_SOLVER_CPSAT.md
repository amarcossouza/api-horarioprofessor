# Bloco 7 — Motor CP-SAT (Google OR-Tools)

**Data:** 31/08/2026  
**Status:** ✅ Concluído e Integrado (Backend + Frontend + UI)

---

## 🎯 O Que É

O **Motor CP-SAT (Constraint Programming - Satisfiability)** utiliza a biblioteca **Google OR-Tools (v9.10)** integrada em C# para resolver e otimizar alocações de horários escolares de forma matemática exata, sem heurísticas cegas.

Ele avalia o espaço combinatório em busca de fechamento completo da grade, respeitando todas as regras obrigatórias (*Hard Constraints*) e otimizando preferências pedagógicas (*Soft Constraints*).

---

## 📐 Modelagem Matemática

### 1. Variáveis de Decisão
$$X_{p,t,d,s} \in \{0, 1\}$$
Onde $X_{p,t,d,s} = 1$ indica que o professor $p$ leciona na turma $t$, no dia $d$, no slot/aula $s$.

### 2. Restrições Rígidas (*Hard Constraints*)
* **R1 — Sem Conflito de Horário:** Um professor só pode estar em no máximo 1 turma por slot:
  $$\sum_{t} X_{p,t,d,s} \le 1 \quad \forall p, d, s$$
* **R2 — Carga Máxima por Turma/Dia:** No máximo 2 aulas do professor $p$ na turma $t$ por dia:
  $$\sum_{s} X_{p,t,d,s} \le 2 \quad \forall p, t, d$$
* **R3 — Bloqueios / Indisponibilidade:** $X_{p,t,d,s} = 0$ para qualquer $(d,s)$ onde o professor possui bloqueio cadastrado.
* **Exclusividade do Slot:** No máximo 1 professor por turma por slot:
  $$\sum_{p} X_{p,t,d,s} \le 1 \quad \forall t, d, s$$
* **Posições Protegidas (Congeladas):** Células já preenchidas e travadas pelo usuário são preservadas fixamente no modelo.

### 3. Função Objetivo (*Soft Constraints*)
$$\text{Maximizar} \quad 10 \cdot \sum X_{p,t,d,s} + \text{peso}_{\text{dobradinha}} \cdot \sum \text{BonusConsecutivo}$$
* Maximiza a quantidade de aulas alocadas (busca fechar todas as pendências).
* Bonifica a formação de dobradinhas (aulas consecutivas na mesma turma).
* Penaliza "sai-e-volta" do professor na mesma turma.

---

## 📁 Arquivos Criados e Integrados

### No microsserviço `api-cenarios`:
* `api-cenarios.csproj` — Adicionado pacote `Google.OrTools` (9.10.4067).
* `Models/SolverModels.cs` — Modelos completos de entrada/saída (`SolverInput`, `SolverOutput`, `ProfessorNecessidade`, `BloqueioSolver`, `Movimentacao`, `EstatisticasSolver`).
* `Services/SolverCpSatService.cs` — Motor de otimização CP-SAT (~560 linhas).
* `Controllers/SimulacaoController.cs` — 3 endpoints REST: `/api/simulacao/resolver`, `/api/simulacao/resolver-versao`, `/api/simulacao/status`.
* `Program.cs` — Injeção de dependência do `SolverCpSatService`.

### No frontend `HorarioProfessor2025_v2`:
* `Services/ICenarioApiService.cs` — Adicionados métodos `ResolverGradeAsync` e `ResolverVersaoAsync` + DTOs de simulação.
* `Services/CenarioApiService.cs` — Implementação das chamadas HTTP via `HttpClient`.
* `Controllers/CenarioController.cs` — Endpoints `/Cenario/ResolverGradeAtual`, `/Cenario/AplicarSolucaoSolver`, `/Cenario/ResolverVersao`.
* `Views/Cenario/_PainelCenarios.cshtml` — Adicionado botão `⚡ Otimizar (CP-SAT)` e modal interativo `#modalSolverCpSat`.
* `wwwroot/js/cenarios.js` — Funções `abrirModalSolverCpSat()`, `executarSolverCpSat()`, `exibirResultadoSolver()`, `aplicarSolucaoSolverNaGrade()`.

---

## 🔌 Endpoints REST Disponíveis

| Serviço | Método | Rota | Função |
| :--- | :--- | :--- | :--- |
| `api-cenarios` (5010) | `POST` | `/api/simulacao/resolver` | Executa o CP-SAT diretamente com o payload JSON |
| `api-cenarios` (5010) | `POST` | `/api/simulacao/resolver-versao` | Resolve a partir do snapshot de uma versão salva no banco |
| `api-cenarios` (5010) | `POST` | `/api/simulacao/gerar-alternativas` | Gera 3 alternativas completas de fechamento (100% preenchidas) |
| `api-cenarios` (5010) | `POST` | `/api/simulacao/adotar-alternativa` | Adota alternativa oficial, salva versão e grava regras de concessão |
| `api-cenarios` (5010) | `GET` | `/api/simulacao/status` | Status operacional do solver e versão do OR-Tools |
| `HorarioProfessorMVC` (5051) | `POST` | `/Cenario/Gerar3Alternativas` | Aciona o motor de 3 alternativas cruzando contratos e concessões |
| `HorarioProfessorMVC` (5051) | `POST` | `/Cenario/AdotarAlternativa` | Atualiza a sessão da tela e persiste a grade oficial adotada |

---

## 🌟 Motor de 3 Alternativas de Fechamento (Tomada de Decisão do Diretor)

O motor CP-SAT entrega **3 Alternativas Estratégicas Completas (100% Fechadas — 510/510 Aulas)**:

1. **Opção 1 — Ajuste Cirúrgico Mínimo (Verde):**
   * Preserva 98% da grade atual e remaneja apenas o estritamente necessário para fechar todos os buracos.
   * Menor impacto na rotina dos professores.
2. **Opção 2 — Qualidade Pedagógica / Dobradinhas (Azul):**
   * Prioriza aulas duplas consecutivas para disciplinas principais (Matemática, Português, Ciências).
   * Melhora o planejamento didático em sala de aula.
3. **Opção 3 — Equilíbrio Docente / Zero Janelas (Ciano):**
   * Elimina horários vagos (janelas) entre aulas e distribui a carga semanal uniformemente.

### ⚙️ Concessões Dirigidas pela Direção (Opcional)
* O diretor pode indicar no modal se deseja flexibilizar algum professor (ex: `VEIGA-EFI`, `DOSILIA-MAT`) em um dia específico (ex: `Sexta-feira`).
* O motor aplica essa diretriz nas 3 opções e registra a justificativa formal em `hprof_cenarioregra` para auditoria permanente.

---

## 📊 Status dos Blocos

| Bloco | Descrição | Status |
| :--- | :--- | :--- |
| 1 | Documentação do fluxo legado e arquitetura | ✅ Concluído |
| 2 | Scripts Flyway SQL (5 tabelas `hprof_*`) | ✅ Executado no MySQL |
| 3 | Microsserviço `api-cenarios` (CRUD e persistência) | ✅ Concluído |
| 4 | Integração Frontend ↔ Microsserviço | ✅ Concluído |
| 5 | Telas e Modais de Cenários (UI) | ✅ Concluído |
| 6 | Validador Canônico (R1–R6 com Cruzamento Real de Contratos) | ✅ Concluído |
| 7 | Motor CP-SAT com Google OR-Tools (3 Alternativas de Fechamento) | ✅ Concluído |
| 8 | Comparador de Versões (Diff Visual e de Métricas) | ✅ Concluído |
| 9 | Publicação Oficial & Concessões Autorizadas | ✅ Concluído |

