# Roteiro de Implementação e Status — Novo Fechamento de Horários

**Documento vivo do projeto**  
**Atualizado em:** 02/10/2026  
**Responsabilidade:** este arquivo deve ser atualizado ao concluir cada bloco de trabalho.  
**Regras obrigatórias:** [MANIFESTO_DO_PROJETO.md](MANIFESTO_DO_PROJETO.md)

## 1. Objetivo

Entregar um processo de fechamento que:

1. carregue do banco os professores, turmas, aulas, bloqueios e preferências;
2. mantenha o processamento em memória durante a simulação;
3. preserve as matrizes legadas apenas como entrada e saída do sistema atual;
4. permita bloquear, desbloquear e conceder exceções sem alterar imediatamente o cadastro original;
5. execute várias simulações com Google OR-Tools CP-SAT;
6. apresente três alternativas reais e comparáveis;
7. permita escolher, salvar, restaurar, comparar e publicar uma versão;
8. permita ajustes cirúrgicos posteriores sem reorganizar professores que não fazem parte do pedido;
9. preserve histórico, autoria, vigência, justificativas e possibilidade de desfazer.

## 2. Onde estamos

### Situação resumida

O projeto está no **Marco D — Fechamento inteligente**. Os Blocos 0 a 6 estão concluídos e o próximo trabalho é permitir ajustes cirúrgicos posteriores sem reorganizar o restante da grade.

As estruturas principais já possuem código, mas ainda não devem ser consideradas prontas para produção. Existe um protótipo funcional do versionamento, validador, CP-SAT, comparação, concessões e geração de três alternativas. Agora precisamos proteger esse código, criar testes, encapsular o que já funciona e provar o fechamento com dados reais. Não haverá reescrita geral nem divisão do código existente apenas por organização.

### Evidências verificadas até 02/10/2026

| Verificação | Resultado |
|---|---|
| API principal `api-horarioprofessor` | Compila: 0 erros e 0 avisos |
| Motor `api-cenarios` | Compila: 0 erros e 0 avisos |
| Frontend `HorarioProfessor2025_v2` | Compila: 0 erros e 0 avisos |
| API principal | Swagger respondeu na porta 5000 |
| Motor de cenários | Swagger respondeu na porta 5010 |
| Frontend | Respondeu na porta 5051 |
| CP-SAT | Endpoint de status respondeu `operacional`, OR-Tools 9.10 |
| Flyway | V1 a V7 validadas e aplicadas com sucesso; schema `droplexdb` na versão 7 |
| Persistência | `GET /api/Cenarios` acessou o banco e retornou cenários existentes |
| Restauração/leitura | Cenário 2 retornou duas versões salvas |
| Validador | Versão 2: 280/280 aulas, 0 violações e 18 avisos R6 |
| Três caminhos reais | Simulação 2 do cenário 4: ALT_A, ALT_B e ALT_C persistidas com três hashes distintos |
| Diversidade medida | ALT_B difere em 393 células da anterior; ALT_C difere em 402 células da anterior |
| Regressão do motor | 45/45 testes automatizados aprovados |
| Diagnóstico de concessões | 48/48 testes; bloqueios tratados como exceções simuladas, sem alterar o cadastro original |
| Caso real corrigido | Regentes de turma única passaram a usar limite diário compatível com a carga semanal |
| Fechamento real | Simulação 5: ALT_A, ALT_B e ALT_C distintas, todas com 360/360 aulas e nenhuma concessão necessária |

### O que ainda não foi comprovado

- aceite do gestor sobre a qualidade pedagógica e operacional das três alternativas;
- respeito a todos os bloqueios e preferências provenientes do cadastro;
- concessão dirigida validada no piloto escolhido pelo gestor;
- adoção e restauração ponta a ponta pelo frontend;
- publicação oficial com referência persistente e vigência completa;
- ajuste cirúrgico posterior preservando o restante da grade;
- testes automatizados de regressão, integração e concorrência;
- recuperação segura após reiniciar os três serviços.

### Processamento que já existe no fonte

O fluxo abaixo já possui implementação, embora parte ainda precise de testes e refatoração:

1. o sistema legado carrega cadastros e produz a grade em matrizes;
2. o frontend mantém a grade atual na sessão;
3. `CenarioController` transforma a grade da sessão em snapshot JSON;
4. `api-cenarios` cria cenário e versões imutáveis;
5. `ValidadorGradeService` verifica conflitos, carga, bloqueios e avisos;
6. `SolverCpSatService` monta e executa o modelo CP-SAT;
7. o solver possui código para gerar três alternativas;
8. o frontend possui tela para visualizar, comparar e adotar uma alternativa;
9. o sistema possui código para regras/exceções e publicação;
10. eventos registram parte da auditoria do cenário.

