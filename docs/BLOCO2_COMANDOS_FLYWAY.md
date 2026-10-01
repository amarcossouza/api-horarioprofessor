# Bloco 2 — Comandos Flyway para Criar as Tabelas

**Data:** 31/08/2026  
**Status:** ✅ Executado e validado no MySQL — schema versão 5 em 01/10/2026

## Resultado da execução de 01/10/2026

- Banco: `droplexdb` — MySQL 8.0.
- Histórico exclusivo: `hprof_flyway_schema_history`.
- Migrações V1, V2, V3, V4 e V5: `Success`.
- O comando `migrate` confirmou: schema atualizado, sem migração pendente.
- Foi executado `repair` somente para normalizar as descrições do histórico criado por uma execução anterior. Nenhuma tabela ou dado de negócio foi apagado ou recriado.
- As regras permanentes de segurança e desenvolvimento estão em [MANIFESTO_DO_PROJETO.md](MANIFESTO_DO_PROJETO.md).

---

## 📁 Arquivos Criados

```
flyway/
├── flyway.conf                              ← configuração (NÃO commitar — tem senha)
└── sql/
    ├── V1__criar_tabela_cenario.sql          ← identidade do cenário
    ├── V2__criar_tabela_cenarioversao.sql    ← versão imutável + snapshot JSON
    ├── V3__criar_tabela_cenarioregra.sql     ← regras e exceções por versão
    ├── V4__criar_tabela_simulacao.sql        ← trabalho de busca/simulação
    └── V5__criar_tabela_cenarioevento.sql    ← trilha de auditoria
```

---

## ⚙️ Antes de Rodar

### 1. Editar credenciais no `flyway.conf`
Abra `C:\projkinix\api-horarioprofessor\flyway\flyway.conf` e preencha:
```
flyway.user=SEU_USUARIO_AQUI
flyway.password=SUA_SENHA_AQUI
```

### 2. Verificar conexão com o banco
Confirme que o MySQL em `134.209.164.87:3306` está acessível e o banco `droplexdb` existe.

### 3. (Recomendado) Fazer backup antes
```bash
mysqldump -h 134.209.164.87 -u SEU_USUARIO -p droplexdb > backup_droplexdb_20260831.sql
```

---

## 🚀 Comandos para Executar

### Comando simplificado deste projeto

No Git Bash, execute a partir da raiz do repositório:

```bash
powershell.exe -ExecutionPolicy Bypass -File ./flyway/run-flyway.ps1 info
powershell.exe -ExecutionPolicy Bypass -File ./flyway/run-flyway.ps1 validate
powershell.exe -ExecutionPolicy Bypass -File ./flyway/run-flyway.ps1 migrate
powershell.exe -ExecutionPolicy Bypass -File ./flyway/run-flyway.ps1 info
```

O script aceita somente `info`, `validate` e `migrate`. Ele não disponibiliza `clean` nem comandos destrutivos e reutiliza a conexão de desenvolvimento do `api-cenarios` sem imprimir a senha.

### Opção A — Com Flyway instalado (recomendado)

Abra o terminal na pasta `C:\projkinix\api-horarioprofessor\flyway\`:

```bash
# 1. Ver o estado atual (quais migrações já foram aplicadas)
flyway -configFiles=flyway.conf info

# 2. Validar os scripts antes de aplicar
flyway -configFiles=flyway.conf validate

# 3. Aplicar as migrações (criar as 5 tabelas)
flyway -configFiles=flyway.conf migrate

# 4. Confirmar que tudo foi aplicado
flyway -configFiles=flyway.conf info
```

### Opção B — Sem Flyway (executar SQL direto no MySQL)

Se não tiver o Flyway instalado, execute os scripts **na ordem** no MySQL Workbench, DBeaver ou terminal:

```bash
# Conectar no MySQL
mysql -h 134.209.164.87 -u SEU_USUARIO -p droplexdb

# Executar na ordem:
source C:/projkinix/api-horarioprofessor/flyway/sql/V1__criar_tabela_cenario.sql
source C:/projkinix/api-horarioprofessor/flyway/sql/V2__criar_tabela_cenarioversao.sql
source C:/projkinix/api-horarioprofessor/flyway/sql/V3__criar_tabela_cenarioregra.sql
source C:/projkinix/api-horarioprofessor/flyway/sql/V4__criar_tabela_simulacao.sql
source C:/projkinix/api-horarioprofessor/flyway/sql/V5__criar_tabela_cenarioevento.sql
```

### Opção C — Via MySQL Workbench / DBeaver

1. Abra a conexão com `droplexdb`
2. Abra cada arquivo `.sql` na ordem (V1 → V2 → V3 → V4 → V5)
3. Execute cada um e confirme que foi criado

---

## ✅ Como Verificar se Deu Certo

Execute no MySQL:
```sql
-- Ver as novas tabelas
SHOW TABLES LIKE 'hprof_cenario%';
SHOW TABLES LIKE 'hprof_simulacao%';

-- Resultado esperado:
-- hprof_cenario
-- hprof_cenarioevento
-- hprof_cenarioregra
-- hprof_cenarioversao
-- hprof_simulacao

-- Ver a estrutura de cada uma (exemplo):
DESCRIBE hprof_cenario;
DESCRIBE hprof_cenarioversao;
```

---

## 📊 Resumo das 5 Tabelas

| Tabela | Propósito | Relação |
|--------|-----------|---------|
| `hprof_cenario` | Identidade (escola/ano/período) | **Raiz** — 1 cenário tem N versões |
| `hprof_cenarioversao` | Snapshot imutável com toda a grade | Pertence a 1 cenário, pode ter versão-pai |
| `hprof_cenarioregra` | Regras e exceções por versão | Pertence a 1 versão |
| `hprof_simulacao` | Trabalho de busca (solver) | Parte de 1 versão-base, gera 1 versão resultado |
| `hprof_cenarioevento` | Auditoria de ações | Registra eventos do cenário/versão/simulação |

### Diagrama de Relacionamento

```
hprof_cenario (1)
    │
    ├──── (N) hprof_cenarioversao
    │              │
    │              ├──── (N) hprof_cenarioregra
    │              │
    │              └──── versao_pai_id → (auto-referência)
    │
    ├──── (N) hprof_simulacao
    │              │
    │              └──── id_versao_resultado → hprof_cenarioversao
    │
    └──── (N) hprof_cenarioevento
                   │
                   ├──── id_versao → hprof_cenarioversao
                   └──── id_simulacao → hprof_simulacao
```

---

## ⚠️ Observações Importantes

1. **`flyway.conf` não deve ir para o Git** — adicione ao `.gitignore`
2. **Os scripts usam `IF NOT EXISTS`** — rodar duas vezes não causa erro
3. **Foreign Keys** — a ordem V1→V5 é obrigatória (V2 depende de V1, V3 depende de V2, etc.)
4. **Engine InnoDB** — necessário para suportar transações e FKs
5. **Charset utf8mb4** — suporta acentos e caracteres especiais
6. **`ON DELETE RESTRICT`** nas FKs críticas — protege contra exclusão acidental de cenários com versões

---

## 🔜 Próximo Passo (após confirmar as tabelas)

Depois que você rodar e confirmar, o **Bloco 3** será:
- Criar os **Models C#** correspondentes no backend
- Criar o **DbContext** atualizado
- Criar o primeiro **endpoint** `POST /api/cenarios` (salvar cenário)

**Me avise quando rodar os scripts e eu sigo com o Bloco 3!**
