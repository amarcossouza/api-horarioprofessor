# Guia Operacional e de Testes — Horário Professor

**Data:** 31/08/2026  
**Status:** ✅ Sistema Integrado e Operacional

---

## 1. Arquitetura em Execução

O sistema opera com **3 componentes desacoplados** e **1 banco de dados compartilhado**:

```
┌───────────────────────────────────────────────────────────────────────────────────┐
│                                FRONTEND MVC (Porta 5051)                          │
│                           HorarioProfessor2025_v2                                 │
│  - Tela de Grade (/Horario/ObterHorarios)                                         │
│  - Barra de Cenários: [💾 Salvar Cenário] [📄 Cenários] [🟡 Validar Grade] [⚡ CP-SAT] │
└────────────────────────────┬─────────────────────────┬────────────────────────────┘
                             │                         │
            (Requisições Legadas)            (Cenários, Versões, Solver)
                             │                         │
                             ▼                         ▼
┌──────────────────────────────────────┐   ┌────────────────────────────────────────┐
│         API LEGADA (Porta 5000)      │   │       API CENÁRIOS (Porta 5010)        │
│          api-horarioprofessor        │   │               api-cenarios             │
│  - Cadastros                         │   │  - CRUD Cenários & Versões             │
│  - Distribuição Legada               │   │  - Validador Canônico (R1-R6)          │
│  - Diário Oficial                    │   │  - Motor CP-SAT (Google OR-Tools 9.10) │
└──────────────────┬───────────────────┘   └───────────────────┬────────────────────┘
                   │                                           │
                   └─────────────────────┬─────────────────────┘
                                         ▼
                   ┌───────────────────────────────────────────┐
                   │           MySQL (droplexdb)               │
                   │            134.209.164.87:3306            │
                   │  - Tabelas legadas                        │
                   │  - Tabelas isoladas: hprof_cenario*       │
                   └───────────────────────────────────────────┘
```

---

## 2. Como Iniciar os Serviços

Abra terminais PowerShell separados para cada serviço:

### Terminal 1 — Microsserviço `api-cenarios` (Porta 5010)
```powershell
cd C:\projkinix\api-cenarios
dotnet run
```
* **Swagger:** `http://localhost:5010/swagger`
* **Health Check:** `http://localhost:5010/health`

### Terminal 2 — API Legada `api-horarioprofessor` (Porta 5000)
```powershell
cd C:\projkinix\api-horarioprofessor
dotnet run
```
* **Swagger:** `http://localhost:5000/swagger`

### Terminal 3 — Frontend `HorarioProfessor2025_v2` (Porta 5051)
```powershell
cd C:\projkinix\HorarioProfessor2025_v2
dotnet run
```
* **Aplicação Web:** `http://localhost:5051`

---

## 3. Roteiro de Testes Passo a Passo na Interface

### Teste 1: Gerar e Salvar Cenário Versionado
1. No navegador, acesse `http://localhost:5051/Horario/ObterHorarios`.
2. Selecione a Escola, Ano (2025) e Período (MANHÃ).
3. Após a grade ser carregada na tela, clique no botão **💾 Salvar Cenário**.
4. Selecione **Criar cenário novo**, informe o título (ex: `Grade Base 2025 - A`) e observação.
5. Clique em **Salvar**.
6. **Resultado esperado:** Modal exibe alerta de sucesso com número da versão gerada, hash SHA-256 e estatísticas.

### Teste 2: Validação Canônica (R1 a R6)
1. Com a grade aberta na tela, clique no botão **🟡 Validar Grade**.
2. O sistema realiza varredura analítica instantânea nas 5 matrizes (Segunda a Sexta).
3. **Resultado esperado:** Modal abre com:
   * Header verde (Válida) ou vermelho (Com Violações).
   * Cards numéricos com total de violações, avisos, aulas distribuídas e células vazias.
   * Tabela discriminando violações de conflito (R1), carga excessiva (R2), bloqueios (R3) e avisos de sai-e-volta (R6).
   * Resumo detalhado por professor.

