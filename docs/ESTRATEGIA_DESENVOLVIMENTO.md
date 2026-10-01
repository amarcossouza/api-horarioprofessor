# Estratégia de Desenvolvimento — Horário Professor

Plano prático de execução baseado no PLANEJAMENTO_NOVO_FECHAMENTO.md

**Criado em:** 31/08/2026  
**Atualizado em:** 31/08/2026  
**Status:** Iniciando

---

## 1. Visão Geral — O Que Vamos Fazer

O planejamento descreve **8 etapas** (0 a 7) para evoluir o sistema de horários escolares.  
A estratégia é **desenvolver por partes pequenas e testáveis**, sem quebrar o que já funciona.

### Princípios
- ✅ **Não mexer no que funciona** — o sistema atual continua rodando
- ✅ **Código novo isolado** — tabelas com prefixo `hprof_`, namespaces separados
- ✅ **Uma entrega de cada vez** — só avançar quando a etapa anterior estiver validada
- ✅ **Documentar tudo** — cada passo fica registrado nesta pasta `docs/`

---

## 2. Mapa dos Dois Projetos

| Projeto | Caminho | Tecnologia | Porta |
|---------|---------|-----------|-------|
| **Backend (API)** | `C:\projkinix\api-horarioprofessor` | ASP.NET .NET 8 | 5000 |
| **Frontend (MVC)** | `C:\projkinix\HorarioProfessor2025_v2` | ASP.NET MVC .NET 8 | 5051 |

**Banco:** MySQL em `134.209.164.87` — database `droplexdb`

---

## 3. Ordem de Execução — Fases Práticas

### 🔷 FASE 1 — FUNDAÇÃO (Etapas 0 e 1 do plano)
> **Objetivo:** Proteger o que existe e criar a base de persistência

| Passo | O Que Fazer | Onde | Status |
|-------|-------------|------|--------|
| 1.1 | Inventariar todas as tabelas `hprof_` existentes no banco | Banco MySQL | ✅ Concluído |
| 1.2 | Fotografar o fluxo atual: capturar dados do caso piloto (CEU EMEF MENINOS, 2025, manhã) | API + Frontend | ✅ Mapeado |
| 1.3 | Mapear regras realmente executadas no código (`AppState.cs`, `WorkflowManager.cs`, `PrincipalService.cs`) | API | ✅ Concluído |
| 1.4 | Criar scripts Flyway para as novas tabelas (`hprof_cenario`, `hprof_cenarioversao`, `hprof_cenarioregra`, `hprof_simulacao`, `hprof_cenarioevento`) | API / MySQL | ✅ Executado no Banco |
| 1.5 | Implementar microsserviço `api-cenarios` com `POST /api/cenarios` — salvar grade existente como cenário | `api-cenarios` | ✅ Concluído |
| 1.6 | Implementar endpoint `GET /api/cenarioversoes/{id}` — restaurar cenário salvo | `api-cenarios` | ✅ Concluído |
| 1.7 | Testar: salvar → fechar sistema → reabrir → restaurar com todas as regras | Frontend + API | ✅ Concluído |

### 🔷 FASE 2 — MODELO E VALIDAÇÃO (Etapa 2 do plano)
> **Objetivo:** Criar o modelo canônico com identificadores e validador independente

| Passo | O Que Fazer | Onde | Status |
|-------|-------------|------|--------|
| 2.1 | Criar modelo canônico com IDs e snapshot JSON estruturado | `api-cenarios` | ✅ Concluído |
| 2.2 | Criar adaptador: modelo canônico ↔ matrizes legadas | Frontend + API | ✅ Concluído |
| 2.3 | Criar validador independente de regras (R1 a R6, puro e determinístico) | `api-cenarios` | ✅ Concluído |
| 2.4 | Encapsular motor legado (`AppState`, `WorkflowManager`) como API legada isolada (porta 5000) | `api-horarioprofessor` | ✅ Isolado |
| 2.5 | Testar: validar grade gerada pelo motor legado com o novo validador | Frontend + API | ✅ Concluído |

### 🔷 FASE 3 — MOTOR CP-SAT (Etapas 3 e 4 do plano)
> **Objetivo:** Provar que o Google OR-Tools resolve o problema real

| Passo | O Que Fazer | Onde | Status |
|-------|-------------|------|--------|
| 3.1 | Instalar pacote `Google.OrTools` (9.10) no projeto `api-cenarios` | `api-cenarios` | ✅ Concluído |
| 3.2 | Criar modelo CP-SAT com Hard Constraints (R1, R2, R3, travas) e Soft (dobradinhas) | `api-cenarios` | ✅ Concluído |
| 3.3 | Implementar serviço `SolverCpSatService.cs` e controller `SimulacaoController.cs` | `api-cenarios` | ✅ Concluído |
| 3.4 | Implementar modos de busca: PARCIAL (preserva alocações) e COMPLETO | `api-cenarios` | ✅ Concluído |
| 3.5 | Integrar frontend com cliente HTTP (`CenarioApiService.cs`) e controller MVC | Frontend | ✅ Concluído |
| 3.6 | Integrar UI no frontend com botão e modal interativo `⚡ Otimizar (CP-SAT)` | Frontend | ✅ Concluído |
| 3.7 | Permitir aplicação da grade resolvida diretamente na tela com 1 clique | Frontend | ✅ Concluído |

