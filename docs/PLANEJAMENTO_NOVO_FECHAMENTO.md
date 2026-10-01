# Horário Professor — planejamento do novo fechamento

> **Regra obrigatória:** toda implementação e manutenção deste planejamento deve obedecer ao [Manifesto de Engenharia](MANIFESTO_DO_PROJETO.md).

Plano técnico e funcional para simulações, redistribuição e gestão de versões

Versão 1.0 | 31 de agosto de 2026 | Situação: proposta para implementação por etapas

Documento de referência para o gestor e para o desenvolvimento. Consolida os requisitos discutidos, pontos observados no código e a ordem recomendada de execução. Não representa funcionalidades já entregues nem garante fechamento de qualquer conjunto de restrições.

## 1. Decisão central e resultado esperado

Evoluir o sistema existente por uma abordagem híbrida: preservar os cadastros, as telas, as matrizes, os relatórios e as regras úteis; criar um novo núcleo de simulação e fechamento, isolado das rotinas antigas. A intenção não é continuar acrescentando indefinidamente trocas específicas nem reescrever toda a aplicação de uma vez.

A tecnologia proposta para a redistribuição é **Google OR-Tools, com o solucionador CP-SAT, integrado em C#**. O novo núcleo representará o problema por variáveis e restrições, procurando combinações envolvendo vários professores, turmas e horários. Essa proposta ainda precisa de protótipo e medição nos dados reais; não está instalada ou integrada apenas por constar neste documento. [R1, R2]

Para alterações pontuais, a técnica proposta é a **reotimização parcial por vizinhança restrita**, inspirada em Large Neighborhood Search (LNS): congelar as posições protegidas e recalcular somente o conjunto autorizado. Isso permitirá investigar cadeias de trocas que ultrapassam as atuais trocas de uma ou duas posições, sem liberar a escola inteira para mudanças.

O resultado desejado é permitir ao diretor comparar até três grades distintas e válidas, entender as concessões de cada uma, salvar as alternativas, escolher a oficial e voltar meses depois com o contexto completo. As preferências originais não serão sobrescritas silenciosamente.

### 1.1 Decisões consolidadas

- Manter o processamento pesado em memória, com estado separado por simulação.
- Continuar usando as matrizes na integração atual, apoiadas por um modelo estruturado com identificadores.
- Usar MySQL para persistência durável de cenários, versões, regras, resultados e auditoria.
- Não exigir Redis na primeira entrega. Ele não é o motor de distribuição nem substitui o histórico no banco.
- Usar Flyway para evolução controlada das novas tabelas, todas com prefixo `hprof_`.
- Oferecer administração protegida de migrações pela API/Swagger, além da implantação controlada.
- Salvar grade, dados de origem, regras efetivas e exceções aceitas em conjunto.
- Separar simular, salvar, aprovar exceções e publicar como grade oficial.
- Não prometer três grades fechadas quando os dados forem inviáveis ou o limite de busca se esgotar.

### 1.2 Limites deste trabalho

Este documento não executa migrações, não altera cadastros, não processa grades, não publica resultados e não modifica os motores existentes. A leitura direcionada do código identifica pontos de integração e riscos, mas não equivale à auditoria exaustiva de cada rotina nem a um benchmark do novo algoritmo.

O atendimento de aproximadamente 90% dos casos foi relatado pelo usuário. Esse número será tratado como percepção operacional até haver testes reproduzíveis. Não há evidência suficiente para afirmar que os casos restantes são todos resolvíveis sem concessões.

## 2. Base atual e caminho da execução

Há dois projetos: frontend ASP.NET MVC em `C:\projkinix\HorarioProfessor2025_v2` e API ASP.NET em `C:\projkinix\api-horarioprofessor`. Ambos usam .NET 8. No ambiente local discutido, o frontend atende na porta 5051 e a API na porta 5000. Swagger é a interface de documentação e teste da API, não o local onde o algoritmo executa.

No fluxo observado, a tela Processar Horário envia ano e período ao controller MVC; o MVC obtém também o contexto da escola e chama a API. A API aciona a rotina principal, que organiza o carregamento e as rotinas de distribuição e realocação.

### 2.1 Sequência observada no código

1. A tela chama `atualizarDistribuicaoGradeHoraria` e solicita `ObterHorarios` ao MVC.
2. O controller MVC chama `GET /api/PrincipalExecucao/horarios` com os filtros de processamento.
3. A API prepara opções de processamento, inclusive apagando e copiando registros auxiliares, antes de chamar o serviço principal.
4. `PrincipalService.Executar` aciona `AppState.Main`; o fluxo carrega dados e trabalha com matrizes e estruturas auxiliares.
5. O encadeamento tenta distribuição inicial, reagendamento e realocações, incluindo combinações verticais e horizontais.
6. Existem caminhos de salvamento automático e persistência de matrizes em JSON; o frontend também mantém dados em sessão.

Consequência importante: hoje essa rota GET não deve ser tratada como simples consulta de diagnóstico. Ela pode alterar estado no banco. O novo desenho terá GET apenas para leitura e POST para iniciar processamento ou efetivar mudanças.

### 2.2 Preservação e revisão

As rotinas comentadas são histórico de experimentação e devem ser identificadas como tal. As regras de negócio serão extraídas do caminho realmente executado e confirmadas com o gestor, sem transformar todo comentário em requisito obrigatório.

O projeto contém lógica útil de montagem, leitura e apresentação das matrizes. O mecanismo de busca por trocas específicas será encapsulado como motor legado e confrontado com o novo motor. O validador de regras deve ser independente de ambos.

Foram observados riscos: inicialização assíncrona no construtor de `AppState`, valores padrão de escola/ano, trabalho com sessão e preparação de tabelas auxiliares compartilhadas por filtros. `AppState` está registrado como serviço scoped, não como singleton global. Mesmo assim, tarefas não aguardadas e operações no mesmo conjunto de registros exigem revisão de concorrência.

