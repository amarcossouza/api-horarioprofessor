# Catálogo de Regras Canônicas — Horário Professor

**Data:** 31/08/2026  
**Status:** ✅ Especificado e Implementado

---

## 1. Visão Geral

Este documento é a referência canônica de todas as regras de negócio para a distribuição, validação e otimização de horários escolares no sistema **Horário Professor**.

As regras são classificadas em três níveis de severidade:

1. 🔴 **OBRIGATÓRIA (Hard Constraint):** Inviolável. Qualquer grade que quebre uma regra obrigatória é considerada **Inválida**. O motor CP-SAT jamais gera soluções violando regras obrigatórias.
2. 🟡 **AVISO / PREFERÊNCIA (Soft Constraint):** Desejável pedagogicamente. Violações não invalidam a grade, mas são contabilizadas e minimizadas na função objetivo do otimizador.
3. 🔒 **TRAVA DE CENÁRIO (Pinned Slot):** Posição congelada pelo diretor. O motor não tem autorização para realocar.

---

## 2. Tabela Resumo das Regras

| Código | Nome da Regra | Tipo | Severidade | Implementada no Validador? | Implementada no CP-SAT? |
| :--- | :--- | :--- | :--- | :---: | :---: |
| **R1** | Sem Conflito de Horário | Exclusividade | 🔴 OBRIGATÓRIA | ✅ Sim | ✅ Sim |
| **R2** | Carga Máxima por Turma/Dia | Limite Pedagógico | 🔴 OBRIGATÓRIA | ✅ Sim | ✅ Sim |
| **R3** | Bloqueios e Indisponibilidade | Restrição Cadastral | 🔴 OBRIGATÓRIA | ✅ Sim | ✅ Sim |
| **R4** | Célula Vazia (Slot Ocioso) | Cobertura | 🟡 AVISO | ✅ Sim | ✅ Sim (Minimizada) |
| **R5** | Exclusividade do Slot | Colisão de Turma | 🔴 OBRIGATÓRIA | ✅ Sim | ✅ Sim |
| **R6** | Sai e Volta (Janela Interna) | Continuidade | 🟡 AVISO | ✅ Sim | ✅ Sim (Penalizada) |
| **R7** | Dobradinhas (Aulas Geminadas)| Preferência Didática | 🟢 PREFERÊNCIA | — | ✅ Sim (Bonificada) |

---

## 3. Detalhamento de Cada Regra

---

### 🔴 R1 — Sem Conflito de Horário (Exclusividade do Professor)

* **Definição:** Um mesmo professor não pode estar alocado em duas ou mais turmas simultaneamente (no mesmo dia da semana e no mesmo slot/aula).
* **Fórmula no Validador:**
  Para cada dia $d \in \{0..4\}$ e para cada aula $s \in \{0..\text{slots}-1\}$:
  $$\text{Se } \text{Grade}[d, s, t_1] = \text{Grade}[d, s, t_2] = P \implies \text{Violação R1}$$
* **Fórmula no CP-SAT:**
  $$\sum_{t=1}^{T} X_{p,t,d,s} \le 1 \quad \forall p \in P, \forall d \in D, \forall s \in S$$
* **Mensagem de Violação:**
  `CONFLITO: Professor 'NOME' está em 2 turmas ao mesmo tempo (Turma A, Turma B) — Segunda-feira, Aula 3`

---

### 🔴 R2 — Carga Máxima por Turma por Dia

* **Definição:** Um professor não pode lecionar mais do que 2 aulas para a mesma turma em um único dia (evita estafa e saturação de conteúdo).
* **Fórmula no Validador:**
  $$\text{Contagem}(P, T, D) = \sum_{s} \mathbb{I}(\text{Grade}[d, s, t] == P) \le 2$$
* **Fórmula no CP-SAT:**
  $$\sum_{s=1}^{S} X_{p,t,d,s} \le 2 \quad \forall p \in P, \forall t \in T, \forall d \in D$$
* **Mensagem de Violação:**
  `CARGA EXCEDIDA: Professor 'NOME' tem 3 aulas na turma '6A' em Terça-feira (máximo permitido: 2)`

---

### 🔴 R3 — Bloqueios e Indisponibilidade de Horário

