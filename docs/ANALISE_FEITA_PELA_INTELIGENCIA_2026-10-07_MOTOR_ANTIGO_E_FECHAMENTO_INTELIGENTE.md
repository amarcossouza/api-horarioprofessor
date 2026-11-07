# ANÁLISE FEITA PELA INTELIGÊNCIA — 07/10/2026

## Motor antigo (Processar Horário) × Fechamento Inteligente — Opção 1/2, Acúmulo de Cargo e plano de melhoria

**Escopo:** Escola 94, ano 2025, período Manhã — grade real que gerou a simulação #28 do Fechamento Inteligente (17 turmas × 30 horários = 510 aulas, 65 cadastros, 32 pessoas).
**Método:** dados reais lidos do banco **somente leitura** (nenhuma gravação) e reprocessados localmente com o motor e com protótipos matemáticos (Google OR-Tools). As afirmações sobre o código foram conferidas por verificação independente.
**Projetos envolvidos:** `HorarioProfessor2025_v2` (site), `api-horarioprofessor` (API e motor antigo), `api-cenarios` (motor do Fechamento Inteligente).

---

## 1. Resumo para decisão

| # | Constatação | Evidência |
|---|---|---|
| 1 | O motor antigo respeita 100% das preferências, mas entrega a grade com **12 casas vazias** e **20 aulas no professor/turma errado** | Grade real da sim. #28 |
| 2 | Com a Opção 1 de todos os professores tratada como obrigatória, **não existe grade completa** (prova matemática). Sem as preferências, existe. | OR-Tools, 0,5 s |
| 3 | O Fechamento Inteligente atual **refaz a grade quase do zero**: mantém só ~21% do que o motor antigo fez | 99 a 113 de 498 aulas mantidas |
| 4 | O Fechamento Inteligente atual **cede Acúmulo de Cargo**: 30 das 43 concessões da sim. #28 caíram em horário AC | DEBORA-POR seg/1, ALEX-BIO sex/6, KATIA-EFI seg/6… |
| 5 | A **Opção 2 não é usada** por nenhum dos dois motores | Código e banco |
| 6 | **Com as regras pedidas** (AC intocável, quem tem AC cumpre a Opção 1 ou a 2, os demais cedem com peso por carga, partindo da grade do motor antigo): **510/510 aulas, 0 vazias, 0 AC violado, 0 preferências cedidas, 94% da grade do motor antigo mantida, em 8,4 s** | Protótipo com os dados reais |

**Conclusão:** a meta "o Fechamento Inteligente sempre fecha, com 3 opções" é viável e, com as regras certas, fecha **melhor**: sem ceder acúmulo de cargo e mexendo pouco na grade do motor antigo.

---

## 2. Missão de cada motor (definida pelo usuário)

- **Motor antigo (Processar Horário):** entregar a melhor distribuição possível ao Fechamento: menos buracos, carga certa e preferências respeitadas.
- **Fechamento Inteligente:** **sempre** apresentar **3 opções** de horário **100% fechadas**, sem casas vazias, sem a mesma pessoa em duas turmas e com a carga exata, aproveitando ao máximo a grade recebida. Ele foi criado para isso.
- **Regras do Fechamento:**
  - **Quem tem AC (Acúmulo de Cargo):** cumpre a **1ª opção ou a 2ª opção inteira**, sem nenhuma concessão.
  - **Os demais:** podem ceder espaço para o horário fechar, de preferência os de **carga mais cheia**.
  - Toda preferência não atendida aparece no relatório como **"Preferência cadastrada não atendida"**, como já acontece hoje.

### Vale para qualquer escola

A escola 94 foi usada só como **dado real de teste**. Nenhuma regra, peso ou correção depende dela:

- **Pessoa:** o nome antes do hífen, para todos os cadastros de qualquer escola.
- **AC:** identificado pela sigla `AC`, gravada da mesma forma em todas as escolas. No banco inteiro são 8.731 células AC, em 24 escolas.
- **Opção 1/Opção 2:** lidas do cadastro de cada escola, ano e período.
- **Peso por carga:** calculado por pessoa a partir do cadastro da própria escola.
- **Horários:** quantidade de dias × aulas vinda da configuração da escola (manhã, tarde ou noite).

