# Diário de Desenvolvimento — Horário Professor

Registro cronológico de cada atividade realizada no projeto.

---

## 31/08/2026

### Sessão 1 — Análise Inicial e Planejamento

**Hora:** Início da sessão  
**Atividade:** Análise completa dos dois projetos e criação da estratégia

**O que foi feito:**

1. ✅ Leitura completa do `PLANEJAMENTO_NOVO_FECHAMENTO.md` (documento principal — 20 seções)
2. ✅ Análise da estrutura do **Frontend** (`HorarioProfessor2025_v2`)
   - Identificados 8+ controllers
   - Mapeado fluxo principal: `HorarioController.ObterHorarios` → API → grade em sessão
   - Identificado acesso direto ao MySQL via Dapper (risco de dupla dependência)
   - Mapeadas tabelas acessadas diretamente pelo frontend
3. ✅ Análise da estrutura do **Backend** (`api-horarioprofessor`)
   - Identificados 11 controllers
   - Mapeada arquitetura: Controllers → Services → CoreServices → AppState
   - Identificado motor legado: `PrincipalService` → `AppState.Main` → `WorkflowManager`
4. ✅ Criação do arquivo `ESTRATEGIA_DESENVOLVIMENTO.md`
   - 4 fases definidas com passos concretos
   - Sequência de trabalho imediata estabelecida
   - Riscos e decisões pendentes documentados
5. ✅ Criação deste arquivo `DIARIO_DESENVOLVIMENTO.md`

**Arquivos criados:**
- `docs/ESTRATEGIA_DESENVOLVIMENTO.md`
- `docs/DIARIO_DESENVOLVIMENTO.md`

**Próximo passo:** Iniciar Passo 1.3 — Ler e documentar o fluxo real do código  
**Arquivos a analisar:** `AppState.cs`, `PrincipalService.cs`, `WorkflowManager.cs`, `PrincipalExecucaoController.cs`

---

### Sessão 1 — Bloco 1: Documentação do Fluxo Atual

**Hora:** Continuação da sessão  
**Atividade:** Leitura e documentação completa do motor de distribuição

**Arquivos analisados (código-fonte lido na íntegra):**

1. ✅ `Controllers/PrincipalExecucaoController.cs` (~700 linhas)
   - 3 endpoints principais: `GetHorarios`, `GetHorariosFechamento`, `RecuperarHorarios`
   - Identificado: GET que modifica banco (apaga + copia opções)
   - Identificado: `Task.Delay(500)` entre operações de banco
   - Identificado: relatório de professores não distribuídos

2. ✅ `Services/CoreServices/PrincipalService.cs` (~20 linhas)
   - Simples delegador: chama `AppState.Main()`

3. ✅ `Services/CoreServices/AppState.cs` (~3500+ linhas)
   - **Coração do sistema** — contém todo o motor de distribuição
   - Pipeline de 12 etapas via WorkflowManager
   - Identificadas 8 regras de negócio codificadas
   - Identificados 10 problemas técnicos
   - Mapeadas 6 tabelas do banco usadas
   - `InicializarProfessores()` usa CSharpScript (geração dinâmica de código)
   - Construtor chama `Main()` async sem await

4. ✅ `Services/CoreServices/Sources/Gerenciamento/WorkflowManager.cs` (~25 linhas)
   - Lista de `TaskStep` executadas em sequência
   - Suporta condição por step (mas todas retornam true atualmente)

**Arquivo criado:**
- `docs/FLUXO_ATUAL.md` — 12 seções, pipeline completo documentado

**Descobertas-chave:**
- O motor tem 12 etapas sequenciais (distribuição → reagendamento → realocação → trocas)
- Usa matrizes `string[,]` de [horários × turmas] para cada dia da semana
- Professores carregados via geração dinâmica de código C# (CSharpScript)
- Fechamento especial (modo 1) tenta inserir os faltantes com trocas de 3 professores
- Salva automaticamente se distribuição ficar completa