O frontend também possui acessos diretos ao banco em partes do legado. Isso não será removido indiscriminadamente. Contudo, os novos cenários, versões, aprovações e publicações terão sua lógica centralizada na API, evitando duas implementações concorrentes das regras de fechamento.

## 3. Situações que a solução precisa atender

### 3.1 Fechamento inicial

O gestor carrega escola, ano e período, escolhe uma grade inicial ou inicia a distribuição, e solicita tentativas de fechamento. A primeira busca respeita as restrições cadastradas e a política selecionada. Caso não feche, o sistema apresenta pendências e investiga alternativas dentro das autorizações fornecidas.

Quando uma folga inviabiliza o fechamento, a tela precisa distinguir preferência negociável de indisponibilidade real. O diretor poderá autorizar mudanças específicas para determinados professores, dias e horários. As simulações usarão essas autorizações sem alterar automaticamente o cadastro original.

### 3.2 Sugestões sem modificar os cadastros

O gestor poderá autorizar uma análise hipotética de preferências negociáveis e receber até três opções. Cada opção mostrará exatamente quais pedidos não foram atendidos. Autorizar a análise não significa aprovar as concessões nem publicar a grade.

Uma grade tecnicamente fechada, mas dependente de exceções ainda não aceitas, será candidata pendente de aprovação. Não poderá ser apresentada como oficial. Bloqueios obrigatórios continuarão obrigatórios, mesmo nesse modo.

### 3.3 Ajuste cirúrgico em março ou abril

O diretor abre uma versão aprovada, informa a chegada ou mudança de um professor e seleciona quem pode participar do ajuste. A grade é clonada para um novo cenário. O restante fica congelado, e o motor procura uma solução com o menor impacto permitido.

Se não houver solução no escopo informado, o sistema explica a limitação e pode sugerir sua ampliação. A ampliação depende de autorização; não ocorre silenciosamente. A versão anterior permanece disponível, inclusive depois de uma nova publicação.

### 3.4 Fechar o sistema e continuar depois

Ao reabrir uma versão salva, o sistema deverá remontar as mesmas matrizes, os identificadores, as regras efetivas, as concessões aceitas, as travas e o vínculo com os dados de origem. Não basta carregar as células preenchidas e voltar às preferências atuais do cadastro como se nada tivesse sido negociado.

## 4. Regras obrigatórias, preferências e exceções

Cada regra precisa de identificação, origem, escopo, severidade, vigência e responsável pela eventual aprovação. A classificação atual deverá ser levantada e validada antes de alimentar o solucionador.

| Categoria | Tratamento proposto | Exemplo |
| --- | --- | --- |
| Obrigatória | Nunca violar em uma grade válida | Professor não estar em duas turmas simultâneas |
| Preferência | Otimizar ou flexibilizar conforme autorização | Pedido de folga negociável |
| Trava do cenário | Manter até liberação explícita | Aula já combinada com outro professor |
| Exceção aprovada | Aplicar somente ao escopo registrado | Trabalhar na sexta neste fechamento |
| Hipótese | Sugerir sem efetivar | Testar a liberação de uma preferência |

Não converter todos os bloqueios da planilha em preferências. Disponibilidade em outra escola, impedimento real de trabalho, habilitação para a disciplina e limitações contratuais informadas precisam de tratamento próprio. A classificação não será deduzida somente de cor ou texto livre na matriz.

### 4.1 Regras mínimas a catalogar

- Identificação de professor, turma, disciplina, escola, ano, período e calendário.
- Quantidade de aulas obrigatórias e compatibilidade professor–disciplina–turma.
- Ocupação simultânea, capacidade da turma e existência do horário no período.
- Disponibilidades reais e conflitos entre períodos ou escolas, quando conhecidos.
- Preferências de dias, horários, folgas e distribuição ao longo da semana.
- Limites de aulas consecutivas, agrupamentos e janelas, quando confirmados.
- Travas manuais, posições que podem mudar e limite de alterações autorizadas.
- Prioridades e critérios de desempate aceitos pelo gestor.

Regra ambígua deve aparecer como pendência de especificação. Não inventar uma restrição e tratá-la como decisão do usuário. Se dados de outras escolas não estiverem disponíveis, informar essa limitação da validação.

### 4.2 Exemplo de autorização persistida

Um cenário mantém o pedido original de folga do professor P na sexta-feira. O diretor autoriza, com justificativa, no máximo duas aulas na sexta pela manhã, para uma versão e vigência definidas. O motor poderá usar somente essa abertura; não ganha autorização para qualquer dia nem para alterar o cadastro de P.

Depois da escolha, a versão registra o pedido original, a exceção aceita e a regra efetiva resultante. Transformar a exceção em mudança permanente de cadastro será outra operação, com confirmação e auditoria próprias.

Aprovar uma concessão de uma candidata já salva deverá produzir uma nova versão derivada, ou um evento de aprovação imutável explicitamente referenciado pela versão de publicação. Não modificar retroativamente o snapshot usado para gerar as outras alternativas. O contrato da Etapa 1 escolherá e testará uma dessas formas.

## 5. Memória, matrizes e modelo do motor

Sim, as matrizes continuarão existindo. São adequadas para exibir a grade, integrar o legado e comparar posições. O novo motor não deverá, porém, depender de nomes de professores e textos concatenados nas células para determinar identidade e regras.

O modelo canônico terá identificadores estáveis e relações explícitas. Cada posição será localizada por contexto, dia e horário; cada alocação apontará para professor, turma, disciplina e demanda de aula. Um adaptador reconstruirá as matrizes usadas hoje, preservando a ordem das linhas e colunas.

### 5.1 Estruturas propostas

- Snapshot de entrada imutável: cadastros necessários, demandas, calendários e regras originais.
- Mapa de índices: correspondência entre IDs e posições nas matrizes e nos vetores.
- Estado de alocação: aulas distribuídas e pendentes, ocupação por professor e turma.
- Máscaras de disponibilidade e travas: posições permitidas, proibidas e congeladas.
- Política do cenário: exceções, orçamento de mudanças, prioridades e vigência.
- Resultado independente: alocações, avaliação, alterações e relatório de validação.