### Teste 3: Otimização Matemática com CP-SAT (Google OR-Tools)
1. Com a grade atual (mesmo que parcial), clique em **⚡ Otimizar (CP-SAT)**.
2. Defina os parâmetros:
   * **Modo:** `PARCIAL` (preenche apenas buracos/pendências) ou `COMPLETO`.
   * **Tempo Limite:** `30` segundos.
   * **Workers:** `4`.
   * **Congelar posições atuais:** Marcado.
3. Clique em **Iniciar Otimização CP-SAT**.
4. Observe o processamento do OR-Tools e o resultado:
   * Status: `OPTIMAL` ou `FEASIBLE`.
   * Métricas de aulas inseridas e tempo de execução em milissegundos.
   * Tabela com todas as movimentações de professores alocados.
5. Clique em **Aplicar Solução na Grade da Tela**.
6. **Resultado esperado:** A tela recarrega instantaneamente com a grade preenchida e sem conflitos.

### Teste 4: Listar e Restaurar Versões Anteriores
1. Clique no botão **📄 Cenários**.
2. Visualize a lista de cenários da escola.
3. Clique no ícone de versões (🔀) de um cenário.
4. Na lista de versões, clique em **Restaurar** em uma versão anterior.
5. Confirme o alerta do SweetAlert2.
6. **Resultado esperado:** A grade anterior é restabelecida na sessão e redesenhada na tela.

### Teste 5: Trilha de Auditoria
1. No modal de cenários, clique no botão **📜 Histórico** (Eventos).
2. **Resultado esperado:** Timeline cronológica com emojis e registros de todas as ações (`CENARIO_CRIADO`, `VERSAO_SALVA`, `VERSAO_RESTAURADA`, `SIMULACAO_CONCLUIDA`), autor e data/hora.

---

## 4. Referência dos Endpoints da API Cenários (5010)

| Verbo | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/health` | Health check simples do microsserviço |
| `POST` | `/api/cenarios` | Cria um novo cenário para a escola |
| `GET` | `/api/cenarios?idEscola=1&anoLetivo=2025&periodo=MANHA` | Lista cenários por filtro |
| `GET` | `/api/cenarios/{id}` | Detalhes do cenário e suas versões |
| `PUT` | `/api/cenarios/{id}` | Atualização com controle otimista de concorrência (`revisao`) |
| `DELETE` | `/api/cenarios/{id}` | Soft delete (arquivar cenário) |
| `POST` | `/api/cenarios/{id}/versoes` | Salva nova versão imutável com snapshot JSON |
| `GET` | `/api/cenarioversoes/{id}` | Obtém o snapshot completo de uma versão |
| `GET` | `/api/cenarios/{id}/eventos` | Lista histórico de auditoria |
| `POST` | `/api/validacao/snapshot` | Valida snapshot JSON puro contra regras R1-R6 |
| `GET` | `/api/validacao/versao/{id}` | Valida versão salva no banco |
| `POST` | `/api/simulacao/resolver` | Executa o solver CP-SAT diretamente via JSON |
| `POST` | `/api/simulacao/resolver-versao` | Executa o solver a partir de versão e salva resultado |
| `GET` | `/api/simulacao/status` | Status operacional do CP-SAT e versão OR-Tools |

---

## 5. Diagnóstico e Resolução de Problemas

* **Erro de conexão ao banco MySQL:**
  * Verifique se o servidor `134.209.164.87:3306` está acessível da sua rede.
  * Verifique a connection string em `api-cenarios/appsettings.Development.json`.
* **Frontend não conecta na API Cenários:**
  * Confirme se `api-cenarios` está rodando na porta 5010.
  * O CORS já está pré-configurado na API para aceitar origens `http://localhost:5051`.
* **Sessão vazia ao tentar salvar cenário:**
  * É necessário carregar uma grade na tela primeiro via `ObterHorarios` antes de clicar em "Salvar Cenário" ou "Validar Grade".
