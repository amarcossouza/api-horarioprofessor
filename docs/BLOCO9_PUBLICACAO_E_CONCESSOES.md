# Bloco 9 — Concessões / Exceções Autorizadas & Publicação Oficial com Vigência

**Data:** 31/08/2026  
**Status:** ✅ Concluído e Integrado (Backend + Frontend + UI)

---

## 🎯 O Que É

O **Bloco 9** conclui o ciclo de governança e operação oficial da grade escolar:

1. **Gestão de Concessões e Exceções Autorizadas (`hprof_cenarioregra`):**
   * Permite que a direção da escola registre exceções formais e temporárias para determinadas regras (ex: permitir que o professor Silva lecione na sexta-feira ou assuma mais de 2 aulas em uma turma específica com justificativa) sem alterar o cadastro fixo permanente do professor.
   * Rastreia quem aprovou, data/hora da aprovação e justificativa formal.

2. **Publicação Oficial com Vigência (`hprof_cenario` & `hprof_cenarioevento`):**
   * Permite eleger qualquer versão salva de um cenário como a **Grade Oficial Vigente** da unidade escolar.
   * Define o período de vigência no calendário (Data de Início e Data de Fim).
   * Registra o evento imutável `PUBLICACAO_FEITA` na trilha de auditoria com hash SHA-256 da grade.

---

## 📁 Arquivos Criados e Alterados

### No microsserviço `api-cenarios` (Porta 5010):
* `Dto/RegraExcecaoDto.cs` — DTOs `CriarRegraExcecaoRequest`, `RegraExcecaoResponse`, `PublicarVersaoRequest`, `PublicarVersaoResponse`.
* `Repositories/CenarioRepository.cs` — Métodos `ListarRegrasPorVersaoAsync`, `AdicionarRegraAsync`, `RemoverRegraAsync`, `DefinirStatusCenarioAsync`.
* `Services/CenarioService.cs` — Lógica de negócio de regras, auditoria de exceções e despacho de publicação oficial.
* `Controllers/CenariosController.cs` — Endpoints:
  * `GET /api/cenarioversoes/{idVersao}/regras`
  * `POST /api/cenarioversoes/{idVersao}/regras`
  * `DELETE /api/cenarioregras/{id}`
  * `POST /api/cenarios/{idCenario}/publicar`

### No frontend MVC `HorarioProfessor2025_v2` (Porta 5051):
* `Services/ICenarioApiService.cs` & `CenarioApiService.cs` — Métodos `ListarRegrasVersaoAsync`, `AdicionarRegraVersaoAsync`, `RemoverRegraAsync`, `PublicarVersaoAsync`.
* `Controllers/CenarioController.cs` — Endpoints MVC `/Cenario/RegrasVersao`, `/Cenario/AdicionarRegraVersao`, `/Cenario/RemoverRegra`, `/Cenario/PublicarVersao`.
* `Views/Cenario/_PainelCenarios.cshtml`:
  * Modal `#modalPublicarVersao` (período de vigência, responsável e despacho oficial).
  * Modal `#modalExcecoesVersao` (tabela de concessões da versão e formulário de cadastro com justificativa).
  * Botões de ação em cada versão da listagem.
* `wwwroot/js/cenarios.js` — Funções `abrirModalPublicarVersao()`, `executarPublicacaoVersao()`, `abrirModalExcecoesVersao()`, `adicionarExcecaoVersao()`, `removerExcecaoVersao()`.

---

## 🔌 Endpoints REST Disponíveis

| Serviço | Método | Rota | Descrição |
| :--- | :--- | :--- | :--- |
| `api-cenarios` (5010) | `GET` | `/api/cenarioversoes/{idVersao}/regras` | Lista todas as regras e exceções vinculadas à versão. |
| `api-cenarios` (5010) | `POST` | `/api/cenarioversoes/{idVersao}/regras` | Registra nova exceção formal com justificativa e autor. |
| `api-cenarios` (5010) | `DELETE` | `/api/cenarioregras/{id}` | Revoga uma exceção cadastrada. |
| `api-cenarios` (5010) | `POST` | `/api/cenarios/{idCenario}/publicar` | Publica a versão como Grade Oficial Vigente com auditoria. |
| `HorarioProfessorMVC` (5051) | `POST` | `/Cenario/PublicarVersao` | Proxy MVC para despacho de publicação. |
| `HorarioProfessorMVC` (5051) | `POST` | `/Cenario/AdicionarRegraVersao` | Proxy MVC para registro de exceções. |

---

## 🎨 Como Usar na Interface

1. Abra o painel de **📄 Cenários** e selecione as versões do cenário.
2. **Para Conceder Exceções:**
   * Clique no ícone de balança (⚖️) na versão desejada.
   * Selecione a regra (ex: `R3 Bloqueio de Horário`), informe o professor, a turma e a **justificativa formal**.
   * Clique em **Registrar Concessão**.
3. **Para Publicar como Grade Oficial:**
   * Clique no ícone de megafone (📢) na versão escolhida.
   * Informe a data de início e fim da vigência e o despacho oficial.
   * Clique em **Confirmar Publicação Oficial**.
   * O cenário recebe o status `PUBLICADO` e a versão é fixada como Grade Oficial.