Cada tentativa usará estado próprio, derivado do mesmo snapshot por cópia segura ou estruturas imutáveis. Uma simulação não pode modificar a matriz de outra, a sessão oficial ou os cadastros. Índices e caches só poderão ser reutilizados se forem realmente imutáveis.

Dados derivados poderão ser recalculados ao restaurar uma versão. O snapshot guardará todas as entradas necessárias e a versão dos adaptadores. Serializar uma instância de serviço, com conexões e referências em memória, não é um mecanismo válido de restauração.

### 5.2 Redis: posição no plano

Na primeira implementação, memória do processo para cálculo e MySQL para persistência são suficientes como arquitetura inicial. A necessidade de Redis será avaliada por métricas, não por pressuposto.

Redis poderá futuramente apoiar cache, comunicação entre instâncias ou coordenação de trabalhos. Não resolve o problema combinatório, não elimina a necessidade de isolamento e não será a única cópia dos cenários. Perder cache não poderá apagar uma grade salva ou uma aprovação.

## 6. Novo núcleo de processamento

Componentes propostos, ainda não implementados: carregador de snapshot, compilador de regras, adaptador de matrizes, validador, motor legado encapsulado, motor CP-SAT, coordenador de reparo, comparador de cenários, repositório de versões e trabalhador de simulações.

O carregador fará consultas de leitura delimitadas pelo contexto autorizado. Não deverá instanciar o fluxo legado com efeitos colaterais apenas para obter dados. O compilador transformará regras confirmadas em restrições. O validador verificará resultados por código independente das decisões internas do solucionador.

### 6.1 Modelo CP-SAT proposto

Representar a escolha de cada aula entre posições elegíveis. Uma formulação possível usa variáveis booleanas de alocação e elimina previamente combinações impossíveis; o desenho exato será escolhido no protótipo conforme o tamanho do problema. Não criar indiscriminadamente todas as combinações de professor, turma, disciplina e horário.

As restrições garantem atendimento da demanda, exclusividade por professor e turma, compatibilidade das aulas, disponibilidade obrigatória e travas. Preferências negociáveis entram como custos ou limites explícitos. Para diagnóstico parcial, poderão existir variáveis de pendência, mas seus resultados nunca serão confundidos com fechamento completo.

A documentação do OR-Tools apresenta exemplos de escalonamento com restrições e preferências. O problema escolar exigirá modelagem, adaptadores e validação próprios; não se trata de simplesmente copiar uma escala de funcionários. [R1]

Especificação inicial para o protótipo: para cada demanda de aula e posição elegível, representar se a alocação ocorre; a soma por demanda deve igualar a carga exigida; por professor e horário, permitir no máximo uma alocação; por turma e horário, permitir no máximo uma aula. Posições proibidas são excluídas das candidatas ou fixadas em zero; travas mantêm o valor da origem. Adaptar a formulação quando uma demanda representar um bloco, evitando contar blocos como aulas individuais.

Antes da busca, verificar cargas maiores que a disponibilidade, turmas sem horários suficientes, atribuições inexistentes e conflitos entre travas. Esses testes antecipam alguns problemas, mas passar neles não prova que a combinação completa seja viável.

### 6.2 Ordem de otimização

Primeiro garantir validade. Depois buscar fechamento completo. Em modo cirúrgico, priorizar preservação da grade e orçamento de alterações. Dentro dessas condições, considerar professores afetados, concessões, janelas e qualidade da distribuição, conforme política aprovada.

A proposta é otimização por prioridades ordenadas, com etapas ou limites explícitos. Evitar pesos arbitrários que permitam melhorar um critério secundário compensando uma quebra importante. A prioridade entre minimizar concessões e minimizar mudanças será escolhida por modo e registrada na execução.

Um ponto de partida: zero violações obrigatórias; zero aulas pendentes; limites de mudanças respeitados; menor impacto; melhor atendimento das preferências. Na busca inicial, o gestor poderá priorizar preferências antes da estabilidade, porque ainda não há grade publicada.

### 6.3 Por que não depender somente das trocas atuais

Uma troca isolada pode parecer impossível porque o destino está ocupado. Uma solução global pode exigir mover B para liberar A, mover C para liberar B e realocar A em outro dia. Rotinas que só aceitam melhora imediata ou exploram uma forma de troca podem não alcançar essa sequência.

O modelo por restrições permite avaliar essas escolhas em conjunto. Isso amplia a busca, mas não elimina o crescimento combinatório nem garante rapidez em todos os casos. O ganho será medido com casos representativos e comparado à distribuição atual.

### 6.4 Outras técnicas discutidas

| Técnica | Papel no planejamento |
| --- | --- |
| CSP, propagação e MRV | Base conceitual para domínios e seleção de variáveis |
| Backtracking manual | Testes pequenos e oráculos; risco de crescimento excessivo |
| Busca local e hill climbing | Refinamento rápido; risco de ficar em ótimo local |
| Tabu e simulated annealing | Experimentos futuros para escapar de soluções locais |
| Algoritmos genéticos | Alternativa posterior; preservar validade exige cuidado |
| CP-SAT | Motor principal proposto de restrições e otimização |
| Reotimização parcial / LNS | Reparo com região liberada e restante congelado |
| Motor legado | Referência, geração inicial e contingência controlada |

Não implementar todas essas técnicas simultaneamente. A primeira hipótese será CP-SAT com solução inicial e reparo restrito. Só acrescentar outra abordagem quando medições demonstrarem necessidade concreta.

## 7. Modos de busca e ajuste cirúrgico

### 7.1 Modo estrito

Respeitar a política cadastrada sem novas concessões. Usar a grade inicial como referência ou dica de busca, nunca como prova de validade. Se houver inconsistências na base, apresentar diagnóstico antes de publicar qualquer resultado.

### 7.2 Modo com concessões autorizadas