**Antes de publicar**, a validação vai reprocessar grades reais de **várias escolas e períodos**, além da escola 94.

**Ponto de atenção geral:** a tela de opções lê só 1 dígito da aula (`ObterLinha`). Se alguma escola tiver 10 ou mais aulas no período, isso precisa ser corrigido.

---

## 3. Já corrigido hoje (07/10): mesma pessoa em duas disciplinas

- **Problema:** "PRISCILA-POR" e "PRISCILA-LEI" são a mesma professora. O Fechamento Inteligente as tratava como duas pessoas e chegou a colocá-las no mesmo horário, na segunda, aula 2, turmas 1D e 1E.
- **Correção:** em `api-cenarios`, a pessoa passa a ser o nome antes do hífen, mesma regra do motor antigo (`ProfessorHelper.ExtrairNomeProfessor`). A regra vale na coloração exata, no CP-SAT, no reparo residual e no Validar Grade. Há também uma **trava final** que nunca entrega opção com conflito de pessoa ou casa vazia.
- **Resultado com as 6 simulações reais (#23 a #28):** o motor antigo tinha de **14 a 19 conflitos por opção**; o novo tem **0**. Em todas: 0 vazias e a carga dos 65 cadastros igual ao pedido.
- **Testes:** 56 de 56 passando (5 novos com o caso PRISCILA).
- **Pacote:** `C:\projkinix\api-cenarios\bin\Release\net8.0\linux-x64\publish\` → `/var/www/horarioprofessor-cenarios`.

---

## 4. O que o motor antigo entrega hoje (grade real da sim. #28)

| Indicador | Motor antigo |
|---|---|
| Casas preenchidas | 498 de 510. **12 vazias**: aula 4 = 2, aula 5 = 3, aula 6 = 7; turmas 1C, 1D(2), 2B, 2C, 2E, 3D(3), 3E(3) |
| Aulas faltando por professor/turma | 15 pares, **16 aulas** (ex.: MARIAELIS-OLP/3D 0 de 2; ANDERSON-MAT/3E 3 de 4) |
| Aulas sobrando | 4 pares, **4 aulas** (ERIC-ORA/3D, ERNANI-EFI/2A, MERCEDES-EFI/1A, REYNALDO-EDU/3C) |
| Mesma pessoa em 2 turmas | 0 |
| Preferências desrespeitadas | **0 de 269** |
| Mais de 2 aulas do mesmo professor/turma/dia | 0 |
| Janelas (horário vago entre aulas da mesma pessoa) | 44 |

**Leitura:** o motor antigo é bom em respeitar preferências, mas não fecha. Para respeitar tudo, ele deixa buracos e põe algumas aulas no lugar errado.

---

## 5. Fato matemático: com a Opção 1 obrigatória para todos, a grade perfeita não existe

| Teste com os dados reais | Resultado |
|---|---|
| Todas as 269 restrições da Opção 1 obrigatórias, mais as regras fixas | **INVIÁVEL** (provado em 0,5 s) |
| Sem nenhuma preferência, só as regras fixas (pessoa, carga exata, máx. 2/turma/dia) | **VIÁVEL** (0,4 s) |

Regras fixas: a mesma pessoa em no máximo 1 turma por horário, a carga exata de cada cadastro em cada turma e no máximo 2 aulas por turma por dia.

**Consequência:** nenhum motor fecha 100% respeitando a Opção 1 de todo mundo. Fechar exige usar a Opção 2 e/ou ceder algumas preferências, e **essa é a missão do Fechamento Inteligente**.

---

## 6. Comparação de estratégias de fechamento (mesmos dados reais)

Coluna "Mantidas" = aulas mantidas da grade do motor antigo, de 498.

| Estratégia | Fecha 100% | Mantidas | Preferências cedidas | AC cedido | Janelas | Tempo |
|---|---|---|---|---|---|---|
| **Fechamento atual** (coloração exata, sim. real) | ✅ | 99 a 113 (≈21%) | 13 a 16 | **7 a 12** | 66 a 72 | 3 a 15 s |
| Reparo residual partindo do motor antigo (já existe no código, mas não é usado nessa escola) | ✅ | 433 (87%) | 11 | não medido | 48 | 0,2 s |
| Protótipo C: "como hoje", mas otimizado de verdade (Opção 1 de todos flexível, mesmo peso) | ✅ | 456 (92%) | 1 | **1 (KATIA ter/6)** | 37 | 14 s |
| Protótipo B: AC intocável, só Opção 1, peso por carga | ✅ | 454 (91%) | 1 (VAGNER ter/6 BL) | **0** | 37 | 14 s |
| **Protótipo A: REGRAS DO USUÁRIO** (AC intocável; quem tem AC cumpre a Opção 1 ou a 2; demais cedem com peso por carga; Opção 2 disponível) | ✅ | **468 (94%)** | **0** | **0** | 43 | **8,4 s** |

No protótipo A, escolheram a Opção 2: **DEBORA e MARIAELIS** (com AC) e **REYNALDO**. Os demais ficaram na Opção 1.

**Conclusões:**
1. O maior ganho vem de o Fechamento **partir da grade do motor antigo e otimizar** (CP-SAT completo com bônus por manter cada aula), em vez de reconstruir a grade.
2. A **Opção 2 resolve o que antes exigia concessão**: com ela, ninguém precisou ceder.
3. Sem a regra de AC, até o melhor otimizador escolhe ceder um horário de AC (protótipo C). **O AC precisa ser regra dura.**

> Observação: o protótipo rodou neste computador com 8 núcleos. No servidor (1,9 GB de RAM) é preciso limitar os workers (2 a 4) e medir o tempo de novo.

---

## 7. Pergunta: "A Opção 2 é considerada no motor antigo?"

**Resposta: não.** E também não é considerada no Fechamento Inteligente.

**Motor antigo:**
- Copia só a Opção 1 para a tabela de processamento (`PrincipalExecucaoController.cs:99-103`; `OpcoesProfessorService.cs:319-320`).
- Ao ler, força tudo como Opção 1 (`ProfessorService.cs:67, 78-88`).
- A "segunda rodada" com a Opção 2 está **comentada** desde o primeiro commit (`PrincipalExecucaoController.cs:108-129`). O método por professor (`:977-1013`) é código morto: a chamada está comentada na linha `:159`.
- Mesmo se fosse reativada, não funcionaria para cadastros com hífen. `ObterProfessoresExcluidos` (`:951-972`) corta o nome no hífen e não encontra "PRISCILA-POR", então a lista fica vazia e a consulta vira `NOT IN (NULL)`.

**Fechamento Inteligente:**
- Lê as regras de `hprof_opcaohorarioprocessamento`, que só tem a Opção 1 (`GradeOrigemSnapshotService.cs:45-68`).
- Quando a Opção 2 chega, vira "PREFERENCIA" não obrigatória (`:128, 139-140`) e é descartada (`PreparacaoSolverFactory.cs:47-59`).

**O que é a Opção 2:** um **plano B completo** de indisponibilidades do próprio professor, que substitui a Opção 1 quando ela não cabe. Não é uma lista de desejos leves.

**Pode ser usada? Sim.** O melhor lugar é o **Fechamento Inteligente**, onde o otimizador escolhe, por pessoa, a Opção 1 ou a 2. Usar a Opção 2 **não é concessão**, porque foi o próprio professor quem cadastrou. Reativar no motor antigo é possível, mas mais lento e arriscado: apaga e regrava a tabela de processamento em produção a cada tentativa.

**Dados da escola 94/2025/Manhã:**
- 34 cadastros têm Opção 2 real, mas 18 deles com a Opção 2 **igual** à Opção 1.
- Dos 16 diferentes, **12 têm só 1 horário** (quase sempre SEG1), o que parece preenchimento só para salvar a tela.
- Só **4** têm uma alternativa de fato: GENILDA-LEI, DEBORA-LEI, MERCEDES-EFI e MERCEDES-MAT.

---

## 8. Acúmulo de Cargo (AC)

**Como é gravado:**
- Não há coluna própria: é uma célula da grade de opções com `Sigla='AC'` / `Tipo='Acumulo Cargo'`.
- Na escola 94, **7 pessoas (18 cadastros)** têm AC: ALEX, CARLA, DEBORA, DOSILIA, KATIA, MARIAELIS e MERCEDES. São 78 horários de AC por pessoa na Opção 1.

**Hoje:**
- **Motor antigo:** AC é bloqueio rígido, igual a BL/HL/ATPC. Mas o endpoint `horariosFechamento` apaga tudo do processamento, menos ATPC (`OpcoesProfessorService.cs:130-134`), e a linha que preservaria o AC está comentada (`:132`). Nesse fluxo, o AC **não** é protegido.
- **Fechamento Inteligente:** o AC chega sem a sigla e vira um bloqueio comum. Na coloração exata, todo bloqueio é sempre flexível (-100000 fixo). **Na sim. #28, 30 das 43 concessões caíram em AC.**

**Bug na tela (confirmado no banco):**
- A tela deveria copiar o AC da Opção 1 para a Opção 2, mas a cópia não funciona:
  - o listener é anexado antes de a grade existir;
  - os dados são enviados antes da cópia.
- No banco há **4.505 células AC da Opção 1 sem AC na Opção 2**.
- **Regra adotada:** o AC físico é o da **Opção 1**, sempre intocável. Um AC que só aparece na Opção 2 vale apenas se ela for escolhida.

**Pessoas com AC no limite** (livre = 30 − carga − horários AC):

| Pessoa | Livre |
|---|---|
| DEBORA | 2 |
| KATIA | 2 |
| DOSILIA | 3 |
| CARLA | 5 |
| MARIAELIS | 5 |

---

## 9. "Ceder primeiro quem tem aulas cheias"

**Interpretação:** pessoa (somando todos os cadastros) com carga próxima de todos os horários livres. Peso da concessão = base + valor × horários livres, ou seja, **quanto mais cheia a carga, mais barato ceder**.

**Descoberta na escola 94:**
- As 9 pessoas com carga cheia ou quase (ANDERSON 30, ANTONIO 30, ELIANA 30, FABIANA 30, JOAO 30, CRISTINA 28, ERNANI 28, GILMAR 28, MARIA 28) **não têm nenhuma restrição cadastrada**.
- Elas "cedem" naturalmente sendo **movidas** de lugar, o que é livre.
- As concessões, quando necessárias, recaem nos demais. Com o peso por carga, quem cede é o de carga maior: no protótipo B foi VAGNER (24 aulas), e não um professor com poucas aulas.

---

## 10. Problemas encontrados no motor antigo (conferidos no código)

| # | Problema | Local | Efeito |
|---|---|---|---|
| 1 | A distribuição inicial **para na quarta-feira** | `AppState.cs:2002` | quinta e sexta só recebem aulas nos consertos, o que gera buracos |
| 2 | **Troca entre 3 professores grava o professor errado** quando há mais de um candidato | `TrocaTresProfessores/TrocasEntreTresProfessores.cs:390-405` | +1 aula para um e −1 para outro (as "4 sobrando / 16 faltando") |
| 3 | `ReagendarHorariosNaoDistribuidos` recebe o **nome do dia** no lugar da turma | `AppState.cs:3383` | a etapa nunca coloca ninguém |
| 4 | Realocação por coluna **nunca roda** (`_appState` nulo, exceção engolida) | `AppState.cs:148, 2805`; `HorarioRealocacaoPorTurma.cs:316` | etapa morta |
| 5 | Professores são distribuídos em **ordem alfabética** | `AppState.cs:1295-1298` | os mais difíceis ficam por último |
| 6 | Aula **sobrando nunca é detectada** (só a faltando) | `Contabilizar_Aulas.cs:307` | o erro passa adiante |
| 7 | O construtor dispara um processamento paralelo com parâmetros fixos (2024) | `AppState.cs:167, 210` | risco de estado misturado |
| 8 | Classe de 7.555 linhas com estado compartilhado | `AppState.cs` | cada mudança exige medição antes/depois |

---

## 11. Problemas de cadastro e dados encontrados

| Problema | Detalhe |
|---|---|
| Turmas cujo cadastro não soma 30 aulas | **2C = 31 e 3D = 29**. O Fechamento corrige em memória, mas ninguém avisa a escola |
| AC não espelhado na Opção 2 | Bug da tela (seção 8): 4.505 células no banco |
| Opção 2 "de enfeite" | 12 dos 16 cadastros com Opção 2 diferente têm só 1 horário (SEG1) |
| Linhas com `IdEscola = 0` | 477 linhas apontando para 48 cadastros da escola 94 (1.406 no banco inteiro). Hoje são ignoradas |
| Cadastros sem nenhuma opção | 11 (ex.: ANTONIO-SOC, ERNANI-EFI, PRISCILA-ING) |
| Regras do Fechamento dependem do último botão usado no motor antigo | Depois de `horariosFechamento`, a tabela de processamento fica só com ATPC, e o Fechamento deixaria de ver AC/BL/HL |

---

## 12. PLANO — o que vamos fazer

### Etapa 1 — Fechamento Inteligente com as regras novas (maior ganho, prioridade máxima)

1. **Regras completas chegando ao motor** (`api-horarioprofessor`, `GradeOrigemSnapshotService`):
   - ler sempre `hprof_opcaohorario`, com a Opção 1, a Opção 2 e a sigla, em vez da tabela de processamento;
   - a Opção 2 deixa de ser descartada.
2. **Motor (`api-cenarios`):**
   - **AC intocável**, por pessoa (todos os cadastros da mesma pessoa), em todos os caminhos e na trava final.
   - **Quem tem AC cumpre a Opção 1 OU a Opção 2 inteira** (escolha feita pelo otimizador).
   - **Demais professores:** podem usar a Opção 2 e podem ceder, com custo menor para quem tem carga mais cheia.
   - **Opção A ("Ajuste mínimo"):** otimização completa partindo da grade do motor antigo, que é o protótipo A.
   - **Opções B e C:** mesma regra, com variação real (outra ênfase: dobradinhas ou janelas) e diferença mínima garantida em relação à A.
   - **Garantia de fechamento:** se a otimização não terminar no tempo, a coloração exata (já com AC duro) fecha a grade. **Sempre saem 3 opções.**
   - **Trava final:** nenhuma opção sai com casa vazia, conflito de pessoa ou AC violado.
   - **Relatório de cada opção:** preferências não atendidas (como hoje), opção escolhida por professor e carga de quem cedeu.
3. **Site:** levar os campos novos até o motor e mostrar a opção escolhida (1 ou 2) por professor.
4. **Validação:** testes automáticos mais o reprocessamento de grades reais de **várias escolas e períodos** (manhã, tarde e noite), comparando antes e depois (como foi feito hoje com a escola 94).
5. **Servidor:** limitar a 2 a 4 workers, por causa da RAM de 1,9 GB, e medir o tempo lá.

### Etapa 2 — Motor antigo entregando melhor

Uma correção por vez, medindo com a grade real antes e depois:

1. Distribuição inicial nos 5 dias (problema 1).
2. Troca entre 3 professores gravando o professor certo (problema 2).
3. Reagendamento com a turma correta (problema 3).
4. Ordem de distribuição pelos mais difíceis primeiro: mais restrições e menos horários livres (problema 5).
5. Detectar e corrigir aula sobrando (problema 6).

### Etapa 3 — Cadastro e tela

1. Corrigir a cópia do AC da Opção 1 para a Opção 2 na tela `OpcaoHorario.cshtml`.
2. Avisar quando o cadastro de uma turma não somar o total de horários.
3. Avisar sobre a Opção 2 "de enfeite" (1 horário só).
4. Analisar com a escola as linhas `IdEscola = 0` antes de qualquer limpeza.

### Cuidados

- Publicar os **3 projetos juntos** (site, API e api-cenarios), porque os campos novos precisam passar pelos três.
- O banco é de **produção**: testes de gravação só com autorização.
- A tela do Diário Oficial depende do Chrome no servidor (fora deste escopo).

---

## 12A. IMPLEMENTADO em 07/10/2026 — Etapa 1 (Fechamento Inteligente)

| Projeto | Arquivo | O que mudou |
|---|---|---|
| api-horarioprofessor | `Services/GradeOrigemSnapshotService.cs` | Regras sempre do cadastro `hprof_opcaohorario` (1ª e 2ª opção, com sigla), não mais da tabela de processamento |
| api-cenarios | `Models/SolverModels.cs` | `BloqueioSolver` ganhou `Opcao` (1/2) e `Sigla`; `SolverOutput` ganhou `PessoasComOpcao2` |
| api-cenarios | `Services/PreparacaoSolverFactory.cs` | A 2ª opção (prioridade 2) e a sigla chegam ao motor; `REMOVE` é ignorado |
| api-cenarios | `Services/RegrasPorPessoa.cs` (novo) | AC (da 1ª opção), 1ª e 2ª opção e carga **por pessoa** |
| api-cenarios | `Services/SolverCpSatService.Otimizado.cs` (novo) | Fechamento otimizado partindo da grade recebida (detalhes abaixo) |
| api-cenarios | `Services/SolverCpSatService.cs` | Ver detalhes abaixo |
| api-cenarios | `ProcessamentoLegadoAdapter`, `AjusteCirurgicoService`, `FechamentoInteligenteFacade` | Copiam opção e sigla; a validação usa só a 1ª opção como bloqueio |
| HorarioProfessor2025_v2 | `Services/ICenarioApiService.cs` | `BloqueioSolverDto` repassa `Opcao` e `Sigla` |

**Fechamento otimizado** (`SolverCpSatService.Otimizado.cs`), partindo da grade recebida:
- AC duro;
- quem tem AC cumpre a 1ª **ou** a 2ª opção;
- os demais cedem com custo menor quanto mais cheia a carga;
- bônus por manter cada aula;
- ênfases: aulas duplas (B) e menos janelas (C);
- diferença mínima entre as 3 opções.

**Mudanças em `SolverCpSatService.cs`:**
- As 3 opções usam o otimizador, com a coloração exata (agora com AC duro e até 5 sorteios) como garantia.
- Concessões informadas nunca liberam AC.
- CP-SAT residual com AC duro.
- A entrada residual tira da grade as aulas que caíram em AC.
- Trava final: sem casa vazia, sem conflito de pessoa e sem AC ocupado.
- A descrição de cada opção mostra o % real mantido, as preferências não atendidas e quem usou a 2ª opção.

**Testes automáticos:** 61 de 61 passando. Novos casos genéricos:
- AC nunca ocupado nas 3 opções;
- quem tem AC usa a 2ª opção quando a 1ª não cabe;
- quem cede é o de carga mais cheia;
- AC respeitado mesmo com concessões automáticas;
- a preparação traz a 2ª opção e a sigla.

**Validação com dados reais** (motor de verdade, somente leitura, 30 s por opção). Escola 94 Manhã, simulações #24 e #28:

| | Opção 1 | Opção 2 | Opção 3 |
|---|---|---|---|
| Vazias / conflitos / AC ocupado / carga errada | 0 | 0 | 0 |
| Preferências não atendidas | 0 | 0 | 0 |
| Grade recebida mantida | 89–91% | 65–67% | 84–89% |
| Tempo total das 3 opções | ~90 s | | |

Antes eram 13 a 16 preferências não atendidas e 7 a 12 AC ocupados por opção.

---

## 13. Como foi medido

- Simulações reais do Fechamento Inteligente lidas de `hprof_simulacao.parametros_json` e das alternativas em `hprof_simulacaoalternativa` (somente `SELECT`).
- Opções lidas de `hprof_opcaohorario` (escola 94, 2025, Manhã, `Ativo = 1`, sem `REMOVE`).
- Motor corrigido e protótipos executados localmente, sem gravar nada.
- Ferramentas temporárias de medição (pasta temporária da sessão): reprocessamento das simulações, análise da grade do motor antigo e protótipo das regras (CP-SAT com x[cadastro, turma, horário], escolha de opção por pessoa e peso por carga).
