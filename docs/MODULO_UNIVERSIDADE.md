# Módulo Universidade — Horário Professor

**Data:** 19/10/2026
**Situação:** código escrito, aguardando compilação e teste pelo responsável.
**Regra principal:** o módulo **acrescenta**; o motor antigo, o CP-SAT e as telas das escolas não mudam.

---

## 1. O que o módulo resolve

- O mesmo professor dá aula em mais de um curso da universidade, com uma carga total (ex.: 40 aulas) dividida entre os cursos.
- O sistema impede que ele fique em dois cursos ao mesmo tempo, comparando **pelo horário do relógio** (cada curso pode ter uma grade de horários diferente).
- Os dados entram por três caminhos com a mesma validação, aprovação e auditoria:
  - **API da universidade** (`/api/integracao/v1`);
  - **planilha colada** (Universidade > Colar da planilha);
  - **telas** (Universidade > Professores, Disciplinas e atribuições).
- Visualização e relatórios no formato da universidade: grade por fase, semana do professor, carga por curso, conferência de choques.

## 2. Como os dados se encaixam no que já existe

| Universidade | Sistema atual (sem mudança de estrutura) |
|---|---|
| Instituição (ex.: UDESC CESFI) | `hprof_escolas` com `TipoEscola = 'Universidade'` (escola do login) + `hprof_uni_instituicao` |
| Curso (ex.: Eng. Petróleo) | Uma **escola** em `hprof_escolas` + `hprof_uni_curso` |
| Fase (ex.: 1ª Fase) | Turma em `hprof_turmapadraoescola` (sigla `1F`) + nome completo em `hprof_uni_turma` |
| Disciplina | `hprof_materiaaulasporano` (sigla gerada, ex.: `CDI1`) |
| Professor (pessoa) | `hprof_uni_pessoaprofessor` (matrícula, carga total) |
| Atribuição | `hprof_cadastroprofessor` (`NomeProfessor = "<NOME CURTO>-<SIGLA>"`) + `hprof_uni_pessoacadastro` |
| Indisponibilidade | `hprof_opcaohorario` (1ª opção, `SEG3`, `Bloqueio`/`BL`) |
| Horários do dia | `hprof_gradehorariosconfig.Horarios` (`08:20 - 09:10,...`) |
| Grade publicada | `hprof_horariodistribuicaojson` escolhida em `hprof_uni_publicacao` |
| Horários ocupados da pessoa | `hprof_uni_pessoaocupacao` (dia + hora de início e fim) |

O nome curto da pessoa (texto antes do hífen em `NomeProfessor`) é único na instituição. É por ele que o motor antigo e o CP-SAT reconhecem que várias linhas do cadastro são a mesma pessoa.

- Formato: primeiro e último nome **colados**, sem espaço, hífen ou sublinhado (ex.: `RENATAWOLF-IE`). O motor antigo usa o texto antes do primeiro hífen como a pessoa (`ProfessorHelper.ExtrairNomeProfessor`) e a chave `professor_turma` aceita só 2 ou 3 partes.
- Cada linha do cadastro é gravada como no cadastro manual: `Modalidade = 'SUP'` e `fk_MateriaAulasPorAno` = id da disciplina. As disciplinas ficam com `ModalidadeEscolar = 'SUP'`. O motor busca as disciplinas pela modalidade do cadastro.
- Para processar, a escola da sessão precisa ser o **curso** (Cursos e fases → Abrir no sistema). A escola da instituição não tem professores.

## 3. Processamento: igual ao de hoje

1. Na área Universidade, **Cursos > Abrir no sistema** troca a escola da sessão para o curso. A tela "Processar Horário" de sempre funciona.
2. Processe e salve o curso montado primeiro.
3. Em **Cursos**, publique a grade salva. A publicação grava `hprof_uni_pessoaocupacao`.
4. Abra o próximo curso e processe:
   - **Motor antigo:** `ProfessorServiceUniversidade` envolve o `ProfessorService` e acrescenta ao `DiasNaoTrabalha` os horários ocupados em outro curso (ex.: `TER1,TER2`), além da opção "Acumulo Cargo" em `OpcoesProfessor`.
   - **CP-SAT:** `GradeOrigemSnapshotUniversidade` envolve o `GradeOrigemSnapshotService` e acrescenta regras `BLOQUEIO`, obrigatórias, sigla `AC`. O solver nunca cede esse bloqueio.
5. Para escolas que não são cursos de universidade, os dois envoltórios devolvem exatamente o resultado original. Se as tabelas do módulo ainda não existirem, o erro é registrado no log e o processamento segue sem o AC automático.

**Alterações em arquivos existentes:**
- **API `Program.cs`:** registro dos dois envoltórios e dos serviços do módulo.
- **MVC `Program.cs`:** registro do cliente.
- **`AccountController`:** guarda `TipoEscola` na sessão e leva a universidade direto para a área dela.
- **`_Layout`:** item "Universidade" no menu, visível só para universidades.

**Limites conhecidos do motor antigo** (vão para o CP-SAT numa próxima etapa):
- monta só de segunda a sexta (aula de sábado fica como aviso);
- distribui um professor por aula (no segundo professor, o horário fica bloqueado depois da publicação);
- não tem salas;
- aula dupla é preferência, não regra.

## 4. Integração por API

Cabeçalho obrigatório: `X-Api-Key: <chave da instituição>`. O banco guarda só o SHA-256 da chave (`hprof_uni_integracaocliente.chave_hash`).

