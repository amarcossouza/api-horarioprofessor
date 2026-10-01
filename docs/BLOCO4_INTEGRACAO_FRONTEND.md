# Bloco 4 — Integração Frontend ↔ Microsserviço api-cenarios

**Data:** 31/08/2026  
**Status:** ✅ Código criado — aguardando configuração manual e teste

---

## 📁 Arquivos Criados no Frontend

```
C:\projkinix\HorarioProfessor2025_v2\
├── Services/
│   ├── ICenarioApiService.cs     ← Interface + DTOs do microsserviço
│   └── CenarioApiService.cs      ← Implementação HttpClient
└── Controllers/
    └── CenarioController.cs       ← Controller MVC (ponte JS ↔ microsserviço)
```

---

## 🔌 Endpoints Criados no Frontend (CenarioController)

| Método | Rota | O que faz |
|--------|------|-----------|
| `GET` | `/Cenario/Listar?anoLetivo=2025&periodo=MANHA` | Lista cenários da escola |
| `GET` | `/Cenario/Detalhe/5` | Detalhe do cenário com versões |
| `POST` | `/Cenario/Criar` | Cria cenário novo |
| `POST` | `/Cenario/SalvarVersao` | **Captura grade da sessão** → snapshot JSON → salva no microsserviço |
| `POST` | `/Cenario/RestaurarVersao` | **Busca versão** do microsserviço → restaura na sessão |
| `GET` | `/Cenario/Eventos/5` | Timeline de auditoria |

---

## ⚙️ Configurações Manuais Necessárias

### 1. Adicionar URL do microsserviço no `appsettings.Development.json`

Editar `C:\projkinix\HorarioProfessor2025_v2\appsettings.Development.json`:

```json
{
  "Api": {
    "BaseUrl": "http://localhost:5000"
  },
  "ApiCenarios": {
    "BaseUrl": "http://localhost:5010"
  },
  "ConnectionStrings": {
    "ProductionConnection": "Server=SEU_SERVIDOR;Port=3306;Database=SEU_BANCO;User ID=SEU_USUARIO;Password=SUA_SENHA;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 2. Registrar o serviço no `Program.cs`

Editar `C:\projkinix\HorarioProfessor2025_v2\Program.cs`.

**Adicionar estas linhas DEPOIS** de `builder.Services.AddHttpClient();`:

```csharp
// ====================================================================
// HttpClient para o microsserviço api-cenarios (porta 5010)
// ====================================================================
builder.Services.AddHttpClient("ApiCenarios", client =>
{
    var baseUrl = builder.Configuration["ApiCenarios:BaseUrl"] 
        ?? "http://localhost:5010";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<ICenarioApiService, CenarioApiService>();
```

**Adicionar o using** no topo do `Program.cs`:
```csharp
using HorarioProfessorMVC.Services;
```

### 3. Verificar usings no topo do CenarioController

O `CenarioController.cs` usa `HorariosDTO`. Verificar se o namespace está acessível:
```csharp
using HorarioProfessorMVC.Dto;  // onde está o HorariosDTO
```

---

## 🔄 Fluxo Completo — Como Funciona

### Salvar Cenário (novo fluxo):

```
1. Usuário gera grade (botão "Gerar Horários")
   → Frontend chama API Legada (5000)
   → Grade fica na Session["GradeHorarios"]

2. Usuário clica "Salvar como Cenário" (novo botão)
   → JavaScript chama POST /Cenario/Criar
   → CenarioController → CenarioApiService → api-cenarios (5010)
   → Cenário criado no MySQL (hprof_cenario)

3. Usuário clica "Salvar Versão"
   → JavaScript chama POST /Cenario/SalvarVersao
   → CenarioController captura Session["GradeHorarios"]
   → Monta snapshot JSON (5 matrizes + turmas)
   → CenarioApiService → api-cenarios (5010)
   → Versão salva no MySQL (hprof_cenarioversao) com hash SHA-256
```

### Restaurar Cenário (novo fluxo):

```
1. Usuário abre lista de cenários
   → JavaScript chama GET /Cenario/Listar
   → CenarioController → CenarioApiService → api-cenarios (5010)

2. Usuário escolhe uma versão e clica "Restaurar"
   → JavaScript chama POST /Cenario/RestaurarVersao
   → CenarioController busca versão do microsserviço
   → Desserializa snapshot JSON → reconstrói HorariosDTO
   → Grava na Session["GradeHorarios"]
   → Frontend recarrega a grade
```

---

## 📊 Diagrama de Comunicação

```
┌──────────────────┐    HTTP     ┌───────────────────┐    HTTP     ┌──────────────────┐
│  JavaScript      │ ─────────→ │  CenarioController │ ─────────→ │  api-cenarios    │
│  (tela grade)    │            │  (porta 5051)      │            │  (porta 5010)    │
│                  │ ←───────── │                    │ ←───────── │                  │
│  AJAX/fetch      │    JSON    │  + Session          │    JSON    │  + MySQL          │
└──────────────────┘            └───────────────────┘            └──────────────────┘
```

---

## ⚠️ Importante

- **API Legada (5000) NÃO foi alterada** — zero mudanças
- **Fluxo atual continua funcionando** — o novo é um caminho paralelo
- Se o microsserviço `api-cenarios` estiver fora do ar, o sistema funciona normalmente — os endpoints antigos não dependem dele
- O `CenarioController` é independente do `HorarioController` — nenhum método existente foi modificado

---

## ✅ Checklist para Ativar

- [ ] Adicionar `"ApiCenarios": { "BaseUrl": "http://localhost:5010" }` no appsettings.Development.json
- [ ] Adicionar registro do HttpClient + DI no Program.cs
- [ ] Verificar que o frontend compila (`dotnet build`)
- [ ] Rodar microsserviço api-cenarios na porta 5010
- [ ] Rodar frontend na porta 5051
- [ ] Testar: POST /Cenario/Criar via Swagger ou Postman
- [ ] Testar: POST /Cenario/SalvarVersao (com grade na sessão)
- [ ] Testar: POST /Cenario/RestaurarVersao

---

## 🔜 Próximo Bloco (Bloco 5)

Depois de confirmar que a integração funciona:
- Criar os **botões na tela** (Razor/JavaScript):
  - "Salvar como Cenário" 
  - "Listar Cenários Salvos"
  - "Restaurar Versão"
  - "Comparar Versões"
- Criar a **View parcial** para exibir a lista de cenários/versões

**Me avise quando quiser continuar!**