**Status:** ✅ BLOCO 1 CONCLUÍDO  

---

### Sessão 1 — Bloco 2: Scripts SQL Flyway

**Atividade:** Criação dos 5 scripts SQL para as novas tabelas + configuração Flyway

**Arquivos criados:**

1. ✅ `flyway/flyway.conf` — configuração do Flyway (tabela de histórico `hprof_flyway_schema_history`)
2. ✅ `flyway/sql/V1__criar_tabela_cenario.sql` — identidade do cenário (escola/ano/período)
3. ✅ `flyway/sql/V2__criar_tabela_cenarioversao.sql` — versão imutável + snapshot JSON completo
4. ✅ `flyway/sql/V3__criar_tabela_cenarioregra.sql` — regras e exceções classificadas por versão
5. ✅ `flyway/sql/V4__criar_tabela_simulacao.sql` — trabalho de busca com estado e métricas
6. ✅ `flyway/sql/V5__criar_tabela_cenarioevento.sql` — trilha de auditoria imutável
7. ✅ `docs/BLOCO2_COMANDOS_FLYWAY.md` — instruções para execução manual

**Decisões de design:**
- `snapshot_json` em JSON nativo do MySQL (permite consultas futuras com JSON_EXTRACT)
- Controle otimista de concorrência via campo `revisao` na tabela cenário
- Foreign Keys com `ON DELETE RESTRICT` para proteger contra exclusão acidental
- Tabela de histórico Flyway própria (`hprof_flyway_schema_history`) — não interfere em outras apps
- `IF NOT EXISTS` em todos os CREATE TABLE — seguro para rodar mais de uma vez
- Charset `utf8mb4` em todas as tabelas

**Status:** ✅ BLOCO 2 CONCLUÍDO — AGUARDANDO EXECUÇÃO MANUAL  

---

### Sessão 1 — Decisão Arquitetural: Microsserviços

**Decisão:** Usuário optou por arquitetura de microsserviços em vez de expandir o monolito.

**Motivação:**
- Não mexer na API legada (porta 5000) que já funciona
- Motor novo (CP-SAT) isolado = teste sem medo
- Deploy independente de cada serviço
- Escala independente (simulação pesada não trava cadastros)

**Arquitetura definida:**

| Serviço | Porta | Projeto | Status |
|---------|-------|---------|--------|
| API Legada | 5000 | `api-horarioprofessor` | ✅ Existe — NÃO MEXER |
| API Cenários | 5010 | `api-cenarios` | 🆕 Criar agora |
| API Simulação | 5020 | `api-simulacao` | 🔮 Futuro (Fase 3) |
| Frontend MVC | 5051 | `HorarioProfessor2025_v2` | ✅ Existe |

**Documento criado:** `docs/ARQUITETURA_MICROSSERVICOS.md`

**Status:** ✅ ARQUITETURA DEFINIDA  

---

### Sessão 1 — Bloco 3: Projeto api-cenarios criado