* **Definição:** Professores que possuem restrições cadastradas de horário (ex: outros vínculos, pós-graduação, dia fixo sem aula) não podem receber alocação no período bloqueado.
* **Fórmula no Validador:**
  $$\text{Se } (P, d, s) \in \text{Bloqueios} \land \text{Grade}[d, s, t] == P \implies \text{Violação R3}$$
* **Fórmula no CP-SAT:**
  $$X_{p,t,d,s} = 0 \quad \forall (p, d, s) \in \text{Bloqueios}, \forall t \in T$$
  *(A variável sequer é instanciada no solver para reduzir o tamanho da árvore combinatória).*
* **Mensagem de Violação:**
  `BLOQUEIO VIOLADO: Professor 'NOME' está alocado em Quarta-feira, Aula 1, onde possui bloqueio`

---

### 🔴 R5 — Exclusividade do Slot da Turma

* **Definição:** Uma turma só pode ter no máximo um professor alocado por horário/aula.
* **Fórmula no CP-SAT:**
  $$\sum_{p=1}^{P} X_{p,t,d,s} \le 1 \quad \forall t \in T, \forall d \in D, \forall s \in S$$

---

### 🟡 R4 — Célula Vazia (Slot Ocioso)

* **Definição:** Horários em que uma turma não possui professor alocado.
* **Severidade:** AVISO (informativo). Grade com células vazias é considerada parcial.
* **Fórmula no CP-SAT:**
  O solver tenta zerar as células vazias adicionando um ganho positivo $+10$ para cada variável $X_{p,t,d,s} = 1$.

---

### 🟡 R6 — Sai e Volta na Mesma Turma

* **Definição:** Quando o professor leciona em um slot $s$, não leciona em $s+1$, e volta a lecionar na mesma turma em $s+2$.
* **Severidade:** AVISO. Pedagogicamente indesejável para a dinâmica de aula.
* **Fórmula no Validador:**
  $$\text{Grade}[d, s, t] == P \land \text{Grade}[d, s+1, t] \ne P \land \text{Grade}[d, s+2, t] == P$$

---

### 🟢 R7 — Dobradinhas (Aulas Geminadas)

* **Definição:** Preferência por agrupar 2 aulas da mesma disciplina/professor em sequência no mesmo dia.
* **Fórmula no CP-SAT:**
  Criação de variável booleana de bônus $B_{p,t,d,s} \in \{0, 1\}$ com restrição de linearização:
  $$B_{p,t,d,s} \le X_{p,t,d,s}$$
  $$B_{p,t,d,s} \le X_{p,t,d,s+1}$$
  $$B_{p,t,d,s} \ge X_{p,t,d,s} + X_{p,t,d,s+1} - 1$$
  $$\text{Objetivo} += \text{PesoDobradinha} \cdot \sum B_{p,t,d,s}$$

---

## 4. Estrutura Canônica de Resposta da Validação

```json
{
  "validadoEm": "2026-08-31T20:50:00Z",
  "idVersao": 42,
  "isValida": false,
  "totalViolacoes": 1,
  "totalAvisos": 5,
  "totalAulasDistribuidas": 175,
  "totalCelulasVazias": 35,
  "totalProfessores": 24,
  "totalTurmas": 12,
  "totalSlots": 210,
  "violacoes": [
    {
      "codigoRegra": "R1",
      "categoria": "CONFLITO_HORARIO",
      "severidade": "OBRIGATORIA",
      "mensagem": "CONFLITO: Professor 'ALMEIDA' está em 2 turmas ao mesmo tempo (6A, 7B) — segunda, aula 2",
      "diaSemana": "segunda",
      "linha": 2,
      "turma": "6A",
      "professor": "ALMEIDA",
      "professorConflitante": "ALMEIDA"
    }
  ],
  "avisos": [
    {
      "codigoRegra": "R6",
      "categoria": "SAI_E_VOLTA",
      "severidade": "AVISO",
      "mensagem": "SAI E VOLTA: Professor 'CARLOS' leciona na turma 8A nas aulas 1 e 3 da terça-feira",
      "diaSemana": "terca",
      "turma": "8A",
      "professor": "CARLOS"
    }
  ],
  "resumoProfessores": [
    {
      "nomeProfessor": "ALMEIDA",
      "totalAulasDistribuidas": 20,
      "aulasPorDia": { "segunda": 4, "terca": 4, "quarta": 4, "quinta": 4, "sexta": 4 },
      "turmas": ["6A", "7B"],
      "totalViolacoes": 1
    }
  ]
}
```
