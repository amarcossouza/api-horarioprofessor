# Bloco 3 — Microsserviço API Cenários

**Data:** 31/08/2026  
**Status:** ✅ Código criado — aguardando teste

---

## 📁 Projeto Criado

```
C:\projkinix\api-cenarios\
├── api-cenarios.sln
├── api-cenarios.csproj               ← .NET 8, Pomelo, Swagger
├── Program.cs                         ← Porta 5010, CORS, DI, Swagger
├── appsettings.json
├── appsettings.Development.json       ← ⚠️ Editar usuário/senha MySQL
├── .gitignore
├── Properties/
│   └── launchSettings.json            ← porta 5010
├── Models/
│   ├── Cenario.cs                     ← hprof_cenario
│   ├── CenarioVersao.cs              ← hprof_cenarioversao
│   ├── CenarioRegra.cs               ← hprof_cenarioregra
│   ├── Simulacao.cs                   ← hprof_simulacao
│   └── CenarioEvento.cs              ← hprof_cenarioevento
├── Data/
│   └── CenariosDbContext.cs           ← Mapeamento EF Core completo
├── Dto/
│   ├── CenarioDto.cs                 ← Request/Response de cenário
│   └── VersaoDto.cs                  ← Request/Response de versão
├── Repositories/
│   └── CenarioRepository.cs          ← Acesso a dados (Cenário + Versão + Evento)
├── Services/
│   └── CenarioService.cs             ← Lógica de negócio + auditoria
└── Controllers/
    └── CenariosController.cs          ← 7 endpoints REST
```

---

## 🔌 Endpoints Disponíveis

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/cenarios` | Criar novo cenário |
| `GET` | `/api/cenarios` | Listar cenários (filtros: idEscola, anoLetivo, periodo) |
| `GET` | `/api/cenarios/{id}` | Detalhe do cenário com lista de versões |
| `PUT` | `/api/cenarios/{id}` | Atualizar título/descrição/status |
| `DELETE` | `/api/cenarios/{id}` | Arquivar cenário (soft delete) |
| `POST` | `/api/cenarios/{id}/versoes` | Salvar versão (snapshot JSON da grade) |
| `GET` | `/api/cenarioversoes/{id}` | Obter versão completa com snapshot |
| `GET` | `/api/cenarios/{id}/eventos` | Timeline de auditoria |
| `GET` | `/health` | Health check |

---

## ⚙️ Como Rodar

### 1. Pré-requisitos
- .NET 8 SDK instalado
- MySQL com as 5 tabelas criadas (Bloco 2)

### 2. Configurar conexão
Editar `C:\projkinix\api-cenarios\appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=134.209.164.87;Port=3306;Database=droplexdb;User=SEU_USUARIO;Password=SUA_SENHA;Pooling=true;Max Pool Size=50;"
  }
}
```

### 3. Restaurar pacotes e rodar
```powershell
cd C:\projkinix\api-cenarios

# Restaurar dependências
dotnet restore

# Rodar em desenvolvimento
dotnet run
```

### 4. Testar
Abrir no navegador:
- **Swagger:** http://localhost:5010/swagger
- **Health:** http://localhost:5010/health

### 5. Testar via terminal (PowerShell)

```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:5010/health"

# Criar cenário
$body = @{
    idEscola = 12
    anoLetivo = 2025
    periodo = "Manha"
    titulo = "CEU EMEF MENINOS - Manhã 2025"
    descricao = "Cenário piloto para teste"
    criadoPor = "admin"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5010/api/cenarios" -Method POST -Body $body -ContentType "application/json"

# Listar cenários
Invoke-RestMethod -Uri "http://localhost:5010/api/cenarios?idEscola=12&anoLetivo=2025"

# Obter cenário por ID
Invoke-RestMethod -Uri "http://localhost:5010/api/cenarios/1"
```

---

## 🏗️ Decisões Técnicas

| Decisão | Motivo |
|---------|--------|
| **Porta 5010** | Não conflita com API legada (5000) nem frontend (5051) |
| **CORS para 5051** | Frontend MVC precisa chamar este serviço |
| **camelCase no JSON** | Padrão REST moderno, compatível com JavaScript |
| **Controle otimista (revisao)** | PUT exige enviar a revisão lida → evita sobrescrever edição de outro usuário |
| **Soft delete** | DELETE arquiva em vez de apagar → auditoria preservada |
| **SHA-256 no snapshot** | Verifica integridade do JSON ao restaurar |
| **Health check em `/health`** | Padrão para monitoramento |
| **Scoped DI** | Repository e Service por request — sem singleton compartilhado |

---

## ✅ Checklist para Testar

- [ ] Editar `appsettings.Development.json` com usuário/senha
- [ ] Rodar `dotnet restore`
- [ ] Rodar `dotnet run`
- [ ] Acessar http://localhost:5010/swagger
- [ ] Acessar http://localhost:5010/health
- [ ] POST criar cenário
- [ ] GET listar cenários
- [ ] GET detalhe cenário
- [ ] POST salvar versão com snapshot JSON
- [ ] GET obter versão com snapshot

---

## 🔜 Próximo Bloco (Bloco 4)

Depois de testar e confirmar que está rodando:
- Integrar com o **frontend MVC** — botão "Salvar Cenário" na tela de grade
- Criar a chamada HTTP do frontend para `POST /api/cenarios` e `POST /api/cenarios/{id}/versoes`
- Capturar as matrizes atuais (Seg/Ter/Qua/Qui/Sex) e montar o snapshot JSON

**Me avise quando rodar e testar!**
