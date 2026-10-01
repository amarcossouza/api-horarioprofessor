# Arquitetura de Microsserviços — Horário Professor

**Data:** 31/08/2026  
**Status:** Proposta aprovada para implementação por blocos

---

## 🎯 Decisão: Por que microsserviços?

O sistema atual é um **monolito acoplado**: a API faz tudo (cadastro, distribuição, relatórios, 
geração HTML, acesso ao banco). Qualquer mudança no motor de distribuição pode quebrar os cadastros 
e vice-versa.

Com microsserviços:
- **Motor novo (CP-SAT)** vive isolado — pode ser testado sem medo
- **API legada continua funcionando** — zero risco para o que já funciona
- **Cada serviço escala independente** — simulação pesada não trava cadastros
- **Deploy independente** — atualizar o solver não precisa redeployar tudo

---

## 🏗️ Arquitetura Proposta — 4 Serviços

```
┌─────────────────────────────────────────────────────────────────┐
│                        FRONTEND MVC                             │
│                    (porta 5051 — já existe)                      │
│                                                                 │
│    Telas de cadastro    Tela de grade    Nova tela simulação     │
│         │                    │                   │               │
└─────────┼────────────────────┼───────────────────┼──────────────┘
          │                    │                   │
          ▼                    ▼                   ▼
┌──────────────┐  ┌─────────────────┐  ┌────────────────────┐
│   SERVIÇO 1  │  │   SERVIÇO 2     │  │   SERVIÇO 3        │
│   API LEGADA │  │   API CENÁRIOS  │  │   API SIMULAÇÃO    │
│              │  │                 │  │                    │
│  Cadastros   │  │  CRUD cenários  │  │  Motor CP-SAT     │
│  Professores │  │  Versões        │  │  Fila de jobs     │
│  Escolas     │  │  Regras         │  │  Resultados       │
│  Matrizes    │  │  Publicação     │  │  Comparação       │
│  Relatórios  │  │  Auditoria      │  │                    │
│  DiarioOfic. │  │                 │  │                    │
│              │  │                 │  │                    │
│  porta 5000  │  │  porta 5010     │  │  porta 5020        │
│  (já existe) │  │  (NOVO)         │  │  (FUTURO)          │
└──────┬───────┘  └────────┬────────┘  └─────────┬──────────┘
       │                   │                     │
       └───────────────────┼─────────────────────┘
                           │
                           ▼
              ┌──────────────────────┐
              │   SERVIÇO 4          │
              │   BANCO DE DADOS     │
              │                      │
              │   MySQL (droplexdb)  │
              │   Tabelas hprof_*    │
              │                      │
              │   134.209.164.87     │
              └──────────────────────┘
```

---

## 📦 Detalhamento de Cada Serviço

### Serviço 1 — API Legada (porta 5000) ✅ JÁ EXISTE
**Projeto:** `api-horarioprofessor` (manter como está)

| Item | Valor |
|------|-------|
| Responsabilidade | Cadastros, distribuição atual, relatórios, diário oficial |
| Mudanças | **ZERO** — não mexemos neste serviço agora |
| Tecnologia | ASP.NET 8, EF Core, Dapper, MySQL |
| Por quê manter | O frontend depende dele, funciona, não queremos quebrar |

**Regra de ouro:** Não alteramos NADA no Serviço 1 até que o Serviço 2 esteja funcionando e testado.

---

### Serviço 2 — API Cenários (porta 5010) 🆕 CRIAR AGORA
**Projeto novo:** `api-cenarios`

| Item | Valor |
|------|-------|
| Responsabilidade | CRUD de cenários, versões, regras, eventos, publicação |
| Tecnologia | ASP.NET 8 Minimal API, EF Core, MySQL |
| Tabelas | `hprof_cenario`, `hprof_cenarioversao`, `hprof_cenarioregra`, `hprof_cenarioevento` |
| Padrão | Clean Architecture leve (Controllers → Services → Repositories) |

**Endpoints planejados:**

```
POST   /api/cenarios                    → Criar cenário
GET    /api/cenarios                    → Listar cenários (filtro: escola, ano, período)
GET    /api/cenarios/{id}               → Detalhe do cenário
PUT    /api/cenarios/{id}               → Atualizar cenário (título, status)
DELETE /api/cenarios/{id}               → Arquivar cenário (soft delete)

POST   /api/cenarios/{id}/versoes       → Salvar versão (snapshot da grade)
GET    /api/cenarios/{id}/versoes       → Listar versões do cenário
GET    /api/cenarioversoes/{id}         → Detalhe da versão + snapshot
GET    /api/cenarioversoes/{id}/comparar/{outroId} → Diff entre versões

POST   /api/cenarioversoes/{id}/regras  → Adicionar regra/exceção
GET    /api/cenarioversoes/{id}/regras  → Listar regras da versão
PUT    /api/cenarioregras/{id}          → Atualizar regra
DELETE /api/cenarioregras/{id}          → Remover regra

GET    /api/cenarios/{id}/eventos       → Timeline de auditoria
POST   /api/cenarios/{id}/publicar      → Eleger versão como oficial
```