Permitir somente flexibilizações descritas no cenário: professores selecionados, dias e horários definidos, limite de concessões e vigência. Distinguir exceções já aprovadas das hipóteses que aguardam decisão.

O sistema poderá informar que uma preferência parece impedir uma alternativa, mas só afirmará impossibilidade com evidência suficiente. Explicações de conflito indicarão se são verificação simples, conjunto comprovadamente conflitante ou hipótese de investigação.

### 7.3 Modo de reparo localizado

1. Partir de versão salva e validada, registrando seu identificador como origem.
2. Marcar professores ou posições solicitados e alterações de regras do cenário.
3. Construir a região autorizada e congelar todas as demais alocações.
4. Resolver o subproblema, permitindo cadeias e ciclos dentro dessa região.
5. Validar a grade inteira, inclusive as fronteiras com a parte congelada.
6. Mostrar diferenças e solicitar autorização antes de ampliar a região.

Congelar significa restrição efetiva no modelo, não apenas penalizar a mudança. Se o usuário permite mexer em A e B, o motor não poderá alterar C por encontrar uma pontuação melhor. Também será possível bloquear células dentro do conjunto autorizado.

O orçamento de mudanças definirá se conta aulas, células alteradas, professores ou uma combinação. Para não haver dupla interpretação, os relatórios mostrarão tanto movimentações de aulas quanto posições cujo conteúdo mudou.

## 8. Três alternativas e diagnóstico

A meta é apresentar até três opções úteis e distintas, não cópias com nomes diferentes. Variar somente a semente não garante diversidade; será necessário comparar alocações, excluir duplicatas e, quando adequado, impor distância mínima entre candidatos.

Perfis sugeridos: A com menor mudança; B com melhor atendimento das preferências; C com outro equilíbrio dentro dos mesmos limites de autorização. Os perfis não autorizam violar travas ou usar professores não liberados.

### 8.1 Indicadores por opção

- Aulas atendidas, aulas pendentes e inconsistências obrigatórias.
- Aulas movimentadas, células alteradas e professores afetados.
- Preferências não atendidas e estado de aprovação de cada ocorrência.
- Janelas e outros critérios confirmados de qualidade.
- Tempo de busca, situação do solucionador e versão de origem.
- Diferenças por professor, turma, dia e horário, com antes e depois.

Uma grade só estará fechada quando todas as demandas obrigatórias estiverem atendidas e as regras obrigatórias passarem na validação. Contar células vazias ou preenchidas isoladamente não prova fechamento: podem existir cargas duplicadas, faltantes ou posições que não deveriam receber aula.

### 8.2 Resultados possíveis

| Situação | Comunicação ao usuário |
| --- | --- |
| Válida e completa | Candidata fechada, sujeita a aprovação e escolha oficial |
| Válida com pendências | Parcial; não publicar como fechamento |
| Inviabilidade comprovada | Não existe solução no modelo e escopo testados |
| Limite sem conclusão | Busca esgotada; não prova impossibilidade |
| Cancelada ou interrompida | Preservar o que foi salvo e informar recuperação |
| Dados ou modelo inválidos | Corrigir entrada ou implementação antes de repetir |

CP-SAT distingue OPTIMAL, FEASIBLE, INFEASIBLE e UNKNOWN, entre outros estados. FEASIBLE não significa ótimo comprovado; UNKNOWN não significa inviável. A interface traduzirá os estados sem perder essa distinção. [R2]

Não prometer um conflito mínimo automaticamente: um conjunto suficiente para explicar inviabilidade pode não ser o menor possível. A precisão das explicações será testada separadamente. [R3]

## 9. Snapshot completo e regras efetivas

O snapshot será a fotografia necessária para reconstruir o cenário sem depender de alterações posteriores nos cadastros. É diferente de copiar o banco inteiro: incluirá somente dados necessários ao contexto, preservando referências e relações.

### 9.1 Conteúdo mínimo da versão

- Identificação: cenário, versão, versão-pai, escola, ano, período, vigência, autor e data.
- Origem: momento da leitura, IDs e impressão digital dos dados de entrada.
- Cadastros: professores, turmas, disciplinas, cargas, calendário, horários e relações.
- Regras originais classificadas, sem perda do pedido original.
- Exceções propostas e aceitas, justificativas, aprovadores, escopos e vigências.
- Regras efetivas, travas e permissões de alteração.
- Grade estruturada, matrizes ou dados para reconstruí-las, índices e pendências.
- Parâmetros: modo, limites, prioridades, sementes e versão do motor/modelo.
- Validação: versão do validador, indicadores, diferenças e aprovações.
- Integridade: versão do formato e hash do conteúdo canônico.

Hash ajuda a detectar mudança ou corrupção, mas não substitui autorização e auditoria. Dados pessoais, inclusive snapshots, exigem controle de acesso e retenção. Não salvar senhas, strings de conexão ou tokens dentro deles.

### 9.2 Uma fonte de verdade por versão

Proposta inicial: snapshot imutável como representação canônica completa. Tabelas auxiliares de regras e indicadores serão projeções consultáveis vinculadas à versão, produzidas na mesma gravação. Não manter duas representações editáveis concorrentes.

Se o desenho físico optar por regras normalizadas como fonte canônica, deverá substituir a decisão anterior expressamente e estabelecer reconstrução determinística. O contrato não pode permitir que o JSON diga uma coisa e as tabelas outra.

## 10. Salvar, restaurar, desfazer e publicar

### 10.1 Salvamento e restauração

Salvar criará versão imutável e registros relacionados numa transação de negócio, usando tabelas transacionais compatíveis. A interface só exibirá “salvo” depois da confirmação. Falha de gravação manterá o estado como não salvo e permitirá repetição idempotente.

Ao restaurar: validar autorização e contexto; verificar formato e integridade; carregar snapshot; remontar índices e matrizes; consolidar regras; revalidar; apresentar origem e alertas. Formatos antigos precisarão de conversores versionados, sem reescrever silenciosamente a história.