### 🔷 FASE 4 — PRÓXIMAS EVOLUÇÕES (Etapas 5 e 6 do plano)
> **Objetivo:** Ajuste cirúrgico avançado, comparação de 3 alternativas e publicação oficial

| Passo | O Que Fazer | Onde | Status |
|-------|-------------|------|--------|
| 4.1 | Comparação visual lado a lado (diff) entre 2 ou 3 versões | `api-cenarios` + Frontend | ✅ Concluído (Bloco 8) |
| 4.2 | Gestão e aprovação de concessões / exceções rastreadas | `api-cenarios` + Frontend | ✅ Concluído (Bloco 9) |
| 4.3 | Publicação de grade oficial com vigência no calendário | `api-cenarios` + Frontend | ✅ Concluído (Bloco 9) |
| 4.4 | Piloto em produção com usuários finais | Ambos | 🚀 Pronto para Uso |

---

## 4. Por Onde Começar — AGORA

### ✅ Primeiro passo concreto: Passo 1.1 + 1.2 + 1.3

Antes de escrever qualquer código novo, precisamos:

1. **Inventariar o banco** — Quais tabelas `hprof_` existem? Qual o estado atual dos dados?
2. **Capturar o caso piloto** — Ler (sem alterar!) os dados da CEU EMEF MENINOS, 2025, manhã
3. **Mapear as regras do código** — Entender exatamente o que `AppState.cs` e `WorkflowManager.cs` fazem

> 🎯 **Meta:** Ter um relatório claro do estado atual antes de tocar em qualquer código

### Sequência de trabalho imediata:

```
1. Ler AppState.cs + PrincipalService.cs + WorkflowManager.cs
   → Documentar o fluxo real em docs/FLUXO_ATUAL.md

2. Ler PrincipalExecucaoController.cs
   → Documentar os endpoints atuais em docs/ENDPOINTS_ATUAIS.md

3. Mapear as regras de negócio encontradas
   → Documentar em docs/REGRAS_NEGOCIO.md

4. Identificar as tabelas do banco usadas no processamento
   → Documentar em docs/INVENTARIO_TABELAS.md
```

---

## 5. Arquivos de Documentação (esta pasta)

| Arquivo | Conteúdo | Status |
|---------|----------|--------|
| `PLANEJAMENTO_NOVO_FECHAMENTO.md` | Plano completo original | ✅ Existente |
| `PLANEJAMENTO_NOVO_FECHAMENTO.docx` | Versão Word do plano | ✅ Existente |
| `ESTRATEGIA_DESENVOLVIMENTO.md` | **Este arquivo** — plano prático | ✅ Criado |
| `FLUXO_ATUAL.md` | Fluxo real do código atual | ✅ Criado |
| `ENDPOINTS_ATUAIS.md` | Mapa dos endpoints da API | ⬜ A criar |
| `REGRAS_NEGOCIO.md` | Catálogo de regras identificadas | ⬜ A criar |
| `INVENTARIO_TABELAS.md` | Tabelas do banco com descrição | ⬜ A criar |
| `DIARIO_DESENVOLVIMENTO.md` | Registro do que foi feito dia a dia | ⬜ A criar |

---

## 6. Riscos Que Precisam de Atenção Imediata

| # | Risco | Impacto | Mitigação |
|---|-------|---------|-----------|
| 1 | GET `/api/PrincipalExecucao/horarios` **modifica o banco** | 🔴 Alto | Novo design: GET = leitura, POST = processamento |
| 2 | Frontend acessa MySQL diretamente (Dapper) **e** via API | 🔴 Alto | Novos fluxos só via API |
| 3 | `AppState` com inicialização assíncrona no construtor | 🟡 Médio | Revisar na Fase 2 |
| 4 | Banco compartilhado com outras aplicações | 🟡 Médio | Prefixo `hprof_` + Flyway isolado |
| 5 | Código comentado extenso misturado com código ativo | 🟡 Médio | Separar na documentação de regras |

---

## 7. Decisões Pendentes do Gestor

Antes de avançar para a Fase 3, precisamos de respostas para:

- [ ] Quais bloqueios de professor são negociáveis vs obrigatórios?
- [ ] Quem tem autoridade para aprovar exceções?
- [ ] Qual o limite aceitável de alterações em um ajuste pontual (março/abril)?
- [ ] Como tratar professor que trabalha em mais de uma escola?
- [ ] Qual a vigência padrão de uma grade oficial?

---

## Próximo Passo Imediato

> **Agora vamos começar pelo Passo 1.3** — ler e documentar o fluxo real do código  
> Arquivos-alvo: `AppState.cs`, `PrincipalService.cs`, `WorkflowManager.cs`, `PrincipalExecucaoController.cs`  
> Resultado: criação do arquivo `docs/FLUXO_ATUAL.md`