**O que foi feito:**
- Criado projeto `C:\projkinix\api-cenarios\` — microsserviço independente
- .NET 8 Web API na porta 5010
- 5 Models C# espelhando as 5 tabelas SQL (hprof_cenario*)
- DbContext completo com mapeamento Fluent API
- DTOs para request/response
- Repository (acesso a dados)
- Service (lógica de negócio + auditoria automática)
- Controller com 7 endpoints REST + health check
- Swagger configurado
- CORS configurado para frontend (porta 5051)
- Controle de concorrência otimista (campo revisão)
- Hash SHA-256 no snapshot para integridade
- Soft delete (arquivar em vez de apagar)

**Arquivos criados (13 arquivos):**
```
api-cenarios/
├── api-cenarios.sln
├── api-cenarios.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── .gitignore
├── Properties/launchSettings.json
├── Models/ (5 arquivos)
├── Data/CenariosDbContext.cs
├── Dto/ (2 arquivos)
├── Repositories/CenarioRepository.cs
├── Services/CenarioService.cs
└── Controllers/CenariosController.cs
```

**Documentação criada:** `docs/BLOCO3_API_CENARIOS.md`

**Status:** ✅ BLOCO 3 CONCLUÍDO — AGUARDANDO TESTE  

---

### Sessão 1 — Bloco 4: Integração Frontend ↔ Microsserviço

**O que foi feito:**
- Criado `ICenarioApiService.cs` — interface + DTOs espelho do microsserviço
- Criado `CenarioApiService.cs` — implementação HttpClient (Named Client "ApiCenarios")
- Criado `CenarioController.cs` — controller MVC ponte entre JS e microsserviço

**Funcionalidades do CenarioController:**
1. `GET /Cenario/Listar` — lista cenários da escola
2. `GET /Cenario/Detalhe/{id}` — cenário com versões
3. `POST /Cenario/Criar` — cria cenário novo
4. `POST /Cenario/SalvarVersao` — captura Session["GradeHorarios"] → snapshot JSON → envia ao microsserviço
5. `POST /Cenario/RestaurarVersao` — busca versão do microsserviço → restaura na sessão
6. `GET /Cenario/Eventos/{id}` — timeline de auditoria

**Decisões técnicas:**
- Nenhum arquivo existente foi alterado
- CenarioController é independente do HorarioController
- Se api-cenarios estiver fora do ar, sistema funciona normalmente
- Named HttpClient com timeout de 30s

**Configurações manuais pendentes:**
- Adicionar `ApiCenarios:BaseUrl` no appsettings.Development.json
- Registrar HttpClient + DI no Program.cs
- Ver doc: `docs/BLOCO4_INTEGRACAO_FRONTEND.md`

**Documentação criada:** `docs/BLOCO4_INTEGRACAO_FRONTEND.md`

**Status:** ✅ BLOCO 4 CONCLUÍDO — AGUARDANDO CONFIGURAÇÃO MANUAL  

---

### Sessão 1 — Bloco 5: Tela de Cenários (UI)

**O que foi feito:**
- Criado `Views/Cenario/_PainelCenarios.cshtml` — Partial View com 3 modais Bootstrap 5
- Criado `wwwroot/js/cenarios.js` — JavaScript completo (~350 linhas)

**Funcionalidades visuais criadas:**
1. Botão "💾 Salvar Cenário" — modal com opção de criar novo ou salvar em existente
2. Botão "📄 Cenários" — modal com tabela filtrada, sub-tabela de versões
3. Botão "Restaurar" — confirmação SweetAlert2, busca snapshot, recarrega tela
4. Botão "Histórico" — timeline de auditoria com emojis por tipo de evento
5. Botão "Arquivar" — soft delete com confirmação

**Padrão seguido:** jQuery + Bootstrap 5 + SweetAlert2 (mesmo do projeto existente)

**Nenhum arquivo existente foi alterado** — tudo é aditivo:
- Partial View inserida via `@await Html.PartialAsync(...)`
- Script carregado via `<script src>`

**Configurações manuais pendentes:**
- Inserir partial view no Horarios.cshtml
- Adicionar script cenarios.js
- Ver doc: `docs/BLOCO5_TELA_CENARIOS.md`

**Documentação criada:** `docs/BLOCO5_TELA_CENARIOS.md`

**Status:** ✅ BLOCO 5 CONCLUÍDO — AGUARDANDO INTEGRAÇÃO MANUAL  
**Próximo passo:** Bloco 6 — Validador Canônico (verificação de conflitos e regras)

---

## 📊 Resumo Geral dos Blocos

| Bloco | Descrição | Arquivos | Status |
|-------|-----------|----------|--------|
| 1 | Documentação (fluxo, estratégia, diário) | 3 docs | ✅ Feito |
| 2 | Scripts SQL Flyway (5 tabelas) | 5 .sql + config | ✅ Executado no MySQL |
| 3 | Microsserviço api-cenarios | 13 arquivos C# | ✅ `dotnet run` |
| 4 | Integração frontend ↔ microsserviço | 3 .cs | ✅ Configurado e Ativo |
| 5 | Tela de cenários (UI) | 1 .cshtml + 1 .js | ✅ Inserido e Ativo |
| 6 | Validador Canônico | 4 novos + 5 alterados | ✅ Pronto |
| 7 | CP-SAT (OR-Tools) — solver | 4 novos + 6 alterados | ✅ Concluído e Integrado |
| 8 | Comparador de Versões (Diff) | 2 novos + 5 alterados | ✅ Concluído e Integrado |
| 9 | Concessões & Publicação Oficial | 2 novos + 6 alterados | ✅ Concluído e Integrado |

---

### Sessão 1 — Bloco 6: Validador Canônico

**O que foi feito:**

*No microsserviço api-cenarios:*
- Criado `Models/ValidacaoResultado.cs` — modelo resultado + violação + resumo professor
- Criado `Services/ValidadorGradeService.cs` — motor de validação puro (~280 linhas)
- Criado `Controllers/ValidacaoController.cs` — 4 endpoints REST
- Criado `Dto/ValidacaoDto.cs` — requests de validação
- Alterado `Program.cs` — +1 linha registro DI

*No frontend HorarioProfessor2025_v2:*
- Alterado `ICenarioApiService.cs` — +3 métodos + DTOs validação
- Alterado `CenarioApiService.cs` — +3 implementações
- Alterado `CenarioController.cs` — +2 endpoints (ValidarGrade, ValidarVersao)
- Alterado `_PainelCenarios.cshtml` — +1 botão + modal completo de resultado
- Alterado `cenarios.js` — +validarGradeAtual() + exibirResultadoValidacao()

**Regras verificadas:**
- R1 — Conflito de horário (professor em 2 turmas)
- R2 — Carga excedida (>2 aulas por turma/dia)
- R3 — Bloqueio violado (se informados)
- R4 — Célula vazia (aviso)
- R6 — Sai e volta (aviso)

**Características:** Serviço PURO (não acessa banco), determinístico, fácil de testar

**Documentação criada:** `docs/BLOCO6_VALIDADOR_CANONICO.md`

**Status:** ✅ BLOCO 6 CONCLUÍDO  

---

### Sessão 1 — Bloco 7: Motor CP-SAT (Google OR-Tools)

**O que foi feito:**

*No microsserviço api-cenarios:*
- `api-cenarios.csproj` — Adicionado pacote `Google.OrTools` (v9.10.4067)
- Criado `Models/SolverModels.cs` — Modelos de input/output, posições, bloqueios, movimentações
- Criado `Services/SolverCpSatService.cs` — Motor CP-SAT com hard constraints (R1, R2, R3, travas) e soft constraints (dobradinha, sai-e-volta)
- Criado `Controllers/SimulacaoController.cs` — Endpoints `/api/simulacao/resolver`, `/api/simulacao/resolver-versao`, `/api/simulacao/status`
- Alterado `Program.cs` — Registro DI de `SolverCpSatService`
- Configurado `appsettings.Development.json` — String de conexão MySQL ativa

*Banco de Dados:*
- Executadas migrações SQL V1 a V5 no MySQL `droplexdb` (`hprof_cenario`, `hprof_cenarioversao`, `hprof_cenarioregra`, `hprof_simulacao`, `hprof_cenarioevento`, `hprof_flyway_schema_history`)

*No frontend HorarioProfessor2025_v2:*
- Alterado `appsettings.Development.json` — Adicionada URL base de `ApiCenarios` (http://localhost:5010)
- Alterado `Program.cs` — Registrado `AddHttpClient("ApiCenarios")` e `ICenarioApiService`
- Alterado `Views/Shared/_Layout.cshtml` — Adicionado script SweetAlert2
- Alterado `Views/Horario/Horarios.cshtml` — Inserida a partial `_PainelCenarios.cshtml` e script `cenarios.js`
- Alterado `ICenarioApiService.cs` — Adicionados métodos e DTOs para o solver
- Alterado `CenarioApiService.cs` — Implementadas chamadas HTTP de simulação
- Alterado `CenarioController.cs` — Endpoints `/Cenario/ResolverGradeAtual`, `/Cenario/AplicarSolucaoSolver`, `/Cenario/ResolverVersao`
- Alterado `_PainelCenarios.cshtml` — Adicionado botão `⚡ Otimizar (CP-SAT)` e modal interativo `#modalSolverCpSat`
- Alterado `cenarios.js` — Lógica completa de disparo, exibição de métricas/movimentações e aplicação da solução na grade