Checkpoints e autosalvamento de rascunhos permitirão recuperar trabalho. Não será necessário persistir cada nó interno da busca, mas candidatos retidos, marcos relevantes e resumos das tentativas serão preservados conforme política. Reiniciar do checkpoint de negócio não equivale a retomar a instrução interna exata do solucionador.

### 10.2 Desfazer e refazer

Durante a edição, o histórico restaurará grade, travas e regras simultaneamente. Ao salvar, formarão versões ou marcos identificáveis. Depois de publicar, “voltar” significará nova publicação auditada da versão escolhida, não apagar o passado.

A interface distinguirá desfazer a última edição, restaurar versão salva e trocar a versão oficial. São operações diferentes, sem botão ambíguo.

### 10.3 Versão oficial e vigência

Somente usuários autorizados publicarão. A publicação verificará validação, aprovações, contexto, vigência e revisão esperada. Troca da referência oficial e evento de auditoria serão atômicos. As outras alternativas continuarão salvas.

A referência oficial considerará escola, ano, período e vigência: uma grade de fevereiro pode ser substituída a partir de março. Não permitir publicações conflitantes para o mesmo intervalo. O desenho físico definirá o controle de intervalos e concorrência, incluindo transação e bloqueio apropriado.

### 10.4 Cadastros alterados depois

Ao abrir versão histórica, oferecer “ver como foi salva” e, separadamente, “comparar com os cadastros atuais”. Novos professores, cargas e indisponibilidades produzirão diferenças explícitas. Incorporar mudanças cria outra versão com decisão registrada.

Nova indisponibilidade obrigatória pode tornar grade antiga inadequada para nova publicação. Preservar histórico não significa permitir republicá-lo sem revalidação. Não misturar automaticamente regras antigas e atuais ocultando esse risco.

## 11. Persistência MySQL e tabelas propostas

O banco compartilhado continuará sendo o ambiente informado para desenvolvimento, com isolamento lógico pelo prefixo. Confirmar o destino antes de implantar. O prefixo sozinho não resolve autorização, isolamento entre escolas ou conflitos de migração.

### 11.1 Núcleo de tabelas

- `hprof_cenario`: identidade, contexto, título, origem e revisão de concorrência.
- `hprof_cenarioversao`: versão imutável, parentesco, snapshot, formato, hash, autoria e validação.
- `hprof_cenarioregra`: projeções das regras e exceções da versão, com origem, escopo e aprovação.
- `hprof_simulacao`: trabalho, versão-base, modo, parâmetros, orçamento, andamento e referências dos resultados.
- `hprof_cenarioevento`: trilha de ações, aprovações, restaurações e mudanças da referência oficial.

Complementos a avaliar no desenho físico: `hprof_publicacao`, para vigências e histórico oficial; `hprof_simulacaotentativa`, para indicadores das tentativas. A tabela técnica do Flyway será `hprof_flyway_schema_history`, mediante configuração própria.

São nomes propostos, não tabelas já criadas. A lista histórica de tabelas existentes renomeadas está em `RELATORIO_TABELAS_HPROF.md`, no frontend, e continuará mantida separadamente.

### 11.2 Integridade e índices

Definir chaves e índices por contexto, versão, parentesco, trabalhos e vigência. Garantir que a versão-pai pertença ao contexto permitido; não confiar apenas no ID do navegador. Datas, versões e estados terão regras claras de transição.

Usar controle otimista de concorrência para edição e publicação; recusar revisão antiga. Definir idempotência para salvar, enfileirar e publicar. Duplo clique não deverá criar duas publicações ou duplicar resultados.

Medir o tamanho do snapshot antes de escolher JSON nativo ou conteúdo serializado em outro tipo adequado. Compressão, fragmentação ou armazenamento externo só entrarão se necessários, mantendo atomicidade lógica. Evitar exclusão em cascata que apague histórico ao remover um cadastro.

## 12. Flyway e administração pelo Swagger

Flyway versionará os scripts SQL das novas estruturas. A integração proposta usa o executável ou executor controlado no ambiente de implantação; não exige que a aplicação seja Java. A API poderá expor operações administrativas delimitadas para uso pelo Swagger. [R4, R5]

Como o banco existe e é compartilhado, primeiro inventariar histórico e tabelas. Não executar baseline automático, EnsureCreated ou renomeações em massa sem verificação. O Entity Framework poderá continuar no acesso a dados, mas não disputará as mesmas migrações com Flyway.

### 12.1 Política de migração

- Diretório de scripts e configuração exclusivos do Horário Professor.
- Histórico `hprof_flyway_schema_history`, sem interferir em outras aplicações.
- Scripts revisados junto ao código; nunca SQL arbitrário enviado pelo Swagger.
- Consulta de estado e validação antes de aplicar, conferindo destino e versões.
- Script aplicado não será editado; correções virão em nova versão.
- Não expor clean, exclusão de tabelas ou repair automático como ações comuns.
- Credencial de migração separada da credencial limitada da aplicação.
- Auditoria de executor, versão, checksum, horário e resultado, sem segredos.

Histórico e validação permitem identificar migrações e divergências de checksum. Não substituem revisão do SQL nem teste de restauração. [R4, R6]

### 12.2 Acesso administrativo seguro

Propor endpoints protegidos para consultar, validar e solicitar aplicação das migrações empacotadas. Exigir autorização administrativa, confirmação de destino e versões, prevenção de concorrência e auditoria. Restringir essa capacidade por configuração; em produção, preferir execução controlada na implantação.

Swagger será somente o cliente. Não receber senha do banco, caminho arbitrário ou comando de terminal na solicitação. Publicação de tabelas não ficará disponível para qualquer usuário autenticado da escola.

### 12.3 Sequência dos scripts e recuperação

Sequência conceitual, a numerar após inventário: cenários e versões; regras, eventos e publicações; simulações e tentativas; índices e ajustes do protótipo. Cada etapa terá teste em banco descartável e plano de recuperação.

