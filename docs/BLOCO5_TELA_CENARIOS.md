# Bloco 5 — Tela de Cenários (UI)

**Data:** 31/08/2026  
**Status:** ✅ Código criado — aguardando inserção na tela

---

## 📁 Arquivos Criados

```
C:\projkinix\HorarioProfessor2025_v2\
├── Views\Cenario\
│   └── _PainelCenarios.cshtml       ← Partial View com modais e botões
└── wwwroot\js\
    └── cenarios.js                    ← JavaScript com toda a lógica
```

---

## 🎨 O Que Foi Criado na Tela

### Botões (inseridos na barra de ações)
| Botão | Ação |
|-------|------|
| 🟢 **Salvar Cenário** | Abre modal para criar cenário novo ou salvar versão em existente |
| 🔵 **Cenários** | Abre modal com lista de cenários, versões e restauração |

### Modal "Salvar Cenário"
- Radio button: **Criar novo** ou **Salvar em existente**
- Se novo: campo título + descrição
- Se existente: combobox com cenários carregados via API
- Campo: observação da versão
- Captura a grade da sessão automaticamente

### Modal "Listar Cenários"
- Filtros: ano letivo + período
- Tabela com: ID, título, período, ano, status (badge colorido), total versões, data
- Ações por cenário:
  - 🔀 **Ver versões** → abre sub-tabela com versões
  - 📜 **Histórico** → abre modal de timeline de auditoria
  - 🗑️ **Arquivar** → soft delete com confirmação
  
### Sub-tabela de Versões
- Versão, aulas distribuídas, pendentes, conflitos, completo? (badge), data, observação
- Botão **Restaurar** → confirmação → busca snapshot → coloca na sessão → recarrega tela

### Modal "Histórico de Eventos"
- Lista cronológica com ícones por tipo de evento
- Emojis: 🆕 Criado, 💾 Salvo, 🔄 Restaurado, 🚀 Simulação, ✅ Concluído

---

## ⚙️ Como Integrar na Tela

### Passo 1: Inserir a Partial View no `Horarios.cshtml`

Editar `C:\projkinix\HorarioProfessor2025_v2\Views\Horario\Horarios.cshtml`.

**Encontrar esta linha** (linha ~140, logo após os botões existentes):
```html
<button type="button" class="btn btn-primary" onclick="validarProfessoresTodasColunaHorarios()">Validar Todas Opções Professores</button>
```

**Adicionar logo DEPOIS dela:**
```html
@await Html.PartialAsync("~/Views/Cenario/_PainelCenarios.cshtml")
```

### Passo 2: Adicionar o script `cenarios.js`

**Na mesma `Horarios.cshtml`**, no final do arquivo (antes do último `</script>` ou no `@section Scripts`):
```html
<script src="~/js/cenarios.js" asp-append-version="true"></script>
```

**Ou** adicionar no `_Layout.cshtml`, antes do `@await RenderSectionAsync("Scripts"...)`:
```html
<script src="~/js/cenarios.js" asp-append-version="true"></script>
```

### Passo 3: Verificar que `atualizarDistribuicaoGradeHoraria(true)` existe

O JavaScript do `cenarios.js` chama `atualizarDistribuicaoGradeHoraria(true)` após restaurar uma versão para recarregar a grade na tela. Essa função **já existe** no JavaScript atual da tela.

---

## 🔄 Fluxo Visual Completo

```
┌─────────────────────────────────────────────────┐
│             TELA DE GRADE (Horarios)             │
│                                                  │
│  [Processar] [Fechamento] [Ajuste] [Salvar Ajust]│
│  [Validar Opções] [🟢Salvar Cenário] [🔵Cenários]│  ← NOVOS BOTÕES
│                                                  │
│  ┌────────────────────────────────────────────┐  │
│  │           Grade de Horários                │  │
│  │  Seg | Ter | Qua | Qui | Sex              │  │
│  │  ... | ... | ... | ... | ...              │  │
│  └────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────┘
         │                        │
         ▼                        ▼
┌─────────────────┐      ┌─────────────────────┐
│ Modal Salvar    │      │ Modal Listar        │
│                 │      │                     │
│ ○ Criar Novo    │      │ Filtros: ano/período│
│   Título: ___   │      │ ┌─────────────────┐ │
│   Descrição:___ │      │ │ Cenário 1  [👁️]  │ │
│                 │      │ │ Cenário 2  [📜]  │ │
│ ○ Existente     │      │ │ Cenário 3  [🗑️]  │ │
│   [▾ Select ]   │      │ └─────────────────┘ │
│                 │      │                     │
│ Observação: ___ │      │ Versões do Cenário: │
│                 │      │ v1 [✅ Restaurar]    │
│ [Cancelar][💾]  │      │ v2 [✅ Restaurar]    │
└─────────────────┘      └─────────────────────┘
```

---

## ⚠️ O que NÃO foi alterado

- `Horarios.cshtml` → **NÃO FOI EDITADO** (instruções manuais acima)
- `_Layout.cshtml` → **NÃO FOI EDITADO**
- `HorarioController.cs` → **NÃO FOI EDITADO**
- `HomeController.cs` → **NÃO FOI EDITADO**
- Nenhum JavaScript existente → **NÃO FOI ALTERADO**

Todos os novos arquivos são **aditivos** — inseridos via partial view e script separado.

---

## ✅ Checklist

- [ ] Inserir `@await Html.PartialAsync("~/Views/Cenario/_PainelCenarios.cshtml")` no Horarios.cshtml
- [ ] Adicionar `<script src="~/js/cenarios.js">` no Horarios.cshtml ou _Layout.cshtml
- [ ] Verificar que o microsserviço api-cenarios está rodando (porta 5010)
- [ ] Verificar que as configurações do Bloco 4 foram aplicadas no Program.cs
- [ ] Testar: clicar "Salvar Cenário"
- [ ] Testar: clicar "Cenários" → ver lista
- [ ] Testar: ver versões → restaurar
- [ ] Testar: ver histórico de eventos

---

## 🔜 Próximo Bloco (Bloco 6)

**Validador Canônico** — criar serviço de validação independente que:
- Verifica conflitos de horário (professor em 2 turmas no mesmo slot)
- Verifica se bloqueios foram respeitados
- Conta aulas por professor vs. carga cadastrada
- Gera relatório de validação em JSON

**Me avise quando quiser continuar!**