---

### Serviço 3 — API Simulação (porta 5020) 🔮 FUTURO (Fase 3)
**Projeto novo:** `api-simulacao`

| Item | Valor |
|------|-------|
| Responsabilidade | Motor CP-SAT, fila de simulações, resultados |
| Tecnologia | ASP.NET 8, Google OR-Tools CP-SAT |
| Tabelas | `hprof_simulacao` |
| Dependência | Consome API Cenários para ler/gravar versões |

**Isso fica para depois.** Primeiro o Serviço 2 precisa funcionar.

---

### Serviço 4 — Banco de Dados (compartilhado)
**Mesmo MySQL** — `droplexdb` em `134.209.164.87`

| Item | Valor |
|------|-------|
| Isolamento | Prefixo `hprof_` nas tabelas |
| Migrações | Flyway com tabela própria `hprof_flyway_schema_history` |
| Nota | Todos os serviços acessam o mesmo banco — isolamento é lógico por prefixo |

---

## 📂 Estrutura de Pastas Proposta

```
C:\projkinix\
├── api-horarioprofessor\          ← Serviço 1 — API Legada (NÃO MEXER)
│   ├── Controllers\
│   ├── Services\
│   ├── docs\                      ← Documentação compartilhada
│   ├── flyway\                    ← Scripts SQL Flyway
│   └── ...
│
├── api-cenarios\                  ← Serviço 2 — API Cenários (NOVO)
│   ├── src\
│   │   ├── Controllers\
│   │   ├── Services\
│   │   ├── Models\
│   │   ├── Repositories\
│   │   ├── Dto\
│   │   └── Program.cs
│   ├── tests\
│   └── api-cenarios.csproj
│
├── api-simulacao\                 ← Serviço 3 — FUTURO
│   └── (será criado na Fase 3)
│
└── HorarioProfessor2025_v2\       ← Frontend MVC (já existe)
    ├── Controllers\
    ├── Views\
    └── ...
```

---

## 🔗 Como os Serviços se Comunicam

```
Frontend MVC (5051)
    │
    ├── HTTP GET/POST → API Legada (5000)     [cadastros, distribuição atual]
    │
    ├── HTTP GET/POST → API Cenários (5010)   [salvar/restaurar/comparar cenários]
    │
    └── HTTP POST     → API Simulação (5020)  [futuro: solicitar simulação CP-SAT]


API Simulação (5020) → HTTP → API Cenários (5010)  [ler versão-base, gravar resultado]
```

**Comunicação síncrona via HTTP REST** entre todos os serviços.
- Sem message broker por enquanto (simplicidade)
- Sem service discovery (portas fixas em dev)
- Futuro: avaliar RabbitMQ ou Redis para fila de simulações pesadas

---

## 🚦 Ordem de Implementação

| Bloco | O que | Quando |
|-------|-------|--------|
| **Bloco 2** ✅ | Scripts SQL Flyway (5 tabelas) | Feito |
| **Bloco 3** 🔜 | Criar projeto `api-cenarios` com estrutura limpa | AGORA |
| **Bloco 4** | Models + DbContext + primeiro endpoint POST cenário | Próximo |
| **Bloco 5** | CRUD completo de cenários e versões | Depois |
| **Bloco 6** | Integrar frontend → salvar grade como cenário | Depois |
| **Bloco 7** | Restaurar cenário no frontend | Depois |
| **Bloco 8** | Comparar versões | Depois |
| **Bloco 9** | Criar `api-simulacao` + OR-Tools | Fase 3 |

---

## ⚠️ Regras Invioláveis

1. **NÃO mexer na API legada** (Serviço 1) até que Serviço 2 funcione
2. **NÃO criar dependência circular** entre serviços
3. **Cada serviço tem seu próprio `Program.cs`** e `.csproj`
4. **Mesmo banco MySQL** mas cada serviço só acessa suas tabelas
5. **Frontend chama cada serviço diretamente** (sem API Gateway por enquanto)
6. **Testes antes de integrar** — cada bloco é testável isolado

---

## 📌 Resumo Visual

```
HOJE:    Monolito (1 API faz tudo)          → Frágil, medo de mexer
         ┌─────────────────────┐
         │  API 5000 (tudo)    │
         └─────────────────────┘

FUTURO:  3 Serviços independentes           → Seguro, cada um no seu quadrado
         ┌──────────┐ ┌──────────┐ ┌──────────┐
         │ Legada   │ │ Cenários │ │ Simulação│
         │  5000    │ │  5010    │ │  5020    │
         └──────────┘ └──────────┘ └──────────┘
```

---

*Próximo bloco: Criar o projeto `api-cenarios` com a estrutura completa.*