No MySQL, várias operações DDL provocam commit implícito. Falha na migração não garante rollback integral da estrutura. A recuperação combinará backup testado e migrações corretivas, conforme o caso. Desfazer grade é operação de negócio, não rollback de esquema. [R7]

## 13. Contratos de API propostos

Os contratos abaixo são planejamento, não endpoints disponíveis. Definir DTOs próprios, JSON consistente, validação de filtros e autorização por escola. Evitar acoplamento do novo motor a HTML de telas parciais.

| Método | Rota proposta | Finalidade |
| --- | --- | --- |
| POST | /api/cenarios | Criar cenário de origem autorizada |
| GET | /api/cenarios | Listar por contexto |
| GET | /api/cenarios/{id}/versoes | Consultar histórico |
| POST | /api/cenarios/{id}/versoes | Salvar nova versão |
| GET | /api/cenarioversoes/{id} | Recuperar estado completo |
| POST | /api/simulacoes | Enfileirar busca |
| GET | /api/simulacoes/{id} | Consultar progresso e resultados |
| POST | /api/simulacoes/{id}/cancelamento | Solicitar cancelamento |
| GET | /api/cenarioversoes/comparacao | Comparar versões autorizadas |
| POST | /api/cenarioversoes/{id}/aprovacoes | Aceitar exceções específicas |
| POST | /api/publicacoes | Eleger oficial com vigência |
| GET | /api/admin/migracoes | Consultar estado protegido |
| POST | /api/admin/migracoes/validacao | Validar pacote permitido |
| POST | /api/admin/migracoes/execucoes | Solicitar aplicação controlada |

Início assíncrono retornará identificação e estado, tipicamente HTTP 202. Consultas não iniciarão processamento. Publicação concorrente poderá retornar conflito. Falta de permissão não vazará dados de outra escola. Não usar 200 com lista vazia para esconder falha de banco ou de validação.

O servidor validará contexto inclusive para comparar e restaurar. IDs e parâmetros recebidos não conferem autorização. Credenciais e dados sensíveis não aparecerão em URLs, logs ou snapshots.

## 14. Telas e relatórios para decisão

Manter a navegação conhecida e acrescentar cenários ao processamento. A tela mostrará escola, ano, período, origem, cenário atual e indicação inequívoca de rascunho, candidata ou oficial.

### 14.1 Painel de simulação

Escolher modo, professores liberados, posições travadas, preferências negociáveis, limite de mudanças e orçamento de execução. Mostrar andamento, cancelamento, último salvamento e resultados disponíveis.

As alternativas terão indicadores comparáveis e acesso à grade completa. A visualização destacará antes e depois por aula e identificará regras modificadas. Cor será complemento, não única informação: usar texto, legendas e filtros.

### 14.2 Relatórios necessários

- Pendências por turma e professor, com cargas previstas e atendidas.
- Disponibilidade versus demanda e gargalos detectáveis antes da busca.
- Preferências mantidas, contrariadas ou substituídas por exceções aprovadas.
- Impacto: professores, turmas, aulas e posições afetadas.
- Travas, escopo e pedidos de ampliação não aprovados.
- Histórico, origem dos dados, decisões e vigências oficiais.
- Diferenças entre snapshot e cadastro atual para manutenção futura.

As mensagens separarão erro técnico, ausência de dados e inviabilidade de horário. Selecionar 2025 não pode consultar silenciosamente 2026. Lista vazia deve refletir filtro e resposta reais, não mascarar exceção.

## 15. Execução assíncrona e desempenho

Simulações serão trabalhos identificados, desacoplados da duração da requisição HTTP. Inicialmente, avaliar trabalhador na aplicação com fila persistida; se custo e hospedagem exigirem, mover o cálculo para processo separado. A decisão depende de medições e limites de implantação.

Definir limites de tempo por tentativa e trabalho, paralelismo, memória, candidatos e fila por escola. Dezenas de simulações não significam dezenas de tarefas ilimitadas. Consultas e cancelamentos deverão continuar disponíveis durante a busca.

Estados previstos: solicitado, executando, concluído, cancelado, interrompido e falhou. Reinicialização deverá identificar trabalhos abandonados por heartbeat ou mecanismo equivalente e permitir recuperação segura de dados salvos. Não anunciar continuidade exata do algoritmo após desligamento.

Registrar tempos, tamanho do modelo, tentativas, melhor resultado, motivo de término e consumo observado. Guardar sementes, versões e parâmetros para comparação reproduzível, sem prometer resultados bit a bit idênticos entre plataformas, versões e execução paralela.

Antes de adotar OR-Tools, validar pacote C#, bibliotecas nativas, arquitetura do servidor, sistema operacional, permissões e limites de CPU/memória. A viabilidade depende também desse ensaio operacional, não só do algoritmo.

Não haverá dependência de IA generativa em cada distribuição. O fechamento será calculado por código e solucionador com regras explícitas. Enviar dados de professores a serviço externo não faz parte desta proposta.

## 16. Testes e critérios de aceite

Construir casos pequenos com solução conhecida e casos reais anonimizados ou protegidos. Comparar legado e novo motor sobre a mesma fotografia, sem disparar gravações do fluxo antigo durante leitura de diagnóstico.