Isso representa um **protótipo funcional adiantado**, não a conclusão formal das fases. O código precisa ser protegido, testado, encapsulado e validado com o caso real, preservando as classes existentes que já funcionam.

### Painel mestre — 11 blocos até a conclusão

O roteiro completo possui **11 blocos, numerados de 0 a 10**. Os **Blocos 0 a 6 foram concluídos** e o próximo trabalho é o **Bloco 7**, com protótipo parcial já existente para o bloco 9. O envio do novo repositório `api-cenarios` à nuvem depende apenas da definição de seu endereço remoto.

| Bloco | Fase | Situação em 01/10/2026 | Entrega que encerra a fase |
|---:|---|---|---|
| 0 | Proteção e linha de base | **Concluído localmente** | Três fontes em Git, novos arquivos sem segredos e três builds reproduzíveis; remoto do `api-cenarios` ainda deve ser cadastrado |
| 1 | Testes de caracterização | **Concluído — 22 testes aprovados** | Testes preservam validador, solver, comparação, versões, restauração, endpoints, concessões e ajustes localizados |
| 2 | Encapsulamento seguro | **Concluído — 36 testes aprovados** | Código existente preservado e acessado por adapters/fachadas e endpoint leve |
| 3 | Persistência completa | **Concluído — 42 testes e ciclo MySQL aprovados** | V6/V7 aplicadas; três alternativas e publicação persistidas |
| 4 | Snapshot completo das regras | **Concluído — 44 testes e integração real** | Motor recebe matriz, IDs, bloqueios, preferências, travas e origem |
| 5 | Três caminhos de fechamento | **Concluído — 45 testes e caso real persistido** | Três soluções distintas, válidas, persistidas e explicadas |
| 6 | Viabilidade e concessões | **Concluído — 48 testes e simulação real** | Sistema informa exatamente quais restrições liberar para fechar 100% |
| 7 | Ajuste cirúrgico | Pendente | Mudança localizada sem alterar o restante da grade |
| 8 | Execução longa | Pendente | Simulações em segundo plano, retomáveis e acompanháveis |
| 9 | Frontend final | Protótipo parcial | Tela modular para comparar, decidir, publicar, restaurar e desfazer |
| 10 | Piloto e aceite | Pendente | Caso real aprovado, reinício/restauração testados e aceite do gestor |

**Contagem objetiva:** são 11 blocos no total. Os Blocos 0 a 6 estão concluídos; restam 4 blocos formais, do Bloco 7 ao aceite final. Como parte do código futuro já existe, alguns blocos serão trabalho de teste, correção e organização, não implementação do zero.

### Marcos de controle

| Marco | Blocos envolvidos | Resultado esperado |
|---|---|---|
| Marco A — Fonte protegido | 0 | Nenhuma perda possível e estado reproduzível |
| Marco B — Núcleo confiável | 1 e 2 | Código existente testado e protegido por contratos/adapters |
| Marco C — Estado persistente | 3 e 4 | Dados completos, versões e alternativas restauráveis |
| Marco D — Fechamento inteligente | 5, 6 e 7 | Três caminhos, liberações explicadas e ajuste cirúrgico |
| Marco E — Operação do gestor | 8 e 9 | Simulação acompanhável e decisão completa pela tela |
| Marco F — Entrega | 10 | Piloto real aprovado e sistema pronto para uso controlado |

### Regra para avançar de bloco

Um bloco somente muda para `Concluído` quando:

1. o fonte estiver salvo no Git;
2. os três projetos compilarem;
3. os testes previstos passarem;
4. a integração pela API for comprovada;
5. o banco preservar os dados existentes;
6. a evidência e o próximo passo forem registrados neste documento.

## 3. Componentes e responsabilidades

| Componente | Caminho | Porta | Responsabilidade |
|---|---|---:|---|
| API principal/legada | `C:\projkinix\api-horarioprofessor` | 5000 | Ler cadastros e executar o processamento legado existente |
| Motor de cenários | `C:\projkinix\api-cenarios` | 5010 | Versionar, validar, simular, comparar, conceder exceções e publicar |
| Frontend MVC | `C:\projkinix\HorarioProfessor2025_v2` | 5051 | Interface do gestor; não deve conter regra pesada de fechamento |
| Banco | `droplexdb` | 3306 | Cadastros originais e tabelas isoladas `hprof_` |

### Fluxo pretendido

```text
Cadastros hprof_ e regras existentes
             |
             v
API principal / motor legado
             |
             v
Snapshot canônico da grade
             |
             v
API de cenários: versão imutável + regras + travas
             |
             v
Validador canônico
             |
             v
CP-SAT: estrito -> concessões autorizadas -> ajuste localizado
             |
             v
Três alternativas persistidas e comparáveis
             |
             v
Escolha do gestor -> publicação com vigência
             |
             v
Restauração ou novo ajuste cirúrgico posterior
```

