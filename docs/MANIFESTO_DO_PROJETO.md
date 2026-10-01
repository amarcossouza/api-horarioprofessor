# Manifesto de Engenharia — Horário Professor

Este manifesto contém regras obrigatórias para qualquer pessoa ou inteligência artificial que altere os projetos do Horário Professor.

## 1. Código-fonte e assemblies

- Nunca alterar DLLs, executáveis, assemblies, arquivos em `bin/` ou `obj/` ou qualquer outro artefato compilado.
- Toda correção ou evolução deve ser feita exclusivamente no código-fonte do projeto.
- Artefatos compilados são resultados do build, não fontes de manutenção.

## 2. Arquitetura e qualidade

- Todo código novo deve seguir princípios SOLID, separação de responsabilidades e injeção de dependências.
- Funcionalidades novas devem ser expostas por controllers e contratos de API claros.
- Controllers devem ser leves: validar a entrada, autorizar, chamar serviços e transformar a resposta HTTP.
- Regras de negócio, processamento de grade, acesso a dados e integrações não devem ficar dentro dos controllers.
- Serviços e arquivos novos devem permanecer pequenos e coesos.
- Código existente que já funciona não deve ser dividido, reescrito ou reorganizado apenas por estética ou estilo. A preferência é criar adapters, fachadas e serviços novos ao redor dele.
- Uma classe existente somente será modificada quando houver erro comprovado, necessidade inevitável de integração ou autorização explícita do responsável.
- Evitar código duplicado, dependências globais e acoplamento entre interface, banco de dados e motor de processamento.
- APIs devem retornar erros estruturados e não expor senha, string de conexão ou detalhes internos sensíveis.

## 3. Banco de dados e Flyway

- Nunca apagar, truncar, renomear ou modificar dados de tabelas pertencentes a outras aplicações.
- Toda mudança de estrutura do banco deve ser criada como uma nova migração versionada do Flyway.
- As tabelas novas do Horário Professor devem usar o prefixo `hprof_`.
- Migrações já aplicadas são imutáveis. Uma mudança posterior deve gerar o próximo arquivo `Vn__descricao.sql`.
- Nunca executar `clean`. A configuração deve manter `flyway.cleanDisabled=true`.
- Não usar `DROP TABLE`, `TRUNCATE`, exclusões em massa ou migrações destrutivas sem plano de preservação, backup verificado e autorização explícita do responsável.
- Antes de `migrate`, executar `info` e `validate`; depois, executar `info` novamente e registrar o resultado.
- Usar a tabela de histórico exclusiva `hprof_flyway_schema_history`.
- Senhas e strings de conexão não devem ser commitadas no Git.

## 4. Componentes oficiais

- Frontend MVC: `C:\projkinix\HorarioProfessor2025_v2` — porta 5051.
- API legada/principal: `C:\projkinix\api-horarioprofessor` — porta 5000.
- Microsserviço de cenários, validação e CP-SAT: `C:\projkinix\api-cenarios` — porta 5010.

## 5. Segurança da evolução

- Preservar o comportamento atual enquanto a funcionalidade nova não estiver validada.
- Evoluir por extensão: reaproveitar o processamento, matrizes e contratos existentes e adicionar capacidades novas em módulos separados.
- Não substituir o motor legado durante a implantação do novo fechamento. Os dois fluxos devem coexistir até o piloto ser aprovado.
- Implementar mudanças em partes pequenas, testáveis e rastreáveis.
- Não considerar uma funcionalidade concluída apenas porque o código existe: compilar, testar integração, testar persistência e validar com um caso real.
- Cenários, versões, concessões, simulações e publicações devem manter auditoria e possibilidade de restauração.
- Mudanças pontuais devem preservar por padrão professores, turmas e horários que não fazem parte do pedido.

## 6. Critério mínimo de entrega

Uma entrega somente pode ser marcada como concluída quando:

1. o código-fonte compila sem erros;
2. os contratos da API estão documentados;
3. o controller permanece leve e a regra está no serviço apropriado;
4. as migrações Flyway foram validadas;
5. os dados existentes foram preservados;
6. existe teste do fluxo principal e do retorno de erro;
7. os arquivos foram versionados no Git, sem credenciais e sem artefatos compilados.