| ID | Verificação | Critério de aprovação |
| --- | --- | --- |
| A01 | Contexto | Consultar escola, ano e período corretos e autorizados |
| A02 | Validade | Nenhum conflito obrigatório ou carga indevida |
| A03 | Regra obrigatória | Jamais flexibilizar no modo de sugestões |
| A04 | Exceção | Respeitar escopo, limite e vigência autorizados |
| A05 | Ajuste cirúrgico | Zero alteração fora da autorização |
| A06 | Cadeia de trocas | Resolver caso conhecido com mais de duas movimentações |
| A07 | Alternativas | Até três grades válidas sem duplicação |
| A08 | Impossibilidade | Distinguir prova de inviabilidade de tempo esgotado |
| A09 | Independência | Não alterar origem nem outra tentativa |
| A10 | Restauração | Reabrir grade, regras e travas após reinício |
| A11 | Desfazer | Restaurar conteúdo e regras preservando histórico |
| A12 | Falha ao salvar | Não declarar sucesso; repetir sem duplicar |
| A13 | Concorrência | Não sobrescrever publicação silenciosamente |
| A14 | Isolamento | Negar acesso ao cenário de outra escola |
| A15 | Cadastro posterior | Mostrar diferenças preservando o histórico |
| A16 | Migrações | Aplicar em cópia sem afetar tabelas vizinhas |
| A17 | Recuperação | Restaurar backup isolado e validar acesso |
| A18 | Interrupção | Informar estado e recuperar dados confirmados |
| A19 | Frontend | Tratar erros, listas vazias e filtros inválidos |
| A20 | Desempenho | Cumprir orçamento acordado no hardware-alvo |

Não fixar prazo universal de segundos ou taxa de fechamento sem medições. Registrar tamanho dos casos, tempo, memória, pendências, impacto e concessões. O gestor aprovará metas após observar a linha de base.

Piloto sugerido: CEU EMEF MENINOS, ano 2025, manhã, conforme exemplo apresentado. A captura completa e reprodução do conflito ainda precisam ocorrer; a imagem da tela não comprova qual restrição impede o fechamento.

## 17. Etapas e pontos de decisão

### Etapa 0 — Proteger e reproduzir a base

Confirmar repositórios e commits; preparar backup e testar restauração; capturar caso somente por leitura; listar regras ativas; reproduzir problemas de contexto. Preservar código comentado como referência, separado da especificação ativa.

Entrega: caso reproduzível, inventário de regras, linha de base e riscos priorizados. Saída: o gestor reconhece os dados e pendências como o caso real. Não avançar com escola ou ano incorretos.

### Etapa 1 — Armazenar e restaurar cenários

Definir snapshot, versões, regras, publicação e transições. Revisar scripts Flyway e implementar gravação, restauração e autorização. Testar reinício, concorrência e falha transacional antes do novo otimizador.

Entrega: salvar grade existente com regras e restaurá-la fielmente. Saída: A09 a A15 atendidos no escopo implementado, sem alterar cadastros. Esta é a primeira entrega funcional recomendada.

### Etapa 2 — Modelo canônico e validador

Criar adaptador de matrizes, identificadores, catálogo e validador independente. Encapsular o legado para comparação sem depender de efeitos colaterais para obter entradas.

Entrega: relatório verificável de validade e pendências. Saída: casos pequenos e piloto consistentes, com regras desconhecidas explicitadas.

### Etapa 3 — Prova de conceito CP-SAT

Implementar modelo mínimo, modo estrito, limites e comparação com legado. Testar hospedagem e bibliotecas nativas. Medir fechamento e custo com entradas iguais.

Entrega: decisão fundamentada de viabilidade. Saída: melhora ou utilidade comprovada nos casos-alvo; caso contrário, revisar modelo antes de ampliar a interface.

### Etapa 4 — Concessões e reparo localizado

Adicionar exceções, travas, orçamento de mudanças e reotimização parcial. Testar cadeias e limites de escopo. Implementar explicações sem confundir hipótese e prova.

Entrega: ajuste cirúrgico rastreável e sugestões de flexibilização. Saída: nenhuma mudança fora da autorização e validação integral da grade.

### Etapa 5 — Três opções e decisão do diretor

Integrar fila, progresso, cancelamento, comparação, histórico e publicação. Produzir alternativas distintas e mostrar impacto e aprovações pendentes. Permitir salvar e reabrir antes de eleger a oficial.

Entrega: jornada completa. Saída: teste de ponta a ponta, da cópia da origem até a reabertura após reinício.

### Etapa 6 — Piloto e implantação

Executar em paralelo ao procedimento atual, conferir com o gestor, corrigir divergências e testar recuperação. Implantar com opção de ativação, mantendo o caminho legado enquanto necessário.

Entrega: relatório comparativo e aceite operacional. Saída: desempenho, correção, suporte e retorno à versão anterior aprovados.

### Etapa 7 — Evolução orientada por evidência

Após o piloto, avaliar Redis, trabalhadores separados, outras meta-heurísticas e modernização mais ampla do frontend. Não atrasar persistência confiável nem substituir testes de negócio por novas tecnologias.

## 18. Git, backup e implantação segura

Bases identificadas: frontend `a979b49` e API `20ad006`, na branch `codex/base-antes-novo-fechamento`, publicadas nos respectivos repositórios. Na verificação desta elaboração, ambos estavam na branch de trabalho `codex/novo-fechamento`.

Isso não significa integração à main. Trabalhar em branch própria, revisar e promover para a branch de integração adotada. Se develop for usada, criar sua política explicitamente; não presumir que já existe ou está sincronizada.

Havia alterações locais em `appsettings.Development.json` e item não rastreado `sor2025_v2` no frontend. Preservá-los até revisão. Não incluí-los por `git add .` indiscriminado nem removê-los apenas para limpar o status.

### 18.1 Antes de alterar estruturas

- Confirmar commits e diferenças locais nos dois projetos.
- Gerar backup do banco e testar restauração isolada.
- Identificar esquema e histórico de migração das outras aplicações.
- Revisar credenciais em arquivos e histórico; planejar retirada e rotação em atividade própria.
- Aplicar migrações em cópia controlada, nunca experimentar sobre dados únicos.

Commit é histórico de código; push publica esse histórico; nenhum faz backup do banco. Backups e teste de restauração são tarefas do plano, não ações executadas pela geração deste documento.

Retorno à versão anterior da aplicação deve preservar leitura dos snapshots criados. Preferir mudanças aditivas de esquema e ativação gradual. Não retirar o motor antigo nem apagar históricos antes de demonstrar recuperação segura.

## 19. Pendências e riscos

### 19.1 Dados e aplicação atual