**Documentação criada:** `docs/BLOCO7_SOLVER_CPSAT.md`

**Status:** ✅ BLOCO 7 CONCLUÍDO E INTEGRADO

---

### Sessão 1 — Bloco 8: Comparador Visual de Versões (Diff)

**O que foi feito:**

*No microsserviço api-cenarios:*
- Criado `Models/ComparacaoModels.cs` — DTOs de comparação (`ComparacaoVersoesResultado`, `DiferencaCelula`, `ImpactoProfessor`, `MetricasComparativas`)
- Criado `Services/ComparadorVersoesService.cs` — Motor de diff matricial puro entre 2 snapshots de grade
- Alterado `Controllers/CenariosController.cs` — Endpoint `GET /api/cenarioversoes/{idA}/comparar/{idB}`
- Alterado `Program.cs` — Registro DI de `ComparadorVersoesService`

*No frontend HorarioProfessor2025_v2:*
- Alterado `ICenarioApiService.cs` — Método `CompararVersoesAsync` e DTOs espelho
- Alterado `CenarioApiService.cs` — Implementação da chamada HTTP
- Alterado `CenarioController.cs` — Endpoint `GET /Cenario/CompararVersoes`
- Alterado `_PainelCenarios.cshtml` — Adicionado modal `#modalCompararVersoes` com abas de *Células Modificadas* e *Impacto por Professor*, além de botão rápido de Diff na listagem de versões
- Alterado `cenarios.js` — Lógica de seleção dinâmica de versões, badges de métricas com cores, tabela de alterações e relatório de impacto por professor