| Método | Rota | O que faz |
|---|---|---|
| POST | `/api/integracao/v1/professores` | Recebe professores (`codigo`, `nome`, `documento`, `cargaMaxima`) e, opcionalmente, `vinculos` |
| POST | `/api/integracao/v1/cursos` | Recebe um curso: `curso`, `horariosAula`, `intervalos`, `turmas`, `disciplinas`, `atribuicoes`, `indisponibilidades` |
| GET | `/api/integracao/v1/lotes/{id}` | Situação do envio e mensagens |
| GET | `/api/integracao/v1/grades/{codigoCurso}?anoLetivo=2026&periodo=Manha` | Grade publicada do curso |

Respostas: `202 Accepted` (recebido e validado), `200 OK` (mesmo envio repetido: nada muda), `400` (JSON inválido ou `codigoEnvio` repetido com outro conteúdo), `401` (chave inválida).

Exemplos completos: os três envios gravados pelo `flyway/sql-demo/R__uni_demo_udesc_cesfi.sql`.

### Regras de validação (ERRO bloqueia; AVISO só informa)

- Matrícula obrigatória e única; carga entre 1 e 80.
- A divisão entre cursos não pode passar da carga.
- Disciplina, turma e professor citados precisam existir (no envio ou já cadastrados).
- Soma das aulas da pessoa em todos os cursos ≤ carga total.
- Horários `HH:MM`, em ordem, de 1 a 14 aulas por dia.
- **Avisos:** sábado, dois professores na mesma aula, número ímpar de aulas, disciplina sem professor, soma das atribuições diferente das aulas da disciplina.

### Ciclo de um envio

`RECEBIDO → VALIDADO (ou COM_ERROS) → APLICADO` ou `REJEITADO`.

- **Prévia:** é a própria gravação, feita dentro de uma transação desfeita no fim.
- **Aprovação:**
  - confere o envio de novo, porque o banco pode ter mudado;
  - grava tudo numa transação só;
  - registra cada registro em `hprof_uni_integracaoalteracao`, com o valor de antes e depois.
- **Reenvio completo de um curso:** a atribuição que não veio fica com 0 aulas no cadastro, com aviso. Ela não é apagada.
- **Auditoria:** em `hprof_uni_integracaoauditoria` ficam recebido, validado, visualizado, aprovado, rejeitado, gravado e falhou, com usuário, IP e data.

## 5. Endpoints internos usados pelo MVC (`/api/universidade`)

**Contexto e professores:**
- `contexto?idEscola=`
- `{inst}/cursos`
- `{inst}/painel`
- `{inst}/pessoas`
- `{inst}/pessoas/{id}`
- `POST {inst}/pessoas`
- `{inst}/pessoas/{id}/semana`
- `{inst}/conferencia`

**Curso:**
- `cursos/{idEscola}/atribuicoes`
- `POST cursos/{idEscola}/atribuicoes`
- `cursos/{idEscola}/turmas`
- `cursos/{idEscola}/grades-salvas`
- `cursos/{idEscola}/grade`
- `POST cursos/{idEscola}/publicar`
- `cursos/{idEscola}/bloqueios`

**Planilha:**
- `POST {inst}/planilha/ler`
- `POST {inst}/planilha/enviar`

**Envios:**
- `{inst}/lotes`
- `{inst}/lotes/{id}`
- `POST {inst}/lotes/{id}/aprovar`
- `POST {inst}/lotes/{id}/rejeitar`

## 6. Banco de dados (Flyway)

- **Estrutura:** `flyway/sql/V8` a `V21`. Cada migração cria uma tabela `hprof_uni_*` e não altera nada existente.
- **Demonstração:** `flyway/sql-demo/R__uni_demo_udesc_cesfi.sql` (repetível). Roda só no banco de demonstração; veja `flyway/sql-demo/LEIA-ME.md`. Adiciona:
  - a escola da UDESC (demonstração) e o login `udesc.demo`;
  - a instituição e o cliente de integração;
  - três envios recebidos: 30 professores, Engenharia de Petróleo (PDF de 28/07/2026) e Administração Pública (**simulação, dados fictícios**).

## 7. Roteiro de teste (prova)

1. Compile a API e o MVC.
2. Rode no banco de demonstração: `info` → `validate` → `migrate` → `info`, com `sql` e `sql-demo` em `flyway.locations`.
3. Entre com `udesc.demo` / `udesc2026`. O sistema abre a área Universidade.
4. Em **Envios**, abra os três envios na ordem (professores, Petróleo, Administração). Em cada um:
   - confira a prévia;
   - aprove;
   - veja a auditoria e os registros gravados.
5. Em **Painel da carga**, confira o Carlos Pecorari Neto: 14 aulas em Petróleo, 26 em Administração, 40 de 40.
6. Em **Cursos > Engenharia de Petróleo > Abrir no sistema**:
   - processe no motor antigo e salve;
   - volte à universidade e publique a grade salva.
7. Em **Cursos > Administração Pública**:
   - confira "Bloqueios automáticos (AC)": a terça às 15:20 do Carlos aparece bloqueada (Cálculo III no Petróleo);
   - abra no sistema, processe, salve e publique.
8. Abra **Conferência de choques**: o resultado esperado é 0 choques e ninguém acima da carga.
9. Repita os passos 6 a 8 com o Fechamento Inteligente (CP-SAT) e compare.
10. Imprima a **Grade por fase** e a **Semana do professor**.
11. Teste a planilha colando o modelo: uma linha que passe de 40 aulas deve aparecer com o erro "Passa da carga".

Resultado esperado da simulação feita antes do desenvolvimento:
- **Curso B montado sem a ocupação do A:** 90 de 100 grades com choque.
- **Com a ocupação:** 0 de 100.