A migração de nomes para `hprof_` deve ser validada contra o banco real, incluindo SQL direto e mapeamentos. Na verificação anterior, `hprof_pesquisalog` estava pendente de confirmação, embora existisse a tabela sem prefixo. Revalidar antes de implantar, pois o banco pode ter mudado.

Nomes parecidos como `hprof_usuarioacesso` e `hprof_usuariosacesso` não devem ser fundidos por suposição. Versões antigas e prefixadas podem coexistir. Não excluir ou renomear automaticamente.

Há histórico de filtros incoerentes e dados em anos diferentes entre professores, turmas e opções. Adicionar 2025 e 2026 aos comboboxes dá escolha, mas não corrige vínculos inconsistentes no banco. O novo núcleo rejeitará ou explicará entradas incompletas sem trocar o ano silenciosamente.

Falhas de JavaScript, parâmetros indefinidos, carregamento de parciais e erros tratados como listas precisam de testes nas jornadas do piloto. Este planejamento não significa que todos os defeitos legados estejam resolvidos.

### 19.2 Riscos do novo motor

- Modelo incompleto: mitigar com catálogo e validador independente.
- Restrições excessivas: distinguir cadastro incorreto, preferência e impedimento real.
- Busca demorada: reduzir domínios, limitar escopo e medir no servidor-alvo.
- Concessões ocultas: relatório por regra e aprovação rastreável.
- Estado compartilhado: isolamento de snapshots, autorização e testes concorrentes.
- Restauração incompleta: teste de reinício e evolução de formato desde a primeira entrega.
- Migração em banco compartilhado: configuração isolada, backup, revisão e credenciais limitadas.

### 19.3 Decisões a validar com o gestor

Quais bloqueios são negociáveis? Quem autoriza exceções? Qual limite de alterações no ajuste pontual? O que desempata impacto e preferências? Como tratar professor compartilhado entre escolas ou períodos? Qual a vigência da grade oficial?

Definir retenção das tentativas, intervalo de autosalvamento, limite de simulações simultâneas e tratamento quando só uma ou duas alternativas forem encontradas. Não inventar respostas e transformá-las em regra permanente.

## 20. Por onde começar na próxima atividade

Começar pela Etapa 0 e pelo contrato de snapshot da Etapa 1. O primeiro objetivo demonstrável será abrir uma grade existente, salvar uma cópia completa e restaurá-la com suas regras, sem alterar o original. Só então integrar o novo solucionador.

1. Confirmar o contexto do piloto e capturar dados por leitura segura.
2. Mapear regras realmente executadas e classificá-las com o gestor.
3. Documentar resultado atual e pequenos testes de troca e bloqueio.
4. Revisar snapshot e tabelas `hprof_`, incluindo publicação por vigência.
5. Preparar e testar Flyway em cópia do banco com recuperação comprovada.
6. Entregar salvar/restaurar e validador; depois iniciar o protótipo CP-SAT.

Antes do motor, aprovar regras e contrato de estado. Antes de qualquer publicação, exigir validação independente e aceite das concessões. A cada etapa, atualizar este documento, testes e decisões pendentes.

## Apêndice A — Referências do código

Caminhos relativos a cada repositório, identificando pontos de partida da leitura direcionada. A posição das linhas poderá mudar com novas edições.

### Frontend: HorarioProfessor2025_v2

- `Views/Horario/Horarios.cshtml`: controles de processamento.
- `Views/Home/Index.cshtml`: chamadas, integração e filtros.
- `Controllers/HorarioController.cs`: API, sessão, trocas e persistência.
- `Controllers/HorarioConcluidoController.cs`: recuperação de grades.
- `Classes/HorarioApiConfiguration.cs`: configuração de acesso à API.
- `RELATORIO_TABELAS_HPROF.md`: nomes das tabelas do legado.

### Backend: api-horarioprofessor

- `Controllers/PrincipalExecucaoController.cs`: entrada e preparação de opções.
- `Services/CoreServices/PrincipalService.cs`: chamada principal.
- `Services/CoreServices/AppState.cs`: matrizes e sequência ativa.
- `Services/CoreServices/Sources/Gerenciamento/WorkflowManager.cs`: encadeamento das rotinas.
- `Services/ServicesCore/OpcoesProfessorService.cs`: opções de processamento.
- `Data/ApplicationDbContext.cs`: mapeamentos.
- `Program.cs`: registro de serviços e configuração.
- `API-HorarioProfessor.csproj`: plataforma e dependências.

## Apêndice B — Fontes e manutenção do plano

Fontes oficiais consultadas para escolhas e cuidados técnicos. Arquitetura, tabelas e contratos são propostas específicas, não transcrições dessas fontes. Confirmar versões, licenças e compatibilidade de pacotes na implementação.

- [R1] Google OR-Tools — Employee Scheduling: https://developers.google.com/optimization/scheduling/employee_scheduling
- [R2] Google OR-Tools — CP-SAT Solver: https://developers.google.com/optimization/cp/cp_solver
- [R3] Google OR-Tools — Troubleshooting: https://github.com/google/or-tools/blob/stable/ortools/sat/docs/troubleshooting.md
- [R4] Redgate Flyway — Schema History Table: https://documentation.red-gate.com/flyway/flyway-concepts/migrations/flyway-schema-history-table
- [R5] Redgate Flyway — Command-line: https://documentation.red-gate.com/flyway/reference/usage/command-line
- [R6] Redgate Flyway — Migrate: https://documentation.red-gate.com/flyway/reference/commands/migrate
- [R7] MySQL 8.0 — Implicit Commit: https://dev.mysql.com/doc/refman/8.0/en/implicit-commit.html

Manutenção: guardar este Markdown junto ao código e regenerar o Word após decisões materiais. Registrar data, etapa concluída, aprovações e pendências. Não marcar recurso como entregue porque seu endpoint foi documentado ou o protótipo executou uma vez.

Histórico: versão 1.0 — consolidação de fechamento, redistribuição, simulações, regras, persistência, Flyway e implantação. Nenhuma nova tabela ou rotina foi implementada por esta entrega documental.