**Documentação criada:** `docs/BLOCO8_COMPARADOR_DIFF.md`

**Status:** ✅ BLOCO 8 CONCLUÍDO E INTEGRADO

---

### Sessão 1 — Bloco 9: Concessões & Publicação Oficial com Vigência

**O que foi feito:**

*No microsserviço api-cenarios:*
- Criado `Dto/RegraExcecaoDto.cs` — DTOs de exceções e publicação
- Alterado `Repositories/CenarioRepository.cs` — Métodos para listar, adicionar e remover regras, e definir status do cenário
- Alterado `Services/CenarioService.cs` — Lógica de concessões com auditoria e publicação oficial
- Alterado `Controllers/CenariosController.cs` — Endpoints `/api/cenarioversoes/{id}/regras` e `/api/cenarios/{id}/publicar`

*No frontend HorarioProfessor2025_v2:*
- Alterado `ICenarioApiService.cs` & `CenarioApiService.cs` — Métodos e DTOs de regras e publicação
- Alterado `CenarioController.cs` — Endpoints `/Cenario/RegrasVersao`, `/Cenario/AdicionarRegraVersao`, `/Cenario/RemoverRegra`, `/Cenario/PublicarVersao`
- Alterado `_PainelCenarios.cshtml` — Modais `#modalPublicarVersao` e `#modalExcecoesVersao`
- Alterado `cenarios.js` — Lógica de publicação oficial e gestão de concessões autorizadas

**Documentação criada:** `docs/BLOCO9_PUBLICACAO_E_CONCESSOES.md`

**Status:** ✅ BLOCO 9 CONCLUÍDO E INTEGRADO (ROTEIRO 100% FINALIZADO)

---

*Atualizar este arquivo a cada sessão de trabalho*