## 4. Matriz legada e modelo canônico

As matrizes continuarão sendo usadas para compatibilidade com o processamento e a tela atuais. Elas não serão o modelo central do novo motor.

O novo núcleo trabalhará com identificadores e objetos canônicos:

- `GradeSnapshot`;
- `SlotHorario`;
- `AlocacaoAula`;
- `ProfessorSnapshot`;
- `TurmaSnapshot`;
- `DisciplinaSnapshot`;
- `RestricaoProfessor`;
- `PreferenciaProfessor`;
- `ConcessaoAutorizada`;
- `TravaAjuste`.

Conversores isolados farão:

```text
matrizes legadas -> GradeSnapshot -> solver/validador -> GradeSnapshot -> matrizes/tela
```

Não devemos usar apenas o nome do professor como identidade. O modelo canônico precisa conservar IDs de escola, professor, turma, disciplina, período e slot.

### 4.1 O que será reaproveitado

Não será criado um sistema totalmente separado do que já funciona. Serão reaproveitados:

- leitura atual dos cadastros e tabelas `hprof_`;
- `PrincipalService`, `AppState` e `WorkflowManager` enquanto forem necessários ao motor legado;
- matrizes atuais de professores, turmas, disciplinas e horários;
- grade já produzida pelo processamento atual como solução inicial;
- tela atual de visualização da grade;
- infraestrutura já criada de cenários, versões, eventos, validação e comparação;
- primeiro modelo CP-SAT já existente no `api-cenarios`.

As matrizes serão usadas nas fronteiras de compatibilidade. Dentro do novo motor, elas serão convertidas para o modelo canônico com IDs e regras explícitas. Uma matriz informa onde a aula está, mas não informa sozinha por que um professor está bloqueado, se a regra é obrigatória ou se pode ser negociada.

### 4.2 Snapshot completo enviado ao novo motor

O fechamento receberá um único snapshot contendo:

- matrizes atuais da grade;
- IDs de professor, turma, disciplina, escola e período;
- carga contratada e já distribuída;
- bloqueios obrigatórios;
- preferências negociáveis;
- origem de cada restrição;
- posições que não podem mudar;
- professores autorizados pelo gestor a participar da mudança;
- concessões temporárias já aprovadas;
- solução-base usada para calcular o impacto das mudanças.

## 4.3 Analisador de viabilidade e liberações necessárias

Antes de sugerir mudanças, o sistema executará o fechamento estrito com todas as regras ativas.

Quando o fechamento estrito for inviável, o sistema não removerá restrições aleatoriamente. Ele tratará cada preferência negociável como uma restrição condicional e procurará conjuntos mínimos de concessões que tornem possível distribuir 100% das aulas.

**Criar no `api-cenarios`:**

| Classe/interface | Responsabilidade |
|---|---|
| `IAnalisadorViabilidadeService` | Contrato da análise de viabilidade |
| `AnalisadorViabilidadeService` | Coordenar teste estrito e busca de concessões |
| `OtimizadorConcessoesService` | Encontrar conjuntos mínimos e distintos de liberações |
| `RestricaoNegociavel` | Representar uma preferência que pode ser autorizada |
| `SugestaoConcessao` | Informar professor, dia/slot, regra, motivo e impacto |
| `CaminhoFechamento` | Representar uma das três possibilidades completas |
| `RelatorioInviabilidade` | Explicar o gargalo quando nem as concessões permitidas fecham a grade |

**Criar endpoint leve:**

```text
POST /api/simulacao/analisar-viabilidade
```

O retorno apresentará até três caminhos. Exemplo conceitual:

```text
Caminho 1 — menor número de concessões
- liberar professor ID 12 na terça, aula 3
- liberar professor ID 31 na quinta, aula 1
- resultado: 100% fechado; 2 concessões; 4 movimentos

Caminho 2 — menor alteração da grade atual
- liberar professor ID 8 na quarta, aulas 2 e 3
- resultado: 100% fechado; 1 professor afetado; 6 movimentos

Caminho 3 — melhor equilíbrio pedagógico
- liberar professores IDs 12, 22 e 31 em um slot cada
- resultado: 100% fechado; menos janelas; 9 movimentos
```

Cada caminho deverá informar:

- se realmente fecha 100%;
- professores e IDs envolvidos;
- restrições exatas que precisam ser liberadas;
- motivo pelo qual cada liberação é necessária;
- quantidade de aulas e posições alteradas;
- professores e turmas afetados;
- avisos pedagógicos;
- pontuação e diferença para a grade-base.

O diretor poderá aceitar um caminho, autorizar somente parte das concessões ou rejeitar todos. Até a aceitação, nenhuma preferência original será alterada no cadastro. As liberações existirão somente dentro da simulação em memória. Quando uma alternativa for adotada, as concessões serão persistidas na versão escolhida, preservando autoria, justificativa e vigência.

