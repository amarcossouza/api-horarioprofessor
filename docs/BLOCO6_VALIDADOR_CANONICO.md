# Bloco 6 — Validador Canônico

**Data:** 31/08/2026  
**Status:** ✅ Código criado — pronto para uso com os blocos anteriores

---

## 🎯 O Que É

O **Validador Canônico** é um serviço PURO que analisa uma grade de horários (snapshot JSON) e retorna **todas** as violações de regras obrigatórias e avisos. Não acessa banco, não depende de sessão — recebe dados e retorna resultado.

---

## 📏 Regras Verificadas

| Código | Regra | Severidade | Descrição |
|--------|-------|------------|-----------|
| **R1** | Conflito de Horário | 🔴 OBRIGATÓRIA | Professor em 2 turmas no mesmo dia + mesma aula |
| **R2** | Carga Excedida | 🔴 OBRIGATÓRIA | Mais de 2 aulas do mesmo professor na mesma turma no mesmo dia |
| **R3** | Bloqueio Violado | 🔴 OBRIGATÓRIA | Professor alocado em dia/hora que tem bloqueio registrado |
| **R4** | Célula Vazia | 🟡 AVISO | Slot ativo sem professor (informativo) |
| **R6** | Sai e Volta | 🟡 AVISO | Professor aparece, desaparece e reaparece na mesma turma no mesmo dia |

---

## 📁 Arquivos Criados/Alterados

### No microsserviço `api-cenarios`:
```
Models/
└── ValidacaoResultado.cs         ← ✅ NOVO — modelo do resultado + Violacao + ResumoProfessor

Services/
└── ValidadorGradeService.cs      ← ✅ NOVO — motor de validação (~280 linhas, puro)

Controllers/
└── ValidacaoController.cs        ← ✅ NOVO — 4 endpoints REST

Dto/
└── ValidacaoDto.cs               ← ✅ NOVO — requests de validação

Program.cs                         ← ✅ ALTERADO — +1 linha (registro DI do validador)
```

### No frontend `HorarioProfessor2025_v2`:
```
Services/
├── ICenarioApiService.cs          ← ✅ ALTERADO — +3 métodos + DTOs de validação
└── CenarioApiService.cs           ← ✅ ALTERADO — +3 implementações

Controllers/
└── CenarioController.cs           ← ✅ ALTERADO — +2 endpoints (ValidarGrade, ValidarVersao)

Views/Cenario/
└── _PainelCenarios.cshtml         ← ✅ ALTERADO — +1 botão + modal de resultado

wwwroot/js/
└── cenarios.js                    ← ✅ ALTERADO — +validarGradeAtual() + exibirResultadoValidacao()
```

---

## 🔌 Endpoints da API (microsserviço)

| Método | Rota | Função |
|--------|------|--------|
| `POST` | `/api/validacao/snapshot` | Valida snapshot JSON enviado diretamente |
| `GET` | `/api/validacao/versao/{id}` | Valida versão salva no banco |
| `POST` | `/api/validacao/versao` | Valida versão com bloqueios no body |
| `GET` | `/api/validacao/versao/{id}/resumo` | Resumo rápido (para badges na listagem) |

## 🔌 Endpoints do Frontend (CenarioController)

| Método | Rota | Função |
|--------|------|--------|
| `POST` | `/Cenario/ValidarGrade` | Captura sessão → monta snapshot → valida no microsserviço |
| `GET` | `/Cenario/ValidarVersao/{id}` | Valida versão salva pelo ID |

---

## 🎨 Botão na Tela

```
[Processar] [Fechamento] [Ajuste] [Salvar Ajust] [Validar]
[🟢 Salvar Cenário] [🔵 Cenários] [🟡 Validar Grade]  ← NOVO
```

### Modal de Resultado

```
┌──────────────────────────────────────────────────────┐
│  ✅ Grade Válida!  /  ❌ Grade com Violações         │  ← header colorido
├──────────────────────────────────────────────────────┤
│  [✅/❌] [3 Violações] [12 Avisos] [180 Aulas] ...  │  ← cards resumo
│                                                      │
│  ⚠️ Violações Obrigatórias                           │
│  ┌──────────────────────────────────────────────┐    │
│  │ R1 │ CONFLITO │ Prof X em 2 turmas seg aula1│    │
│  │ R2 │ CARGA    │ Prof Y 3 aulas turma 5A seg │    │
│  └──────────────────────────────────────────────┘    │
│                                                      │
│  ℹ️ Avisos [Mostrar/Ocultar]                         │
│  👤 Resumo por Professor [Mostrar/Ocultar]           │
│  ┌──────────────────────────────────────────────┐    │
│  │ Prof │ Total │ S │ T │ Q │ Q │ S │ Turmas   │    │
│  │ João │  25   │ 5 │ 5 │ 5 │ 5 │ 5 │ 5A,6B    │    │
│  └──────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────┘
```

---

## 📊 Exemplo de Resposta JSON

```json
{
  "validadoEm": "2026-08-31T15:00:00Z",
  "idVersao": null,
  "isValida": false,
  "totalViolacoes": 2,
  "totalAvisos": 15,
  "totalAulasDistribuidas": 180,
  "totalCelulasVazias": 30,
  "totalProfessores": 22,
  "totalTurmas": 10,
  "totalSlots": 210,
  "violacoes": [
    {
      "codigoRegra": "R1",
      "categoria": "CONFLITO_HORARIO",
      "severidade": "OBRIGATORIA",
      "mensagem": "CONFLITO: Professor 'SILVA' está em 2 turmas ao mesmo tempo (5A, 6B) — segunda, aula 3",
      "diaSemana": "segunda",
      "linha": 3,
      "professor": "SILVA"
    }
  ],
  "avisos": [...],
  "resumoProfessores": [
    {
      "nomeProfessor": "SILVA",
      "totalAulasDistribuidas": 25,
      "aulasPorDia": { "segunda": 5, "terca": 5, ... },
      "turmas": ["5A", "6B", "7C"],
      "totalViolacoes": 1
    }
  ]
}
```

---

## ⚙️ Características Técnicas

- **Puro**: não acessa banco, não depende de sessão — fácil de testar
- **Determinístico**: mesma entrada = mesma saída
- **Rápido**: varre as 5 matrizes em uma única passada
- **Extensível**: novas regras são adicionadas sem alterar as existentes
- **Separado do motor legado**: não interfere com `validar_professores_todas_colunanovo`

---

## 🔜 Próximo Bloco (Bloco 7)

**Prova de Conceito do CP-SAT (Google OR-Tools)** — Etapa 3 do planejamento:
- Instalar `Google.OrTools` no microsserviço
- Criar um solver que recebe a grade parcial e tenta distribuir os professores pendentes
- Definir variáveis: `X[professor, turma, dia, linha] ∈ {0,1}`
- Restrições: R1, R2, R3 como hard constraints
- Objetivo: minimizar violações de preferências (soft constraints)
- Retornar a grade otimizada como novo snapshot

**Me avise quando quiser continuar!**