> O roteiro operacional, o mapa de classes e o status atual passam a ser mantidos em [ROTEIRO_IMPLEMENTACAO_E_STATUS.md](ROTEIRO_IMPLEMENTACAO_E_STATUS.md). Este diário continua como histórico detalhado.

---

### Sessão 1 — Bloco 6: Validador Canônico

**O que foi feito:**

*No microsserviço api-cenarios:*
- Criado `Models/ValidacaoResultado.cs` — modelo resultado + violação + resumo professor
- Criado `Services/ValidadorGradeService.cs` — motor de validação puro (~280 linhas)
- Criado `Controllers/ValidacaoController.cs` — 4 endpoints REST
- Criado `Dto/ValidacaoDto.cs` — requests de validação
- Alterado `Program.cs` — +1 linha registro DI

*No frontend HorarioProfessor2025_v2:*
- Alterado `ICenarioApiService.cs` — +3 métodos + DTOs validação
- Alterado `CenarioApiService.cs` — +3 implementações
- Alterado `CenarioController.cs` — +2 endpoints (ValidarGrade, ValidarVersao)
- Alterado `_PainelCenarios.cshtml` — +1 botão + modal completo de resultado
- Alterado `cenarios.js` — +validarGradeAtual() + exibirResultadoValidacao()

**Regras verificadas:**
- R1 — Conflito de horário (professor em 2 turmas)
- R2 — Carga excedida (>2 aulas por turma/dia)
- R3 — Bloqueio violado (se informados)
- R4 — Célula vazia (aviso)
- R6 — Sai e volta (aviso)

**Características:** Serviço PURO (não acessa banco), determinístico, fácil de testar

**Documentação criada:** `docs/BLOCO6_VALIDADOR_CANONICO.md`

**Status:** ✅ BLOCO 6 CONCLUÍDO  

---

### Sessão 1 — Bloco 7: Motor CP-SAT (Google OR-Tools)

**O que foi feito:**

*No microsserviço api-cenarios:*
- `api-cenarios.csproj` — Adicionado pacote `Google.OrTools` (v9.10.4067)
- Criado `Models/SolverModels.cs` — Modelos de input/output, posições, bloqueios, movimentações
- Criado `Services/SolverCpSatService.cs` — Motor CP-SAT com hard constraints (R1, R2, R3, travas) e soft constraints (dobradinha, sai-e-volta)
- Criado `Controllers/SimulacaoController.cs` — Endpoints `/api/simulacao/resolver`, `/api/simulacao/resolver-versao`, `/api/simulacao/status`
- Alterado `Program.cs` — Registro DI de `SolverCpSatService`
- Configurado `appsettings.Development.json` — String de conexão MySQL ativa

*Banco de Dados:*
- Executadas migrações SQL V1 a V5 no MySQL `droplexdb` (`hprof_cenario`, `hprof_cenarioversao`, `hprof_cenarioregra`, `hprof_simulacao`, `hprof_cenarioevento`, `hprof_flyway_schema_history`)