Se nenhum dos três caminhos autorizados fechar 100%, o sistema deverá declarar a inviabilidade e mostrar o gargalo restante. Ele nunca deverá apresentar como fechado um cenário com aulas pendentes.

## 5. Diagnóstico do fonte atual e decisão de preservação

O código existe e alguns arquivos são grandes. A decisão do projeto é não dividi-los agora, pois já existe comportamento funcionando e uma refatoração ampla aumentaria o risco:

| Arquivo | Linhas aproximadas | Ação necessária |
|---|---:|---|
| Frontend `Controllers/CenarioController.cs` | 902 | Preservar; novas ações entram por controllers novos |
| Frontend `wwwroot/js/cenarios.js` | 1.231 | Preservar; novas telas usam arquivos JavaScript novos |
| Frontend `_PainelCenarios.cshtml` | 711 | Preservar; novos recursos usam partials adicionais |
| Frontend `ICenarioApiService.cs` | 539 | Preservar; novos clientes usam interfaces novas |
| Motor `SolverCpSatService.cs` | 802 | Reaproveitar por adapter/fachada; não reescrever agora |
| Motor `ValidadorGradeService.cs` | 466 | Reaproveitar como validador existente |
| Motor `CenarioService.cs` | 358 | Reaproveitar para cenários e versões existentes |
| Motor `SimulacaoController.cs` | 330 | Preservar endpoints existentes; novos fluxos usam controller novo |

Esses arquivos serão cobertos por testes de caracterização e não serão ampliados com as novas capacidades. O novo fechamento será construído ao lado deles, usando composição, interfaces, adapters e endpoints novos. Alterações internas somente ocorrerão se um teste demonstrar um erro ou se não existir outro ponto seguro de integração.

## 6. Plano de implementação

### Bloco 0 — Proteger o estado atual

**Objetivo:** impedir perda do protótipo antes da refatoração.

- versionar mudanças pendentes da API principal e do frontend;
- transformar `C:\projkinix\api-cenarios` em repositório controlado ou incorporá-lo formalmente à solução definida pelo gestor;
- garantir que `bin/`, `obj/`, senhas e `flyway.conf` não sejam commitados;
- registrar os três hashes de commit neste documento;
- guardar um snapshot de teste já existente como fixture sem credenciais ou dados pessoais desnecessários.

**Critério de saída:** todo fonte recuperável pelo Git e os três projetos compilando.

**Evidências de conclusão local em 01/10/2026:**

- API principal: commits `38fbc9c` e `1e20931` enviados para `origin/codex/novo-fechamento` — documentação, migrações Flyway V1-V5 e atualização do roteiro;
- frontend MVC: commit `959c33e` enviado para `origin/codex/novo-fechamento` — integração da tela e do cliente de cenários;
- motor de cenários: commit `6357fc1` — fonte do motor e testes iniciais;
- recompilação completa concluída com zero erros nos três projetos; o novo motor ficou com zero avisos, enquanto a API principal apresentou 290 avisos legados e o frontend 452 avisos legados;
- `appsettings.Development.json`, `flyway.conf`, `bin/` e `obj/` não foram incluídos nos novos commits;
- nenhuma credencial foi adicionada pelas alterações desta entrega;
- pendência operacional: criar/cadastrar o remoto do repositório `api-cenarios` e enviá-lo à nuvem; o endereço esperado foi verificado e ainda não existe no GitHub.

**Risco identificado:** a API principal e o frontend já possuíam credenciais antigas versionadas antes desta entrega. A remoção do código atual e a rotação dessas credenciais serão tratadas em uma tarefa própria, usando configuração segura, sem interromper a conexão utilizada pelo desenvolvimento.

### Bloco 1 — Testes de caracterização

**Criar:**

- projeto `C:\projkinix\api-cenarios.Tests`;
- `ValidadorGradeServiceTests`;
- `SolverCpSatServiceTests`;
- `ComparadorVersoesServiceTests`;
- `CenarioServiceTests`;
- `CenariosApiIntegrationTests`;
- fixtures pequenas: grade viável, inviável, com bloqueio e com ajuste localizado.

**Cobrir obrigatoriamente:**

- conflito de professor;
- carga contratada versus distribuída;
- bloqueio obrigatório;
- preferência negociável;
- sai-e-volta;
- preservação de posições travadas;
- resultado `OPTIMAL`, `FEASIBLE`, `INFEASIBLE` e `UNKNOWN`;
- salvar, fechar processo, reabrir e restaurar.

**Critério de saída:** comportamento atual protegido antes da extensão por adapters e fachadas.

