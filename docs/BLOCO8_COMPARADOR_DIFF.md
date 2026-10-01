# Bloco 8 — Comparador Visual de Versões (Diff)

**Data:** 31/08/2026  
**Status:** ✅ Concluído e Integrado (Backend + Frontend + UI)

---

## 🎯 O Que É

O **Comparador Visual de Versões (Diff)** permite que a equipe gestora compare **qualquer par de versões** de um mesmo cenário (exemplo: a *Versão 1 Original* gerada pelo motor legado vs a *Versão 2 Otimizada* pelo solver CP-SAT) para avaliar com precisão cirúrgica o que foi modificado na grade horária.

---

## 🔍 O Que o Diff Analisa

O motor de comparação varre as 5 matrizes (Segunda a Sexta-feira) e todas as turmas, identificando:

1. **Alterações Célula a Célula:**
   * 🟢 **INSERIDO:** Horário que estava vago em A e recebeu um professor em B.
   * 🔴 **REMOVIDO:** Horário que tinha um professor em A e foi desocupado em B.
   * 🟡 **ALTERADO:** Horário onde o Professor X em A foi substituído pelo Professor Y em B.

2. **Métricas Comparativas de Fechamento:**
   * Saldo de aulas distribuídas (ex: $+8$ aulas alocadas).
   * Saldo de pendências (ex: $-8$ pendências eliminadas).
   * Saldo de conflitos de horário (ex: $0$ conflitos).
   * Indicador de fechamento completo da grade.

3. **Impacto por Professor:**
   * Saldo líquido de aulas por docente.
   * Turmas novas atribuídas ao professor.
   * Turmas removidas do professor.
   * Total de movimentações/trocas sofridas pelo professor na grade.

---

## 📁 Arquivos Criados e Alterados

### No microsserviço `api-cenarios` (Porta 5010):
* `Models/ComparacaoModels.cs` — Modelos `ComparacaoVersoesResultado`, `DiferencaCelula`, `ImpactoProfessor`, `MetricasComparativas`.
* `Services/ComparadorVersoesService.cs` — Motor de cálculo matricial de diff puro e determinístico.
* `Controllers/CenariosController.cs` — Endpoint `GET /api/cenarioversoes/{idVersaoA}/comparar/{idVersaoB}`.
* `Program.cs` — Injeção de dependência de `ComparadorVersoesService`.

### No frontend MVC `HorarioProfessor2025_v2` (Porta 5051):
* `Services/ICenarioApiService.cs` — Assinatura `CompararVersoesAsync` e DTOs correspondentes.
* `Services/CenarioApiService.cs` — Implementação da chamada HTTP `GET /api/cenarioversoes/{idA}/comparar/{idB}`.
* `Controllers/CenarioController.cs` — Endpoint `GET /Cenario/CompararVersoes?idVersaoA=x&idVersaoB=y`.
* `Views/Cenario/_PainelCenarios.cshtml` — Adicionado modal `#modalCompararVersoes` com duas abas (*Células Modificadas* e *Impacto por Professor*) e botão de acionamento rápido na tabela de versões.
* `wwwroot/js/cenarios.js` — Funções `abrirModalCompararVersoes()`, `executarComparacaoVersoes()` e `exibirResultadoComparacao()`.

---

## 🔌 Endpoints REST

| Serviço | Método | Rota | Descrição |
| :--- | :--- | :--- | :--- |
| `api-cenarios` (5010) | `GET` | `/api/cenarioversoes/{idA}/comparar/{idB}` | Executa o cálculo analítico de diff entre as duas versões salvas no banco. |
| `HorarioProfessorMVC` (5051) | `GET` | `/Cenario/CompararVersoes?idVersaoA=1&idVersaoB=2` | Proxy MVC que consome o microsserviço e entrega o JSON para a interface. |

---

## 🎨 Como Usar na Interface (UI)

1. Clique no botão **📄 Cenários** na barra superior da grade.
2. Na lista de cenários, clique no ícone de versões (🔀) do cenário desejado.
3. Na tabela de versões, clique no botão **Diff** ao lado de qualquer versão (ou no botão **Comparar Versões (Diff)** abaixo da tabela).
4. No modal:
   * Selecione a **Versão Base (A)** e a **Versão Comparada (B)**.
   * Visualize os cards de métricas no topo (Células alteradas, saldo de aulas, saldo de pendências e professores impactados).
   * Na aba **Células Modificadas**, veja cada slot alterado com cores destacadas (*Verde*, *Amarelo* ou *Vermelho*).
   * Na aba **Impacto por Professor**, avalie se algum professor ganhou ou perdeu turmas e sua variação de carga horária.