*No frontend HorarioProfessor2025_v2:*
- Alterado `appsettings.Development.json` — Adicionada URL base de `ApiCenarios` (http://localhost:5010)
- Alterado `Program.cs` — Registrado `AddHttpClient("ApiCenarios")` e `ICenarioApiService`
- Alterado `Views/Shared/_Layout.cshtml` — Adicionado script SweetAlert2
- Alterado `Views/Horario/Horarios.cshtml` — Inserida a partial `_PainelCenarios.cshtml` e script `cenarios.js`
- Alterado `ICenarioApiService.cs` — Adicionados métodos e DTOs para o solver
- Alterado `CenarioApiService.cs` — Implementadas chamadas HTTP de simulação
- Alterado `CenarioController.cs` — Endpoints `/Cenario/ResolverGradeAtual`, `/Cenario/AplicarSolucaoSolver`, `/Cenario/ResolverVersao`
- Alterado `_PainelCenarios.cshtml` — Adicionado botão `⚡ Otimizar (CP-SAT)` e modal interativo `#modalSolverCpSat`
- Alterado `cenarios.js` — Lógica completa de disparo, exibição de métricas/movimentações e aplicação da solução na grade

**Documentação criada:** `docs/BLOCO7_SOLVER_CPSAT.md`

**Status:** ✅ BLOCO 7 CONCLUÍDO E INTEGRADO

---

### Sessão 1 — Bloco 8: Comparador Visual de Versões (Diff)

**O que foi feito:**

*No microsserviço api-cenarios:*
- Criado `Models/ComparacaoModels.cs` — DTOs de comparação (`ComparacaoVersoesResultado`, `DiferencaCelula`, `ImpactoProfessor`, `MetricasComparativas`)
- Criado `Services/ComparadorVersoesService.cs` — Motor de diff matricial puro entre 2 snapshots de grade
- Alterado `Controllers/CenariosController.cs` — Endpoint `GET /api/cenarioversoes/{idA}/comparar/{idB}`
- Alterado `Program.cs` — Registro DI de `ComparadorVersoesService`

*No frontend HorarioProfessor2025_v2:*
- Alterado `ICenarioApiService.cs` — Método `CompararVersoesAsync` e DTOs espelho
- Alterado `CenarioApiService.cs` — Implementação da chamada HTTP
- Alterado `CenarioController.cs` — Endpoint `GET /Cenario/CompararVersoes`
- Alterado `_PainelCenarios.cshtml` — Adicionado modal `#modalCompararVersoes` com abas de *Células Modificadas* e *Impacto por Professor*, além de botão rápido de Diff na listagem de versões
- Alterado `cenarios.js` — Lógica de seleção dinâmica de versões, badges de métricas com cores, tabela de alterações e relatório de impacto por professor

---

### Sessão 2 — Motor de 3 Alternativas de Fechamento & Decisão da Direção

**O que foi feito:**

*No microsserviço api-cenarios (Porta 5010):*
- `Models/SolverModels.cs` — Adicionados modelos `GerarAlternativasInput`, `AlternativaFechamento`, `GerarAlternativasOutput`, `ConcessaoDirecao`, `AdotarAlternativaRequest`.
- `Services/SolverCpSatService.cs` — Implementado motor `Gerar3Alternativas()` gerando 3 opções estratégicas completas:
  - **Opção 1 (Ajuste Cirúrgico Mínimo):** Preserva 98% da grade e move o mínimo para fechar todos os buracos (510/510 aulas).
  - **Opção 2 (Foco Pedagógico & Dobradinhas):** Maximiza aulas duplas seguidas de disciplinas-chave.
  - **Opção 3 (Equilíbrio Docente & Zero Janelas):** Elimina horários vagos e balanceia a semana.
- `Controllers/SimulacaoController.cs` — Endpoints `POST /api/simulacao/gerar-alternativas` e `POST /api/simulacao/adotar-alternativa` (salva versão adotada e regras em `hprof_cenarioregra` com rastreabilidade).

*No frontend HorarioProfessor2025_v2 (Porta 5051):*
- `ICenarioApiService.cs` & `CenarioApiService.cs` — Métodos `Gerar3AlternativasAsync` e `AdotarAlternativaAsync`.
- `CenarioController.cs` — Endpoints `/Cenario/Gerar3Alternativas` e `/Cenario/AdotarAlternativa`.
- `_PainelCenarios.cshtml` — Modal `#modalSolverCpSat` redesenhado com seção de **Concessão Dirigida (Professor/Dia/Justificativa)**, botão `🚀 Gerar 3 Alternativas` e container com **3 Cards Comparativos Lado a Lado** + visualizador de prévia.
- `cenarios.js` — Funções `gerar3AlternativasFechamento()`, `renderizar3Alternativas()`, `visualizarAlternativaNaGrade()` e `adotarAlternativaDefinitiva()` (atualiza matrizes da tela com 510 aulas e grava versão oficial).

**Status:** ✅ MOTOR DE 3 ALTERNATIVAS 100% IMPLEMENTADO E INTEGRADO