**Conclusão em 01/10/2026:** o projeto `tests/ApiCenarios.Tests` possui 22 testes aprovados. Estão cobertos grade viável e inviável, bloqueios, carga, conflito simultâneo, sai-e-volta, estados `OPTIMAL`, `FEASIBLE`, `INFEASIBLE` e `UNKNOWN`, geração de três alternativas, concessão autorizada, preferência negociável, posição travada, ajuste localizado, comparação, persistência, reabertura, histórico e fluxo HTTP integrado. O Bloco 1 está concluído.

### Bloco 2 — Encapsulamento seguro sem reescrever o motor

**Objetivo:** reaproveitar as classes existentes sem colocar novas responsabilidades dentro delas.

**Criar no `api-cenarios`:**

| Classe/interface | Responsabilidade |
|---|---|
| `IProcessamentoLegadoAdapter` | Contrato para obter a grade produzida hoje |
| `ProcessamentoLegadoAdapter` | Converter as matrizes existentes em snapshot, sem alterar o legado |
| `ISolverExistenteAdapter` | Contrato estável para chamar o CP-SAT já implementado |
| `SolverExistenteAdapter` | Chamar `SolverCpSatService` e traduzir entrada/saída |
| `IValidadorExistenteAdapter` | Contrato estável para o validador atual |
| `ValidadorExistenteAdapter` | Chamar `ValidadorGradeService` sem mudar suas regras |
| `GradeCanonicalMapper` | Converter matrizes e metadados para o modelo do novo processo |
| `FechamentoInteligenteFacade` | Coordenar os novos casos de uso usando os adapters |

**Preservar:**

- `Services/SolverCpSatService.cs` continua como motor existente;
- `Services/ValidadorGradeService.cs` continua como validador existente;
- `Controllers/SimulacaoController.cs` e seus endpoints continuam funcionando;
- matrizes e sessão do frontend continuam compatíveis;
- `Program.cs` receberá somente os novos registros de injeção de dependência.

**Critério de saída:** o novo processo consegue chamar grade, validador e solver existentes por contratos estáveis, sem mudança de comportamento no fluxo atual.

**Progresso em 01/10/2026:** criados `GradeSnapshotV1`, `IProcessamentoLegadoAdapter` e `ProcessamentoLegadoAdapter`. O adapter converte matrizes legadas em `SolverInput`, calcula a carga já distribuída, copia bloqueios, protege posições existentes e não modifica os objetos de entrada. Commit local `e061f32`; suíte completa com 25/25 testes aprovados.

Também foram criados `ISolverExistenteAdapter`, `SolverExistenteAdapter`, `IValidadorExistenteAdapter` e `ValidadorExistenteAdapter` no microsserviço. Eles delegam para o solver e o validador atuais sem duplicar regras. Commit local `4a1c829`; 29/29 testes aprovados.

**Decisão arquitetural aplicada:** a persistência do `api-cenarios` usa exclusivamente Dapper + `MySqlConnector`. Entity Framework, Pomelo e `DbContext` foram removidos do microsserviço. Flyway continua sendo o único mecanismo de evolução do schema. O acesso usa `IDbConnectionFactory`, `ICenarioRepository` e SQL explícito parametrizado. Commit local `ac8f41e`; 31/31 testes aprovados; recompilação completa com zero erros e zero avisos.

**Conclusão em 01/10/2026:** criados `GradeCanonicalMapper`, `FechamentoInteligenteFacade` e `FechamentoInteligenteController`. A fachada normaliza o snapshot, valida pelas regras existentes e chama o solver por interfaces. Pendências `R4_PENDENCIA` são solucionáveis; conflitos obrigatórios impedem o solver. O endpoint `POST /api/fechamento-inteligente/analisar-e-fechar` foi comprovado por teste HTTP. Commits locais `9171de9` e `83b7d2b`; 36/36 testes aprovados; build com zero erros e zero avisos. O Bloco 2 está concluído.

### Bloco 3 — Persistência completa das alternativas e publicação

As tabelas V1 a V5 existem. O fonte mostrou uma lacuna: a publicação atual altera o status e registra evento, mas não mantém uma entidade própria apontando claramente qual versão está vigente. As três alternativas também precisam existir no banco antes da escolha.

**Criar por Flyway, sem alterar V1 a V5:**

- `V6__criar_tabela_publicacao.sql` -> `hprof_publicacao`;
- `V7__criar_tabela_simulacaoalternativa.sql` -> `hprof_simulacaoalternativa`.

**Criar no `api-cenarios`:**

- `Publicacao` e `SimulacaoAlternativa`;
- `IPublicacaoRepository` e `ISimulacaoRepository`;
- `PublicacaoService`;
- `SimulacaoRepository`;
- DTOs separados em `Dto/Publicacao` e `Dto/Simulacao`.

**Regras:**

- as três alternativas devem ser imutáveis;
- a escolha cria uma nova versão filha, sem sobrescrever a anterior;
- publicar encerra a vigência anterior e abre uma nova vigência;
- desfazer publicação cria novo evento e restaura uma referência anterior, sem apagar histórico.

**Progresso em 01/10/2026:** V6 e V7 foram executadas pelo gestor no banco de desenvolvimento; Flyway confirmou o schema `droplexdb` na versão 7. No `api-cenarios`, foram implementados repositórios Dapper parametrizados para alternativas e publicações, sem Entity Framework. As três alternativas são gravadas na mesma transação. A adoção bloqueia a candidata, cria uma nova `hprof_cenarioversao` imutável, marca a escolhida como `ADOTADA` e as demais como `DESCARTADA`, tudo atomicamente e sem exclusão. A publicação encerra a vigente como `SUBSTITUIDA`; revogação e restauração preservam o histórico, e restaurar cria uma nova publicação apontando para a versão histórica. Foram adicionados controllers leves para listar/adotar alternativas e publicar/restaurar/revogar. Suíte completa: 41/41 testes aprovados, build sem erros e sem avisos.

**Conclusão formal em 01/10/2026:** o endpoint de geração foi ligado a `hprof_simulacao` e `hprof_simulacaoalternativa` por `SimulacaoPersistenciaService` e repositórios Dapper. No cenário de desenvolvimento 3, versão-base 3, foi criada a simulação 1 com três alternativas e 280/280 aulas. A alternativa 1 foi adotada e criou a versão imutável 4; ALT_A ficou `ADOTADA`, ALT_B e ALT_C ficaram `DESCARTADA`. Foram criadas três publicações auditáveis: publicação inicial, substituição e restauração. Após reiniciar o `api-cenarios`, a publicação 3 permaneceu `VIGENTE`, apontando para a versão 4, e as três publicações continuaram no histórico. O teste real também identificou e corrigiu a conversão `DateOnly` do Dapper/MySQL. Suíte final: 42/42 testes aprovados, zero erros de compilação. O Bloco 3 está concluído.

### Bloco 4 — Carregamento completo das regras reais

**Criar:**

- `IGradeOrigemClient` no motor de cenários;
- `GradeOrigemClient` para conversar com a API principal;
- `GradeSnapshotFactory` na API principal;
- `RegraSnapshotFactory`;
- contratos versionados `GradeSnapshotV1` e `RegraSnapshotV1`.

**Modificar na API principal:**

- criar controller leve para fornecer o snapshot completo ao motor;
- manter `PrincipalExecucaoController`, `PrincipalService`, `AppState` e `WorkflowManager` intactos até os testes demonstrarem substituição segura;
- nunca fazer o frontend consultar banco diretamente para o fluxo novo.

**Conclusão em 02/10/2026:** criado `RegraSnapshotV1` e ampliado `GradeSnapshotV1` com contexto, IDs, professores, cargas, atribuições, disciplinas, turmas detalhadas e regras com campos normalizados e valores originais. A API principal ganhou o endpoint leve e somente leitura `GET /api/grade-origem/snapshot`, implementado com Dapper e sem instanciar `AppState` ou disparar o processamento legado. O `api-cenarios` ganhou `IGradeOrigemClient`, `GradeOrigemClient`, `PreparacaoSolverFactory` e `GET /api/grade-origem/carregar`.

Teste integrado real com escola 103, ano 2025 e período MANHA: 12 turmas, 19 professores, carga total 360, 46 disciplinas e 86 regras preservadas. Os códigos reais `SEG1`, `TER2`, `QUA1`, `QUI1` e `SEX1` foram normalizados; 86/86 bloqueios chegaram ao solver. O fechamento executou em memória com status `FEASIBLE`, 3.266 variáveis, 321 aulas distribuídas e 39 pendentes. Suíte do microsserviço: 44/44 testes aprovados. Commits locais: API principal `8a221eb`; microsserviço `b959159` e `568c8ea`. O Bloco 4 está concluído.

**Dados obrigatórios no snapshot:**

- IDs e cargas dos professores;
- turmas, disciplinas e quantidade de aulas;
- bloqueios obrigatórios;
- preferências negociáveis;
- posição atual da grade;
- posições travadas pelo gestor;
- exceções já aprovadas;
- escola, ano, período e versão da estrutura.

### Bloco 5 — Fechamento em três estratégias

Cada execução deverá produzir até três resultados distintos:

1. **Ajuste mínimo:** menor número de movimentos em relação à grade-base.
2. **Qualidade pedagógica:** prioriza dobradinhas e reduz sai-e-volta.
3. **Equilíbrio docente:** reduz janelas e distribui melhor a semana.

Não serão aceitas três cópias praticamente iguais. `GeradorAlternativasService` aplicará objetivos, pesos ou sementes diferentes e o comparador medirá diversidade mínima.

Cada alternativa deverá apresentar:

- status do solver;
- aulas distribuídas e pendentes;
- violações e avisos;
- professores afetados;
- quantidade de movimentos;
- concessões utilizadas;
- pontuação por critério;
- diferença visual para a grade-base.

### Bloco 6 — Concessões e casos inviáveis

Fluxo obrigatório:

1. tentar modo estrito;
2. se inviável, explicar os bloqueios envolvidos;
3. permitir ao gestor autorizar professores/dias específicos;
4. executar novamente apenas com essas concessões;
5. gravar autor, justificativa e vigência;
6. nunca alterar a preferência original do cadastro para simular uma concessão;
7. ao adotar a alternativa, persistir a concessão na versão escolhida.

### Bloco 7 — Ajuste cirúrgico de março/abril

**Criar:**

- `AjusteCirurgicoRequest`;
- `EscopoAjuste`;
- `TravaAlocacao`;
- endpoint `POST /api/simulacao/ajuste-cirurgico`;
- serviço frontend específico para selecionar professor, turmas e limite de movimentos.

O usuário escolherá quem pode mudar. Todo professor, turma e slot fora do escopo será travado. O motor retornará três opções localizadas e informará quando o pedido for matematicamente inviável.

### Bloco 8 — Simulações longas e execução em segundo plano

Para dezenas de tentativas, a chamada HTTP não deve ficar aberta.

**Criar:**

- `SimulacaoJobService`;
- `ISimulacaoQueue`;
- `SimulacaoWorker : BackgroundService`;
- endpoints para solicitar, acompanhar e cancelar a simulação;
- persistência de estado em `hprof_simulacao`.

Primeira implementação: fila em memória com estado persistido no MySQL, suficiente para uma instância. Redis somente será introduzido se houver múltiplas instâncias, necessidade de fila distribuída ou medição que justifique. Redis não será o banco oficial dos cenários.

### Bloco 9 — Extensão segura do frontend

**Preservar:**

- `CenarioController.cs`, `ICenarioApiService.cs`, `_PainelCenarios.cshtml` e `cenarios.js` continuam como estão;
- correções nesses arquivos serão pontuais e cobertas por teste;
- o fluxo atual permanecerá disponível como retorno seguro.

**Criar para as funcionalidades novas:**

- `FechamentoInteligenteController.cs`;
- `IFechamentoInteligenteApiClient.cs` e `FechamentoInteligenteApiClient.cs`;
- `_PainelFechamentoInteligente.cshtml`;
- `fechamento-inteligente.js`;
- contratos específicos para viabilidade, três caminhos e ajuste cirúrgico.

O frontend exibirá os dados, coletará decisões do gestor e chamará APIs. Ele não implementará as regras do CP-SAT.

### Bloco 10 — Piloto e aceite

Executar com uma escola/ano/período escolhidos pelo gestor:

1. gerar a grade pelo fluxo atual;
2. salvar a versão-base;
3. validar;
4. executar modo estrito;
5. autorizar concessões quando necessário;
6. gerar três alternativas;
7. comparar e adotar uma;
8. publicar com vigência;
9. reiniciar todos os serviços;
10. restaurar a versão oficial;
11. solicitar um ajuste cirúrgico;
12. desfazer e confirmar retorno ao estado anterior.

## 7. Ordem imediata de trabalho

Não iniciaremos novas regras diretamente nos arquivos grandes. A ordem é:

1. **Bloco 0:** proteger todo o código atual no Git;
2. **Bloco 1:** criar testes de caracterização;
3. **Bloco 2:** criar adapters ao redor do motor, validador e processamento existentes;
4. **Bloco 3:** criar V6/V7 e persistir publicação/alternativas;
5. **Bloco 4:** garantir snapshot completo das regras reais;
6. **Blocos 5 a 7:** fechar, conceder exceções e ajustar cirurgicamente;
7. **Blocos 8 a 10:** execução longa, frontend e piloto.

## 8. Próximo passo exato

**Próxima tarefa:** iniciar o Bloco 7 sobre uma das alternativas completas da simulação 5. Implementar ajuste cirúrgico com escopo explícito de professores, turmas e posições que podem mudar; travar todo o restante; devolver até três soluções localizadas; comprovar por teste que nenhuma célula fora do escopo foi alterada; persistir as propostas como novas alternativas sem substituir a versão oficial.

## 9. Diário resumido

| Data | Trabalho | Resultado | Próximo passo |
|---|---|---|---|
| 31/08/2026 | Planejamento, Flyway V1-V5 e protótipo dos blocos 1-9 | Código inicial criado | Consolidar e provar integração |
| 01/10/2026 | Flyway, build e verificação das três aplicações | V1-V5 `Success`; 3 builds sem erros; APIs e frontend ativos | Proteger fonte e criar testes |
| 01/10/2026 | Consulta de cenário e validação de versão existente | Banco acessível; 280/280 aulas; 0 violações; 18 avisos | Testar três alternativas em caso real |
| 01/10/2026 | Proteção dos três projetos e testes iniciais de caracterização | API e frontend enviados à nuvem; motor protegido em commit local `6357fc1`, aguardando criação do remoto; 7/7 testes aprovados; três builds sem erros; 290/452 avisos legados registrados; motor sem avisos | Completar testes de comparação, persistência, restauração e API integrada |
| 01/10/2026 | Comparação, persistência e restauração do motor de cenários | Commits locais `3a3f07a` e `6a45a38`; 13/13 testes aprovados; cenário e versões restaurados após reabertura | Testar os endpoints integrados e regras de preservação localizada |
| 01/10/2026 | Fluxo HTTP integrado do motor de cenários | Commit local `3d7d911`; 14/14 testes aprovados; criação, versões, restauração, validação e comparação comprovadas por API; zero avisos no novo motor | Cobrir preferências negociáveis, posições travadas e ajuste localizado |
| 01/10/2026 | Conclusão dos testes de caracterização | Commit local `3d2b706`; 22/22 testes aprovados; preferências flexíveis, concessões, travas, ajuste localizado, sai-e-volta e estados do solver cobertos | Iniciar adapter de entrada legado → `SolverInput` no Bloco 2 |
| 01/10/2026 | Primeiro adapter do Bloco 2 | Commit local `e061f32`; snapshot legado convertido para contrato `SolverInput`; 25/25 testes aprovados | Criar adapters estáveis do solver e do validador existentes |
| 01/10/2026 | Adapters do solver/validador e decisão de persistência leve | Commit local `4a1c829`; 29/29 testes aprovados; definido Dapper + MySqlConnector e Flyway, sem novos DbContexts | Migrar o repositório de cenários/versões para Dapper |
| 01/10/2026 | Migração completa do microsserviço para Dapper | Commit local `ac8f41e`; EF, Pomelo e DbContext removidos; Dapper + MySqlConnector; 31/31 testes; build sem avisos | Criar mapper canônico e fachada do fechamento inteligente |
| 01/10/2026 | Conclusão do encapsulamento seguro | Commits locais `9171de9` e `83b7d2b`; mapper, fachada e endpoint leve concluídos; 36/36 testes; build sem avisos | Iniciar Flyway V6/V7 e persistência Dapper do Bloco 3 |
| 01/10/2026 | Persistência Dapper do Bloco 3 | Flyway V6/V7 aplicado pelo gestor; alternativas, adoção transacional, publicação, revogação e restauração implementadas; 41/41 testes; sem EF e sem exclusão de histórico | Executar ciclo integrado no MySQL e comprovar restauração após reinício |
| 01/10/2026 | Conclusão integrada do Bloco 3 | Simulação 1, cenário 3: três alternativas persistidas, versão 4 adotada, três publicações históricas e restauração confirmada após reinício; correção `DateOnly`; 42/42 testes | Iniciar Bloco 4 pelo inventário completo das regras e dados de origem |
| 02/10/2026 | Conclusão do Bloco 4 | Snapshot completo via Dapper; integração API principal → motor; 12 turmas, 19 professores, carga 360, 46 disciplinas e 86/86 bloqueios; solver FEASIBLE com 321 aulas e 39 pendentes; 44/44 testes | Iniciar Bloco 5 e gerar três soluções distintas para o caso real |
| 02/10/2026 | Conclusão do Bloco 5 | Modelo com semente, penalidade real de janelas, diversidade e proibição de repetição; 45/45 testes; cenário 4, versão 5 e simulação 2 persistidos; ALT_A/ALT_B/ALT_C com hashes distintos e diferenças de 393/402 células | Iniciar Bloco 6 e descobrir as concessões mínimas para as 39 aulas pendentes |
| 02/10/2026 | Conclusão do Bloco 6 | Corrigido limite diário dos regentes (causa das 39 pendências); concessões automáticas penalizadas e explicadas por professor/dia/aula/turma; fechamento exato obrigatório; 48/48 testes; simulação 5 com três grades distintas de 360/360 e zero concessões no caso real | Iniciar Bloco 7 com ajuste cirúrgico sobre alternativa completa |

## 10. Como atualizar este documento

Ao final de cada sessão:

1. atualizar a seção “Onde estamos”;
2. marcar o bloco executado e registrar evidências;
3. listar arquivos criados e modificados;
4. registrar testes executados e resultados;
5. escrever um único “Próximo passo exato”;
6. adicionar uma linha ao diário;
7. nunca marcar como concluído algo apenas criado, mas não testado.
