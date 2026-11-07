-- Módulo universidade: cria as tabelas novas (só CREATE TABLE IF NOT EXISTS) e faz a carga (só INSERT protegido).
-- Pode rodar mais de uma vez sem duplicar nada. Não apaga nem altera nada que já existe.
USE droplexdb;

-- ============================================================================
-- V8 — hprof_uni_instituicao
-- Módulo universidade — instituição (universidade/campus) que agrupa cursos
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_instituicao (
    id                  BIGINT       NOT NULL AUTO_INCREMENT,
    sigla               VARCHAR(40)  NOT NULL                COMMENT 'Ex.: UDESC-CESFI',
    nome                VARCHAR(200) NOT NULL,
    id_escola_principal INT          NOT NULL                COMMENT 'hprof_escolas.EscolaId usada no login da instituição',
    ativo               TINYINT(1)   NOT NULL DEFAULT 1,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_instituicao_sigla (sigla),
    UNIQUE KEY uk_uni_instituicao_escola (id_escola_principal)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — instituição (universidade/campus) que agrupa cursos';

-- ============================================================================
-- V9 — hprof_uni_curso
-- Módulo universidade — curso da instituição; cada curso é uma escola do sistema
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_curso (
    id              BIGINT       NOT NULL AUTO_INCREMENT,
    id_instituicao  BIGINT       NOT NULL,
    id_escola       INT          NOT NULL                COMMENT 'hprof_escolas.EscolaId do curso (referência de escola de sempre)',
    codigo          VARCHAR(40)  NOT NULL                COMMENT 'Código do curso na universidade (ex.: PET)',
    nome            VARCHAR(200) NOT NULL,
    turno           VARCHAR(60)  NULL,
    dias            VARCHAR(40)  NOT NULL DEFAULT 'SEG,TER,QUA,QUI,SEX' COMMENT 'Dias letivos do curso',
    intervalos_json JSON         NULL                    COMMENT 'Intervalos e almoço para os relatórios',
    ordem           INT          NOT NULL DEFAULT 1,
    ativo           TINYINT(1)   NOT NULL DEFAULT 1,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_curso_escola (id_escola),
    UNIQUE KEY uk_uni_curso_codigo (id_instituicao, codigo)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — curso da instituição; cada curso é uma escola do sistema';

-- ============================================================================
-- V10 — hprof_uni_turma
-- Módulo universidade — fase/turma com o nome completo usado pela universidade
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_turma (
    id              BIGINT       NOT NULL AUTO_INCREMENT,
    id_curso        BIGINT       NOT NULL,
    id_turmapadrao  INT          NULL                    COMMENT 'hprof_turmapadraoescola.TurmaId',
    codigo          VARCHAR(60)  NOT NULL                COMMENT 'Código na universidade',
    sigla           VARCHAR(20)  NOT NULL                COMMENT 'Nome curto gravado em hprof_turmapadraoescola.Turmas (ex.: 1F)',
    nome            VARCHAR(120) NOT NULL                COMMENT 'Nome exibido (ex.: 1ª Fase)',
    ordem           INT          NOT NULL DEFAULT 1,
    ativo           TINYINT(1)   NOT NULL DEFAULT 1,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_turma_codigo (id_curso, codigo),
    UNIQUE KEY uk_uni_turma_sigla (id_curso, sigla)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — fase/turma com o nome completo usado pela universidade';

-- ============================================================================
-- V11 — hprof_uni_pessoaprofessor
-- Módulo universidade — a pessoa do professor, acima dos cursos
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_pessoaprofessor (
    id              BIGINT       NOT NULL AUTO_INCREMENT,
    id_instituicao  BIGINT       NOT NULL,
    nome            VARCHAR(200) NOT NULL,
    nome_curto      VARCHAR(40)  NOT NULL                COMMENT 'Nome usado no cadastro do motor (antes do hífen); único na instituição',
    documento       VARCHAR(40)  NOT NULL                COMMENT 'Matrícula ou CPF; identifica a pessoa (não usar o nome)',
    carga_maxima    INT          NOT NULL                COMMENT 'Aulas por semana contratadas (ex.: 40)',
    ativo           TINYINT(1)   NOT NULL DEFAULT 1,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_pessoa_documento (id_instituicao, documento),
    UNIQUE KEY uk_uni_pessoa_nomecurto (id_instituicao, nome_curto)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — a pessoa do professor, acima dos cursos';

-- ============================================================================
-- V12 — hprof_uni_pessoavinculo
-- Módulo universidade — divisão da carga da pessoa entre cursos
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_pessoavinculo (
    id                  BIGINT      NOT NULL AUTO_INCREMENT,
    id_pessoa           BIGINT      NOT NULL,
    id_escola           INT         NOT NULL                COMMENT 'Curso = escola no sistema',
    ano_letivo          INT         NOT NULL,
    periodo             VARCHAR(20) NOT NULL,
    aulas_reservadas    INT         NOT NULL                COMMENT 'Parte da carga reservada para este curso',
    ordem_processamento INT         NOT NULL DEFAULT 1      COMMENT 'Qual curso é montado primeiro',
    ativo               TINYINT(1)  NOT NULL DEFAULT 1,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_vinculo (id_pessoa, id_escola, ano_letivo, periodo)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — divisão da carga da pessoa entre cursos';

-- ============================================================================
-- V13 — hprof_uni_pessoacadastro
-- Módulo universidade — liga a pessoa às linhas de hprof_cadastroprofessor
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_pessoacadastro (
    id                   BIGINT      NOT NULL AUTO_INCREMENT,
    id_pessoa            BIGINT      NOT NULL,
    id_cadastroprofessor INT         NOT NULL                COMMENT 'hprof_cadastroprofessor.ProfessorId (tabela existente, não alterada)',
    id_escola            INT         NOT NULL,
    papel                VARCHAR(12) NOT NULL DEFAULT 'PRINCIPAL' COMMENT 'PRINCIPAL ou APOIO (aula com dois professores)',
    ativo                TINYINT(1)  NOT NULL DEFAULT 1,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_pessoacadastro (id_cadastroprofessor, id_pessoa),
    INDEX idx_uni_pessoacadastro_pessoa (id_pessoa)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — liga a pessoa às linhas de hprof_cadastroprofessor';

-- ============================================================================
-- V14 — hprof_uni_publicacao
-- Módulo universidade — grade salva escolhida como oficial do curso
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_publicacao (
    id            BIGINT       NOT NULL AUTO_INCREMENT,
    id_escola     INT          NOT NULL,
    ano_letivo    INT          NOT NULL,
    periodo       VARCHAR(20)  NOT NULL,
    id_planilha   INT          NOT NULL                COMMENT 'hprof_horariodistribuicaojson.Id',
    ativo         TINYINT(1)   NOT NULL DEFAULT 1      COMMENT 'Só a última publicação fica ativa',
    publicado_por VARCHAR(100) NOT NULL,
    publicado_em  DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    total_aulas   INT          NOT NULL DEFAULT 0,
    PRIMARY KEY (id),
    INDEX idx_uni_publicacao_escola (id_escola, ano_letivo, periodo, ativo)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — grade salva escolhida como oficial do curso';

-- ============================================================================
-- V15 — hprof_uni_pessoaocupacao
-- Módulo universidade — horários ocupados da pessoa, em horas do relógio
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_pessoaocupacao (
    id               BIGINT       NOT NULL AUTO_INCREMENT,
    id_pessoa        BIGINT       NOT NULL,
    id_escola_origem INT          NOT NULL                COMMENT 'Curso onde a aula foi publicada',
    ano_letivo       INT          NOT NULL,
    periodo          VARCHAR(20)  NOT NULL,
    dia_semana       TINYINT      NOT NULL                COMMENT '1=segunda ... 6=sábado',
    hora_inicio      TIME         NOT NULL,
    hora_fim         TIME         NOT NULL,
    descricao        VARCHAR(200) NULL                    COMMENT 'Ex.: 3F · Cálculo III',
    origem           VARCHAR(20)  NOT NULL                COMMENT 'PUBLICACAO, INTEGRACAO ou MANUAL',
    id_publicacao    BIGINT       NULL,
    ativo            TINYINT(1)   NOT NULL DEFAULT 1,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    INDEX idx_uni_ocupacao_pessoa (id_pessoa, ano_letivo, ativo),
    INDEX idx_uni_ocupacao_origem (id_escola_origem, ano_letivo, periodo, ativo)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — horários ocupados da pessoa, em horas do relógio';

-- ============================================================================
-- V16 — hprof_uni_integracaocliente
-- Módulo universidade — sistemas autorizados a enviar dados pela API
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_integracaocliente (
    id                   BIGINT       NOT NULL AUTO_INCREMENT,
    id_instituicao       BIGINT       NOT NULL,
    sigla                VARCHAR(40)  NOT NULL,
    nome                 VARCHAR(200) NOT NULL,
    chave_hash           CHAR(64)     NOT NULL                COMMENT 'SHA-256 da chave; a chave nunca é gravada',
    aplicacao_automatica TINYINT(1)   NOT NULL DEFAULT 0      COMMENT '0 = espera aprovação do gestor',
    configuracao_json    JSON         NULL                    COMMENT 'Particularidades da instituição',
    ativo                TINYINT(1)   NOT NULL DEFAULT 1,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_cliente_sigla (sigla),
    UNIQUE KEY uk_uni_cliente_chave (chave_hash)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — sistemas autorizados a enviar dados pela API';

-- ============================================================================
-- V17 — hprof_uni_integracaolote
-- Módulo universidade — cada envio, guardado como chegou
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_integracaolote (
    id              BIGINT       NOT NULL AUTO_INCREMENT,
    id_instituicao  BIGINT       NOT NULL,
    id_cliente      BIGINT       NULL                    COMMENT 'Nulo quando veio da planilha ou da tela',
    origem          VARCHAR(12)  NOT NULL DEFAULT 'API' COMMENT 'API, PLANILHA ou TELA',
    codigo_envio    VARCHAR(100) NOT NULL,
    tipo            VARCHAR(30)  NOT NULL                COMMENT 'PESSOAS ou CURSO',
    ano_letivo      INT          NOT NULL,
    periodo         VARCHAR(20)  NOT NULL,
    conteudo_json   JSON         NOT NULL                COMMENT 'Envio original, imutável',
    hash_conteudo   CHAR(64)     NOT NULL,
    status          VARCHAR(20)  NOT NULL DEFAULT 'RECEBIDO' COMMENT 'RECEBIDO, VALIDADO, COM_ERROS, REJEITADO, APLICADO',
    total_itens     INT          NOT NULL DEFAULT 0,
    total_erros     INT          NOT NULL DEFAULT 0,
    total_avisos    INT          NOT NULL DEFAULT 0,
    enviado_por     VARCHAR(100) NOT NULL,
    recebido_em     DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    recebido_de_ip  VARCHAR(45)  NULL,
    validado_em     DATETIME(3)  NULL,
    aprovado_por    VARCHAR(100) NULL,
    aprovado_em     DATETIME(3)  NULL,
    aplicado_em     DATETIME(3)  NULL,
    motivo_rejeicao VARCHAR(500) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_lote_envio (id_instituicao, codigo_envio),
    INDEX idx_uni_lote_status (id_instituicao, status)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — cada envio, guardado como chegou';

-- ============================================================================
-- V18 — hprof_uni_integracaoerro
-- Módulo universidade — erros e avisos de cada item do envio
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_integracaoerro (
    id          BIGINT       NOT NULL AUTO_INCREMENT,
    id_lote     BIGINT       NOT NULL,
    secao       VARCHAR(40)  NOT NULL,
    item        INT          NOT NULL                COMMENT 'Posição do item no envio (1 = primeiro)',
    campo       VARCHAR(60)  NULL,
    mensagem    VARCHAR(500) NOT NULL,
    severidade  VARCHAR(10)  NOT NULL DEFAULT 'ERRO' COMMENT 'ERRO bloqueia; AVISO só informa',
    PRIMARY KEY (id),
    INDEX idx_uni_erro_lote (id_lote)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — erros e avisos de cada item do envio';

-- ============================================================================
-- V19 — hprof_uni_integracaomapa
-- Módulo universidade — código da universidade ↔ id do Horário Professor
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_integracaomapa (
    id             BIGINT       NOT NULL AUTO_INCREMENT,
    id_instituicao BIGINT       NOT NULL,
    entidade       VARCHAR(30)  NOT NULL                COMMENT 'PESSOA, CURSO, TURMA, DISCIPLINA, ATRIBUICAO',
    codigo_externo VARCHAR(120) NOT NULL,
    tabela_interna VARCHAR(64)  NOT NULL,
    id_interno     BIGINT       NOT NULL,
    ativo          TINYINT(1)   NOT NULL DEFAULT 1      COMMENT '0 = deixou de vir no reenvio (desativado, não apagado)',
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_mapa (id_instituicao, entidade, codigo_externo)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — código da universidade ↔ id do Horário Professor';

-- ============================================================================
-- V20 — hprof_uni_integracaoauditoria
-- Módulo universidade — quem fez o quê em cada envio
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_integracaoauditoria (
    id          BIGINT        NOT NULL AUTO_INCREMENT,
    id_lote     BIGINT        NOT NULL,
    acao        VARCHAR(20)   NOT NULL                COMMENT 'RECEBIDO, VALIDADO, VISUALIZADO, APROVADO, REJEITADO, APLICADO, FALHOU',
    usuario     VARCHAR(100)  NOT NULL,
    tipo_ator   VARCHAR(20)   NOT NULL                COMMENT 'INTEGRACAO, USUARIO ou SISTEMA',
    ip          VARCHAR(45)   NULL,
    ocorrido_em DATETIME(3)   NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    detalhe     VARCHAR(1000) NULL,
    PRIMARY KEY (id),
    INDEX idx_uni_auditoria_lote (id_lote, ocorrido_em)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — quem fez o quê em cada envio';

-- ============================================================================
-- V21 — hprof_uni_integracaoalteracao
-- Módulo universidade — cada registro gravado por um envio (antes/depois)
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_integracaoalteracao (
    id           BIGINT       NOT NULL AUTO_INCREMENT,
    id_lote      BIGINT       NOT NULL,
    tabela       VARCHAR(64)  NOT NULL,
    id_registro  BIGINT       NOT NULL,
    operacao     VARCHAR(12)  NOT NULL                COMMENT 'INSERIR, ATUALIZAR, DESATIVAR',
    valor_antes  JSON         NULL,
    valor_depois JSON         NULL,
    gravado_em   DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    INDEX idx_uni_alteracao_lote (id_lote),
    INDEX idx_uni_alteracao_registro (tabela, id_registro)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — cada registro gravado por um envio (antes/depois)';

-- ============================================================================
-- R__ (repetível) — DEMONSTRAÇÃO UDESC CESFI (módulo universidade)
-- Repetível de propósito: não bloqueia as próximas migrações versionadas (V22, V23…).
-- Todos os INSERTs são protegidos: rodar de novo não duplica nada.
-- Rodar SOMENTE no banco de demonstração/homologação, com flyway/sql-demo incluída em
-- flyway.locations (veja flyway/sql-demo/LEIA-ME.md). Só ADICIONA registros.
--   tabelas antigas: hprof_escolas (escola da instituição) e hprof_usuariosacesso (login)
--   tabelas novas  : instituição, cliente de integração e 3 envios RECEBIDOS
-- Os cursos, turmas, disciplinas, professores e atribuições entram nas tabelas antigas
-- quando o gestor aprova os envios na tela Universidade > Envios (com auditoria).
-- Login de demonstração: udesc.demo / udesc2026  (troque depois do teste)
-- Chave de integração de demonstração: UDESC-DEMO-2026-troque-esta-chave
-- ============================================================================

INSERT INTO hprof_escolas (CNPJ, Nome, Endereco, Bairro, Estado, Municipio, Diretor, ViceDiretor, Coordenador,
                           Contato, Telefone, Celular, Email, QuantidadeTurmas, QuantidadeTurnos, TipoEscola,
                           UsuarioAcesso, Senha, QuantidadeHorarios, QuantidadeHorariosManha, QuantidadeHorariosTarde, QuantidadeHorariosNoite)
SELECT '', 'UDESC CESFI (demonstração)', 'Av. Lourival Cesario Pereira, s/n', '', 'SC', 'Balneário Camboriú', '', '', '',
       '', '', '', '', 0, 1, 'Universidade', 'udesc.demo', 'udesc2026', 10, 10, 0, 0 FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM hprof_escolas WHERE Nome = 'UDESC CESFI (demonstração)');

SET @id_escola_udesc = (SELECT EscolaId FROM hprof_escolas WHERE Nome = 'UDESC CESFI (demonstração)' ORDER BY EscolaId LIMIT 1);

INSERT INTO hprof_usuariosacesso (EscolaId, Usuario, Senha, DataCadastro)
SELECT @id_escola_udesc, 'udesc.demo', 'udesc2026', NOW() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM hprof_usuariosacesso WHERE Usuario = 'udesc.demo');

INSERT INTO hprof_uni_instituicao (sigla, nome, id_escola_principal, criado_por)
SELECT 'UDESC-CESFI-DEMO', 'UDESC CESFI — Centro de Educação Superior da Foz do Itajaí (demonstração)', @id_escola_udesc, 'flyway-demo' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM hprof_uni_instituicao WHERE sigla = 'UDESC-CESFI-DEMO');
SET @id_inst = (SELECT id FROM hprof_uni_instituicao WHERE sigla = 'UDESC-CESFI-DEMO');

INSERT INTO hprof_uni_integracaocliente (id_instituicao, sigla, nome, chave_hash, aplicacao_automatica, criado_por)
SELECT @id_inst, 'UDESC-CESFI-DEMO', 'Sistema acadêmico UDESC (demonstração)', '9c16284228c03d9ab2a85b3f5ab2f9f34c5b977e2a6defbc5bcfe9ea75f8eda8', 0, 'flyway-demo' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM hprof_uni_integracaocliente WHERE sigla = 'UDESC-CESFI-DEMO');
SET @id_cliente = (SELECT id FROM hprof_uni_integracaocliente WHERE sigla = 'UDESC-CESFI-DEMO');

INSERT INTO hprof_uni_integracaolote (id_instituicao, id_cliente, origem, codigo_envio, tipo, ano_letivo, periodo,
                                      conteudo_json, hash_conteudo, status, total_itens, enviado_por)
SELECT @id_inst, @id_cliente, 'API', 'UDESC-PESSOAS-2026-2-001', 'PESSOAS', 2026, 'Manha',
        '{"codigoEnvio":"UDESC-PESSOAS-2026-2-001","anoLetivo":2026,"periodo":"Manha","professores":[{"codigo":"P0001","nome":"Damianni Sebrão","documento":"DEMO-0001","cargaMaxima":40},{"codigo":"P0002","nome":"Carla Corina Porto","documento":"DEMO-0002","cargaMaxima":40},{"codigo":"P0003","nome":"Patricia Vilar Vitor Salinas","documento":"DEMO-0003","cargaMaxima":40},{"codigo":"P0004","nome":"Carlos Pecorari Neto","documento":"DEMO-0004","cargaMaxima":40},{"codigo":"P0005","nome":"Leonardo D. Knippenberg","documento":"DEMO-0005","cargaMaxima":40},{"codigo":"P0006","nome":"Muriel De Pauli","documento":"DEMO-0006","cargaMaxima":40},{"codigo":"P0007","nome":"Eduardo José Paiva Mendes","documento":"DEMO-0007","cargaMaxima":40},{"codigo":"P0008","nome":"Rafael Rodrigues Francisco","documento":"DEMO-0008","cargaMaxima":40},{"codigo":"P0009","nome":"Renata Wandroski Peris Wolf","documento":"DEMO-0009","cargaMaxima":40},{"codigo":"P0010","nome":"Diego Alex Mayer","documento":"DEMO-0010","cargaMaxima":40},{"codigo":"P0011","nome":"Luiz Antonio Alves","documento":"DEMO-0011","cargaMaxima":40},{"codigo":"P0012","nome":"Marvim Mota","documento":"DEMO-0012","cargaMaxima":40},{"codigo":"P0013","nome":"Débora Cristina Brandt","documento":"DEMO-0013","cargaMaxima":40},{"codigo":"P0014","nome":"Carolina Klein Padilha","documento":"DEMO-0014","cargaMaxima":40},{"codigo":"P0015","nome":"Daniel Fabian Bettú","documento":"DEMO-0015","cargaMaxima":40},{"codigo":"P0016","nome":"Rafael Costa Engel","documento":"DEMO-0016","cargaMaxima":40},{"codigo":"P0017","nome":"Gustavo Gondran Ribeiro","documento":"DEMO-0017","cargaMaxima":40},{"codigo":"P0018","nome":"Taiane Pereira Reis","documento":"DEMO-0018","cargaMaxima":40},{"codigo":"P0019","nome":"Luiz Adolfo Hegele Jr","documento":"DEMO-0019","cargaMaxima":40},{"codigo":"P0020","nome":"Michele Schmitt","documento":"DEMO-0020","cargaMaxima":40},{"codigo":"P0021","nome":"Carlos Eduardo M. de Andrade","documento":"DEMO-0021","cargaMaxima":40},{"codigo":"P0022","nome":"Aníbal Alexandre Campos Bonilla","documento":"DEMO-0022","cargaMaxima":40},{"codigo":"P0023","nome":"Gustavo Trindade Choaire","documento":"DEMO-0023","cargaMaxima":40},{"codigo":"P0024","nome":"Antônio Marinho Barbosa Neto","documento":"DEMO-0024","cargaMaxima":40},{"codigo":"P0025","nome":"Vinícius Czarnobay","documento":"DEMO-0025","cargaMaxima":40},{"codigo":"P0026","nome":"Lindaura Maria Stefens","documento":"DEMO-0026","cargaMaxima":40},{"codigo":"P0027","nome":"Marcos Vieira (fictício)","documento":"DEMO-0027","cargaMaxima":20},{"codigo":"P0028","nome":"Juliana Ferraz (fictício)","documento":"DEMO-0028","cargaMaxima":20},{"codigo":"P0029","nome":"Fernanda Rocha (fictício)","documento":"DEMO-0029","cargaMaxima":20},{"codigo":"P0030","nome":"Ricardo Santos (fictício)","documento":"DEMO-0030","cargaMaxima":20}]}', '79d1343ae11808ae073af15ef5a9d073575f30ab0d7f5c169746e25dfa605ad7', 'RECEBIDO', 30, 'UDESC-CESFI-DEMO' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM hprof_uni_integracaolote WHERE id_instituicao = @id_inst AND codigo_envio = 'UDESC-PESSOAS-2026-2-001');
INSERT INTO hprof_uni_integracaoauditoria (id_lote, acao, usuario, tipo_ator, detalhe)
SELECT l.id, 'RECEBIDO', 'UDESC-CESFI-DEMO', 'INTEGRACAO', 'Carga de demonstração (Flyway sql-demo)'
  FROM hprof_uni_integracaolote l WHERE l.id_instituicao = @id_inst AND l.codigo_envio = 'UDESC-PESSOAS-2026-2-001'
   AND NOT EXISTS (SELECT 1 FROM hprof_uni_integracaoauditoria a WHERE a.id_lote = l.id AND a.acao = 'RECEBIDO');

INSERT INTO hprof_uni_integracaolote (id_instituicao, id_cliente, origem, codigo_envio, tipo, ano_letivo, periodo,
                                      conteudo_json, hash_conteudo, status, total_itens, enviado_por)
SELECT @id_inst, @id_cliente, 'API', 'UDESC-PET-2026-2-001', 'CURSO', 2026, 'Manha',
        '{"codigoEnvio":"UDESC-PET-2026-2-001","anoLetivo":2026,"periodo":"Manha","curso":{"codigo":"PET","nome":"Engenharia de Petróleo","turno":"Integral","dias":["SEG","TER","QUA","QUI","SEX","SAB"]},"horariosAula":[{"aula":1,"inicio":"08:20","fim":"09:10"},{"aula":2,"inicio":"09:10","fim":"10:00"},{"aula":3,"inicio":"10:20","fim":"11:10"},{"aula":4,"inicio":"11:10","fim":"12:00"},{"aula":5,"inicio":"13:30","fim":"14:20"},{"aula":6,"inicio":"14:20","fim":"15:10"},{"aula":7,"inicio":"15:20","fim":"16:10"},{"aula":8,"inicio":"16:10","fim":"17:00"},{"aula":9,"inicio":"17:00","fim":"17:50"},{"aula":10,"inicio":"17:50","fim":"18:40"}],"intervalos":[{"nome":"Intervalo","inicio":"10:00","fim":"10:20"},{"nome":"Almoço","inicio":"12:00","fim":"13:30"},{"nome":"Intervalo","inicio":"15:10","fim":"15:20"}],"turmas":[{"codigo":"PET-1AFASE","nome":"1ª Fase"},{"codigo":"PET-2AFASE","nome":"2ª Fase"},{"codigo":"PET-2AFASEEXTRA","nome":"2ª Fase Extra"},{"codigo":"PET-3AFASE","nome":"3ª Fase"},{"codigo":"PET-4AFASE","nome":"4ª Fase"},{"codigo":"PET-5AFASE","nome":"5ª Fase"},{"codigo":"PET-6AFASE","nome":"6ª Fase"},{"codigo":"PET-7AFASE","nome":"7ª Fase"},{"codigo":"PET-8AFASE","nome":"8ª Fase"},{"codigo":"PET-9AFASE","nome":"9ª Fase"},{"codigo":"PET-OPTATIVAS","nome":"Optativas"}],"disciplinas":[{"codigo":"PET-1AFASE-QUIMICAGERAL","nome":"Química Geral","turma":"PET-1AFASE","aulasSemana":2},{"codigo":"PET-1AFASE-INTRODUCAOAENGENHARIADEPETROLEO","nome":"Introdução à Engenharia de Petróleo","turma":"PET-1AFASE","aulasSemana":4},{"codigo":"PET-1AFASE-QUIMICAEXPERIMENTALI","nome":"Química Experimental I","turma":"PET-1AFASE","aulasSemana":4},{"codigo":"PET-1AFASE-GEOMETRIAANALITICA","nome":"Geometria Analítica","turma":"PET-1AFASE","aulasSemana":2},{"codigo":"PET-1AFASE-CALCULODIFERENCIALEINTEGRALI","nome":"Cálculo Diferencial e Integral I","turma":"PET-1AFASE","aulasSemana":6},{"codigo":"PET-1AFASE-DESENHOTECNICO","nome":"Desenho Técnico","turma":"PET-1AFASE","aulasSemana":4},{"codigo":"PET-2AFASE-ALGEBRALINEAR","nome":"Álgebra Linear","turma":"PET-2AFASE","aulasSemana":4},{"codigo":"PET-2AFASE-QUIMICAEXPERIMENTALII","nome":"Química Experimental II","turma":"PET-2AFASE","aulasSemana":4},{"codigo":"PET-2AFASE-CALCULODIFERENCIALEINTEGRALII","nome":"Cálculo Diferencial e Integral II","turma":"PET-2AFASE","aulasSemana":4},{"codigo":"PET-2AFASE-CIENCIADOSMATERIAIS","nome":"Ciência dos Materiais","turma":"PET-2AFASE","aulasSemana":4},{"codigo":"PET-2AFASE-FISICAI","nome":"Física I","turma":"PET-2AFASE","aulasSemana":4},{"codigo":"PET-2AFASE-FISICAEXPERIMENTALI","nome":"Física Experimental I","turma":"PET-2AFASE","aulasSemana":4},{"codigo":"PET-2AFASE-ESTATISTICADESCRITIVA","nome":"Estatística Descritiva","turma":"PET-2AFASE","aulasSemana":2},{"codigo":"PET-2AFASE-QUIMICAORGANICAI","nome":"Química Orgânica I","turma":"PET-2AFASE","aulasSemana":2},{"codigo":"PET-2AFASEEXTRA-QUIMICAEXPERIMENTALIIEXTRA","nome":"Química Experimental II - EXTRA","turma":"PET-2AFASEEXTRA","aulasSemana":4},{"codigo":"PET-3AFASE-LINGUAGEMDEPROGRAMACAOI","nome":"Linguagem de Programação I","turma":"PET-3AFASE","aulasSemana":4},{"codigo":"PET-3AFASE-ESTATICA","nome":"Estática","turma":"PET-3AFASE","aulasSemana":4},{"codigo":"PET-3AFASE-GEOLOGIAGERAL","nome":"Geologia Geral","turma":"PET-3AFASE","aulasSemana":3},{"codigo":"PET-3AFASE-FISICAII","nome":"Física II","turma":"PET-3AFASE","aulasSemana":4},{"codigo":"PET-3AFASE-CALCULODIFERENCIALEINTEGRALIII","nome":"Cálculo Diferencial e Integral III","turma":"PET-3AFASE","aulasSemana":4},{"codigo":"PET-3AFASE-FISICAEXPERIMENTALII","nome":"Física Experimental II","turma":"PET-3AFASE","aulasSemana":4},{"codigo":"PET-4AFASE-RESISTENCIADOSMATERIAIS","nome":"Resistência dos Materiais","turma":"PET-4AFASE","aulasSemana":4},{"codigo":"PET-4AFASE-EQUACOESDIFERENCIAIS","nome":"Equações Diferenciais","turma":"PET-4AFASE","aulasSemana":4},{"codigo":"PET-4AFASE-FISICAIII","nome":"Física III","turma":"PET-4AFASE","aulasSemana":4},{"codigo":"PET-4AFASE-INTRODUCAOAFISICOQUIMICA","nome":"Introdução à Físico-Química","turma":"PET-4AFASE","aulasSemana":2},{"codigo":"PET-4AFASE-FISICAEXPERIMENTALIII","nome":"Física Experimental III","turma":"PET-4AFASE","aulasSemana":4},{"codigo":"PET-4AFASE-GEOLOGIADOPETROLEO","nome":"Geologia do Petróleo","turma":"PET-4AFASE","aulasSemana":5},{"codigo":"PET-4AFASE-METODOSNUMERICOSI","nome":"Métodos Numéricos I","turma":"PET-4AFASE","aulasSemana":4},{"codigo":"PET-4AFASE-GESTAOELIDERANCA","nome":"Gestão e Liderança","turma":"PET-4AFASE","aulasSemana":2},{"codigo":"PET-5AFASE-GEOMECANICA","nome":"Geomecânica","turma":"PET-5AFASE","aulasSemana":4},{"codigo":"PET-5AFASE-METODOSNUMERICOSII","nome":"Métodos Numéricos II","turma":"PET-5AFASE","aulasSemana":4},{"codigo":"PET-5AFASE-GEOFISICA","nome":"Geofísica","turma":"PET-5AFASE","aulasSemana":4},{"codigo":"PET-5AFASE-MECANICADOSFLUIDOS","nome":"Mecânica dos Fluidos","turma":"PET-5AFASE","aulasSemana":4},{"codigo":"PET-5AFASE-TERMODINAMICAAPLICADAAOPETROLEO","nome":"Termodinâmica Aplicada ao Petróleo","turma":"PET-5AFASE","aulasSemana":4},{"codigo":"PET-5AFASE-INSTRUMENTACAOEMEDICAODEPETROLEO","nome":"Instrumentação e Medição de Petróleo","turma":"PET-5AFASE","aulasSemana":2},{"codigo":"PET-5AFASE-FUNDAMENTOSEMENERGIASRENOVAVEIS","nome":"Fundamentos em Energias Renováveis","turma":"PET-5AFASE","aulasSemana":2},{"codigo":"PET-6AFASE-TRANSFERENCIADECALOR","nome":"Transferência de Calor","turma":"PET-6AFASE","aulasSemana":4},{"codigo":"PET-6AFASE-ENGENHARIADEPOCOI","nome":"Engenharia de Poço I","turma":"PET-6AFASE","aulasSemana":4},{"codigo":"PET-6AFASE-MAQUINASDEFLUXO","nome":"Máquinas de Fluxo","turma":"PET-6AFASE","aulasSemana":4},{"codigo":"PET-6AFASE-ESCOAMENTOMULTIFASICO","nome":"Escoamento Multifásico","turma":"PET-6AFASE","aulasSemana":4},{"codigo":"PET-6AFASE-QUIMICAORGANICAII","nome":"Química Orgânica II","turma":"PET-6AFASE","aulasSemana":2},{"codigo":"PET-6AFASE-ESTUDOGEOLOGICODECAMPO","nome":"Estudo Geológico de Campo","turma":"PET-6AFASE","aulasSemana":4},{"codigo":"PET-6AFASE-ENGENHARIADERESERVATORIOI","nome":"Engenharia de Reservatório I","turma":"PET-6AFASE","aulasSemana":4},{"codigo":"PET-6AFASE-ENGENHARIASUBMARINA","nome":"Engenharia Submarina","turma":"PET-6AFASE","aulasSemana":2},{"codigo":"PET-7AFASE-ENGENHARIADEPOCOII","nome":"Engenharia de Poço II","turma":"PET-7AFASE","aulasSemana":4},{"codigo":"PET-7AFASE-ENGENHARIADERESERVATORIOII","nome":"Engenharia de Reservatório II","turma":"PET-7AFASE","aulasSemana":4},{"codigo":"PET-7AFASE-SISTEMASDECONTROLEEAUTOMACAO","nome":"Sistemas de Controle e Automação","turma":"PET-7AFASE","aulasSemana":4},{"codigo":"PET-7AFASE-PROJETOINTEGRADOI","nome":"Projeto Integrado I","turma":"PET-7AFASE","aulasSemana":2},{"codigo":"PET-7AFASE-GESTAODEPROJETOS","nome":"Gestão de Projetos","turma":"PET-7AFASE","aulasSemana":4},{"codigo":"PET-7AFASE-ECONOMIADOPETROLEO","nome":"Economia do Petróleo","turma":"PET-7AFASE","aulasSemana":2},{"codigo":"PET-7AFASE-TRANSFERENCIADEMASSA","nome":"Transferência de Massa","turma":"PET-7AFASE","aulasSemana":2},{"codigo":"PET-7AFASE-METODOSDEELEVACAO","nome":"Métodos de Elevação","turma":"PET-7AFASE","aulasSemana":4},{"codigo":"PET-8AFASE-ENGENHARIADEPOCOIII","nome":"Engenharia de Poço III","turma":"PET-8AFASE","aulasSemana":4},{"codigo":"PET-8AFASE-LOGISTICAINTEGRADA","nome":"Logística Integrada","turma":"PET-8AFASE","aulasSemana":2},{"codigo":"PET-8AFASE-GARANTIADEESCOAMENTO","nome":"Garantia de Escoamento","turma":"PET-8AFASE","aulasSemana":4},{"codigo":"PET-8AFASE-PROJETOINTEGRADOII","nome":"Projeto Integrado II","turma":"PET-8AFASE","aulasSemana":4},{"codigo":"PET-8AFASE-SIMULACAODERESERVATORIOS","nome":"Simulação de Reservatórios","turma":"PET-8AFASE","aulasSemana":4},{"codigo":"PET-8AFASE-PROCESSAMENTOPRIMARIODEPETROLEO","nome":"Processamento Primário de Petróleo","turma":"PET-8AFASE","aulasSemana":2},{"codigo":"PET-8AFASE-TESTEDEPOCOS","nome":"Teste de Poços","turma":"PET-8AFASE","aulasSemana":4},{"codigo":"PET-8AFASE-IMPACTOSAMBIENTAISDAINDUSTRIADEOLEOEGAS","nome":"Impactos Ambientais da Indústria de Óleo e Gás","turma":"PET-8AFASE","aulasSemana":2},{"codigo":"PET-9AFASE-AVALIACAODEPROJETOSDEOLEOEGAS","nome":"Avaliação de Projetos de Óleo e Gás","turma":"PET-9AFASE","aulasSemana":4},{"codigo":"PET-9AFASE-SAUDEESEGURANCANOTRABALHO","nome":"Saúde e Segurança no Trabalho","turma":"PET-9AFASE","aulasSemana":4},{"codigo":"PET-9AFASE-PROCESSAMENTODEOLEOEGAS","nome":"Processamento de Óleo e Gás","turma":"PET-9AFASE","aulasSemana":2},{"codigo":"PET-9AFASE-GERENCIAMENTODERISCOSAMBIENTAIS","nome":"Gerenciamento de Riscos Ambientais","turma":"PET-9AFASE","aulasSemana":2},{"codigo":"PET-9AFASE-PROJETOINTEGRADOIII","nome":"Projeto Integrado III","turma":"PET-9AFASE","aulasSemana":4},{"codigo":"PET-9AFASE-FUNDAMENTOSEMRECUPERACAOAVANCADA","nome":"Fundamentos em Recuperação Avançada","turma":"PET-9AFASE","aulasSemana":2},{"codigo":"PET-OPTATIVAS-LINGUAGEMDEPROGRAMACAOII","nome":"Linguagem de Programação II","turma":"PET-OPTATIVAS","aulasSemana":2},{"codigo":"PET-OPTATIVAS-METODOLOGIACIENTIFICAOPTATIVA","nome":"Metodologia Científica - Optativa","turma":"PET-OPTATIVAS","aulasSemana":2},{"codigo":"PET-OPTATIVAS-FISICAIV","nome":"Física IV","turma":"PET-OPTATIVAS","aulasSemana":2},{"codigo":"PET-OPTATIVAS-POLIMEROSAPLICADOSAENGENHARIAOPTATIVA","nome":"Polímeros Aplicados à Engenharia - Optativa","turma":"PET-OPTATIVAS","aulasSemana":2},{"codigo":"PET-OPTATIVAS-SISTTERMPENGENHARIADEPETROLEOOPTATIVA","nome":"Sist. Térm. p/ Engenharia de Petróleo - Optativa","turma":"PET-OPTATIVAS","aulasSemana":2},{"codigo":"PET-OPTATIVAS-INTELIGENCIAARTIFICIALAPLICADAAENGDEPETR","nome":"Inteligência Artificial Aplicada à Eng. de Petróleo","turma":"PET-OPTATIVAS","aulasSemana":2}],"atribuicoes":[{"codigo":"PET-001","disciplina":"PET-1AFASE-QUIMICAGERAL","professores":["P0001"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-002","disciplina":"PET-1AFASE-INTRODUCAOAENGENHARIADEPETROLEO","professores":["P0002"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-003","disciplina":"PET-1AFASE-QUIMICAEXPERIMENTALI","professores":["P0001"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-004","disciplina":"PET-1AFASE-GEOMETRIAANALITICA","professores":["P0003"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-005","disciplina":"PET-1AFASE-CALCULODIFERENCIALEINTEGRALI","professores":["P0004"],"aulasSemana":6,"blocoAulas":2},{"codigo":"PET-006","disciplina":"PET-1AFASE-DESENHOTECNICO","professores":["P0005"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-007","disciplina":"PET-2AFASE-ALGEBRALINEAR","professores":["P0003"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-008","disciplina":"PET-2AFASE-QUIMICAEXPERIMENTALII","professores":["P0006"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-009","disciplina":"PET-2AFASE-CALCULODIFERENCIALEINTEGRALII","professores":["P0004"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-010","disciplina":"PET-2AFASE-CIENCIADOSMATERIAIS","professores":["P0007"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-011","disciplina":"PET-2AFASE-FISICAI","professores":["P0008"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-012","disciplina":"PET-2AFASE-QUIMICAEXPERIMENTALII","professores":["P0001"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-013","disciplina":"PET-2AFASE-FISICAEXPERIMENTALI","professores":["P0006"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-014","disciplina":"PET-2AFASE-ESTATISTICADESCRITIVA","professores":["P0009"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-015","disciplina":"PET-2AFASE-QUIMICAORGANICAI","professores":["P0007"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-016","disciplina":"PET-2AFASEEXTRA-QUIMICAEXPERIMENTALIIEXTRA","professores":["P0006"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-017","disciplina":"PET-2AFASEEXTRA-QUIMICAEXPERIMENTALIIEXTRA","professores":["P0001"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-018","disciplina":"PET-3AFASE-LINGUAGEMDEPROGRAMACAOI","professores":["P0010"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-019","disciplina":"PET-3AFASE-ESTATICA","professores":["P0011"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-020","disciplina":"PET-3AFASE-GEOLOGIAGERAL","professores":["P0012"],"aulasSemana":3,"blocoAulas":1},{"codigo":"PET-021","disciplina":"PET-3AFASE-FISICAII","professores":["P0011","P0006"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-022","disciplina":"PET-3AFASE-CALCULODIFERENCIALEINTEGRALIII","professores":["P0004"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-023","disciplina":"PET-3AFASE-FISICAEXPERIMENTALII","professores":["P0006"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-024","disciplina":"PET-4AFASE-RESISTENCIADOSMATERIAIS","professores":["P0011"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-025","disciplina":"PET-4AFASE-EQUACOESDIFERENCIAIS","professores":["P0013"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-026","disciplina":"PET-4AFASE-FISICAIII","professores":["P0008"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-027","disciplina":"PET-4AFASE-INTRODUCAOAFISICOQUIMICA","professores":["P0007"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-028","disciplina":"PET-4AFASE-FISICAEXPERIMENTALIII","professores":["P0006"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-029","disciplina":"PET-4AFASE-GEOLOGIADOPETROLEO","professores":["P0012"],"aulasSemana":5,"blocoAulas":1},{"codigo":"PET-030","disciplina":"PET-4AFASE-METODOSNUMERICOSI","professores":["P0010"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-031","disciplina":"PET-4AFASE-GESTAOELIDERANCA","professores":["P0014"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-032","disciplina":"PET-5AFASE-GEOMECANICA","professores":["P0015","P0011"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-033","disciplina":"PET-5AFASE-METODOSNUMERICOSII","professores":["P0010"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-034","disciplina":"PET-5AFASE-GEOFISICA","professores":["P0012"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-035","disciplina":"PET-5AFASE-MECANICADOSFLUIDOS","professores":["P0016","P0008"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-036","disciplina":"PET-5AFASE-TERMODINAMICAAPLICADAAOPETROLEO","professores":["P0017"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-037","disciplina":"PET-5AFASE-INSTRUMENTACAOEMEDICAODEPETROLEO","professores":["P0002"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-038","disciplina":"PET-5AFASE-FUNDAMENTOSEMENERGIASRENOVAVEIS","professores":["P0018"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-039","disciplina":"PET-6AFASE-TRANSFERENCIADECALOR","professores":["P0019"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-040","disciplina":"PET-6AFASE-ENGENHARIADEPOCOI","professores":["P0007"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-041","disciplina":"PET-6AFASE-MAQUINASDEFLUXO","professores":["P0016"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-042","disciplina":"PET-6AFASE-ESCOAMENTOMULTIFASICO","professores":["P0016"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-043","disciplina":"PET-6AFASE-QUIMICAORGANICAII","professores":["P0001"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-044","disciplina":"PET-6AFASE-ESTUDOGEOLOGICODECAMPO","professores":["P0015"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-045","disciplina":"PET-6AFASE-ENGENHARIADERESERVATORIOI","professores":["P0017"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-046","disciplina":"PET-6AFASE-ENGENHARIASUBMARINA","professores":["P0020"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-047","disciplina":"PET-7AFASE-ENGENHARIADEPOCOII","professores":["P0021"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-048","disciplina":"PET-7AFASE-ENGENHARIADERESERVATORIOII","professores":["P0019"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-049","disciplina":"PET-7AFASE-SISTEMASDECONTROLEEAUTOMACAO","professores":["P0022"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-050","disciplina":"PET-7AFASE-PROJETOINTEGRADOI","professores":["P0021"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-051","disciplina":"PET-7AFASE-GESTAODEPROJETOS","professores":["P0020"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-052","disciplina":"PET-7AFASE-ECONOMIADOPETROLEO","professores":["P0009"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-053","disciplina":"PET-7AFASE-TRANSFERENCIADEMASSA","professores":["P0005"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-054","disciplina":"PET-7AFASE-METODOSDEELEVACAO","professores":["P0020"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-055","disciplina":"PET-8AFASE-ENGENHARIADEPOCOIII","professores":["P0021"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-056","disciplina":"PET-8AFASE-LOGISTICAINTEGRADA","professores":["P0023"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-057","disciplina":"PET-8AFASE-GARANTIADEESCOAMENTO","professores":["P0024"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-058","disciplina":"PET-8AFASE-PROJETOINTEGRADOII","professores":["P0002"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-059","disciplina":"PET-8AFASE-SIMULACAODERESERVATORIOS","professores":["P0017"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-060","disciplina":"PET-8AFASE-PROCESSAMENTOPRIMARIODEPETROLEO","professores":["P0020"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-061","disciplina":"PET-8AFASE-TESTEDEPOCOS","professores":["P0025"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-062","disciplina":"PET-8AFASE-IMPACTOSAMBIENTAISDAINDUSTRIADEOLEOEGAS","professores":["P0018"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-063","disciplina":"PET-9AFASE-AVALIACAODEPROJETOSDEOLEOEGAS","professores":["P0021","P0007"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-064","disciplina":"PET-9AFASE-SAUDEESEGURANCANOTRABALHO","professores":["P0018"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-065","disciplina":"PET-9AFASE-PROCESSAMENTODEOLEOEGAS","professores":["P0007"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-066","disciplina":"PET-9AFASE-GERENCIAMENTODERISCOSAMBIENTAIS","professores":["P0018"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-067","disciplina":"PET-9AFASE-PROJETOINTEGRADOIII","professores":["P0026"],"aulasSemana":4,"blocoAulas":2},{"codigo":"PET-068","disciplina":"PET-9AFASE-FUNDAMENTOSEMRECUPERACAOAVANCADA","professores":["P0002"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-069","disciplina":"PET-OPTATIVAS-LINGUAGEMDEPROGRAMACAOII","professores":["P0010"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-070","disciplina":"PET-OPTATIVAS-METODOLOGIACIENTIFICAOPTATIVA","professores":["P0010"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-071","disciplina":"PET-OPTATIVAS-FISICAIV","professores":["P0008"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-072","disciplina":"PET-OPTATIVAS-POLIMEROSAPLICADOSAENGENHARIAOPTATIVA","professores":["P0007"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-073","disciplina":"PET-OPTATIVAS-SISTTERMPENGENHARIADEPETROLEOOPTATIVA","professores":["P0016"],"aulasSemana":2,"blocoAulas":2},{"codigo":"PET-074","disciplina":"PET-OPTATIVAS-INTELIGENCIAARTIFICIALAPLICADAAENGDEPETR","professores":["P0007"],"aulasSemana":2,"blocoAulas":2}],"indisponibilidades":[],"parcial":false}', 'd4f808d891392398e471f1a15396e56d43fcdc455f7ca916f16ddbb99bb360cc', 'RECEBIDO', 74, 'UDESC-CESFI-DEMO' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM hprof_uni_integracaolote WHERE id_instituicao = @id_inst AND codigo_envio = 'UDESC-PET-2026-2-001');
INSERT INTO hprof_uni_integracaoauditoria (id_lote, acao, usuario, tipo_ator, detalhe)
SELECT l.id, 'RECEBIDO', 'UDESC-CESFI-DEMO', 'INTEGRACAO', 'Carga de demonstração (Flyway sql-demo)'
  FROM hprof_uni_integracaolote l WHERE l.id_instituicao = @id_inst AND l.codigo_envio = 'UDESC-PET-2026-2-001'
   AND NOT EXISTS (SELECT 1 FROM hprof_uni_integracaoauditoria a WHERE a.id_lote = l.id AND a.acao = 'RECEBIDO');

INSERT INTO hprof_uni_integracaolote (id_instituicao, id_cliente, origem, codigo_envio, tipo, ano_letivo, periodo,
                                      conteudo_json, hash_conteudo, status, total_itens, enviado_por)
SELECT @id_inst, @id_cliente, 'API', 'UDESC-ADM-2026-2-001', 'CURSO', 2026, 'Manha',
        '{"codigoEnvio":"UDESC-ADM-2026-2-001","anoLetivo":2026,"periodo":"Manha","curso":{"codigo":"ADM","nome":"Administração Pública (simulação)","turno":"Vespertino e noturno","dias":["SEG","TER","QUA","QUI","SEX"]},"horariosAula":[{"aula":1,"inicio":"15:20","fim":"16:10"},{"aula":2,"inicio":"16:10","fim":"17:00"},{"aula":3,"inicio":"17:00","fim":"17:50"},{"aula":4,"inicio":"17:50","fim":"18:40"},{"aula":5,"inicio":"19:00","fim":"19:50"},{"aula":6,"inicio":"19:50","fim":"20:40"},{"aula":7,"inicio":"20:50","fim":"21:40"},{"aula":8,"inicio":"21:40","fim":"22:30"}],"intervalos":[{"nome":"Jantar","inicio":"18:40","fim":"19:00"},{"nome":"Intervalo","inicio":"20:40","fim":"20:50"}],"turmas":[{"codigo":"ADM-1AFASE","nome":"1ª Fase"},{"codigo":"ADM-2AFASE","nome":"2ª Fase"},{"codigo":"ADM-3AFASE","nome":"3ª Fase"},{"codigo":"ADM-4AFASE","nome":"4ª Fase"}],"disciplinas":[{"codigo":"ADM-1AFASE-TEORIAGERALDAADMINISTRACAO","nome":"Teoria Geral da Administração","turma":"ADM-1AFASE","aulasSemana":4},{"codigo":"ADM-1AFASE-MATEMATICABASICA","nome":"Matemática Básica","turma":"ADM-1AFASE","aulasSemana":4},{"codigo":"ADM-1AFASE-INTRODUCAOAECONOMIA","nome":"Introdução à Economia","turma":"ADM-1AFASE","aulasSemana":4},{"codigo":"ADM-1AFASE-CIENCIAPOLITICA","nome":"Ciência Política","turma":"ADM-1AFASE","aulasSemana":4},{"codigo":"ADM-1AFASE-METODOLOGIACIENTIFICA","nome":"Metodologia Científica","turma":"ADM-1AFASE","aulasSemana":2},{"codigo":"ADM-2AFASE-ESTATISTICAAPLICADA","nome":"Estatística Aplicada","turma":"ADM-2AFASE","aulasSemana":4},{"codigo":"ADM-2AFASE-DIREITOCONSTITUCIONAL","nome":"Direito Constitucional","turma":"ADM-2AFASE","aulasSemana":4},{"codigo":"ADM-2AFASE-CONTABILIDADEGERAL","nome":"Contabilidade Geral","turma":"ADM-2AFASE","aulasSemana":4},{"codigo":"ADM-2AFASE-MATEMATICAAPLICADAAGESTAO","nome":"Matemática Aplicada à Gestão","turma":"ADM-2AFASE","aulasSemana":4},{"codigo":"ADM-2AFASE-RACIOCINIOLOGICO","nome":"Raciocínio Lógico","turma":"ADM-2AFASE","aulasSemana":2},{"codigo":"ADM-3AFASE-MATEMATICAFINANCEIRA","nome":"Matemática Financeira","turma":"ADM-3AFASE","aulasSemana":4},{"codigo":"ADM-3AFASE-METODOSQUANTITATIVOS","nome":"Métodos Quantitativos","turma":"ADM-3AFASE","aulasSemana":4},{"codigo":"ADM-3AFASE-LIDERANCANOSETORPUBLICO","nome":"Liderança no Setor Público","turma":"ADM-3AFASE","aulasSemana":4},{"codigo":"ADM-3AFASE-DIREITOADMINISTRATIVO","nome":"Direito Administrativo","turma":"ADM-3AFASE","aulasSemana":4},{"codigo":"ADM-3AFASE-CONTABILIDADEPUBLICA","nome":"Contabilidade Pública","turma":"ADM-3AFASE","aulasSemana":2},{"codigo":"ADM-4AFASE-GESTAODEPROJETOSPUBLICOS","nome":"Gestão de Projetos Públicos","turma":"ADM-4AFASE","aulasSemana":4},{"codigo":"ADM-4AFASE-POLITICASPUBLICAS","nome":"Políticas Públicas","turma":"ADM-4AFASE","aulasSemana":4},{"codigo":"ADM-4AFASE-FINANCASPUBLICAS","nome":"Finanças Públicas","turma":"ADM-4AFASE","aulasSemana":4},{"codigo":"ADM-4AFASE-ANALISEDEDADOSPUBLICOS","nome":"Análise de Dados Públicos","turma":"ADM-4AFASE","aulasSemana":4},{"codigo":"ADM-4AFASE-ESTATISTICAPARAPOLITICASPUBLICAS","nome":"Estatística para Políticas Públicas","turma":"ADM-4AFASE","aulasSemana":4}],"atribuicoes":[{"codigo":"ADM-001","disciplina":"ADM-1AFASE-TEORIAGERALDAADMINISTRACAO","professores":["P0014"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-002","disciplina":"ADM-1AFASE-MATEMATICABASICA","professores":["P0004"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-003","disciplina":"ADM-1AFASE-INTRODUCAOAECONOMIA","professores":["P0009"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-004","disciplina":"ADM-1AFASE-CIENCIAPOLITICA","professores":["P0027"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-005","disciplina":"ADM-1AFASE-METODOLOGIACIENTIFICA","professores":["P0010"],"aulasSemana":2,"blocoAulas":2},{"codigo":"ADM-006","disciplina":"ADM-2AFASE-ESTATISTICAAPLICADA","professores":["P0009"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-007","disciplina":"ADM-2AFASE-DIREITOCONSTITUCIONAL","professores":["P0028"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-008","disciplina":"ADM-2AFASE-CONTABILIDADEGERAL","professores":["P0029"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-009","disciplina":"ADM-2AFASE-MATEMATICAAPLICADAAGESTAO","professores":["P0004"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-010","disciplina":"ADM-2AFASE-RACIOCINIOLOGICO","professores":["P0004"],"aulasSemana":2,"blocoAulas":2},{"codigo":"ADM-011","disciplina":"ADM-3AFASE-MATEMATICAFINANCEIRA","professores":["P0004"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-012","disciplina":"ADM-3AFASE-METODOSQUANTITATIVOS","professores":["P0004"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-013","disciplina":"ADM-3AFASE-LIDERANCANOSETORPUBLICO","professores":["P0014"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-014","disciplina":"ADM-3AFASE-DIREITOADMINISTRATIVO","professores":["P0028"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-015","disciplina":"ADM-3AFASE-CONTABILIDADEPUBLICA","professores":["P0029"],"aulasSemana":2,"blocoAulas":2},{"codigo":"ADM-016","disciplina":"ADM-4AFASE-GESTAODEPROJETOSPUBLICOS","professores":["P0020"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-017","disciplina":"ADM-4AFASE-POLITICASPUBLICAS","professores":["P0027"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-018","disciplina":"ADM-4AFASE-FINANCASPUBLICAS","professores":["P0030"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-019","disciplina":"ADM-4AFASE-ANALISEDEDADOSPUBLICOS","professores":["P0004"],"aulasSemana":4,"blocoAulas":2},{"codigo":"ADM-020","disciplina":"ADM-4AFASE-ESTATISTICAPARAPOLITICASPUBLICAS","professores":["P0004"],"aulasSemana":4,"blocoAulas":2}],"indisponibilidades":[{"professor":"P0028","dia":"SEG","aula":1,"sigla":"BL","opcao":1},{"professor":"P0028","dia":"SEG","aula":2,"sigla":"BL","opcao":1},{"professor":"P0028","dia":"SEG","aula":3,"sigla":"BL","opcao":1},{"professor":"P0028","dia":"SEG","aula":4,"sigla":"BL","opcao":1},{"professor":"P0028","dia":"SEG","aula":5,"sigla":"BL","opcao":1},{"professor":"P0028","dia":"SEG","aula":6,"sigla":"BL","opcao":1},{"professor":"P0028","dia":"SEG","aula":7,"sigla":"BL","opcao":1},{"professor":"P0028","dia":"SEG","aula":8,"sigla":"BL","opcao":1},{"professor":"P0028","dia":"TER","aula":1,"sigla":"BL","opcao":1},{"professor":"P0028","dia":"TER","aula":2,"sigla":"BL","opcao":1},{"professor":"P0028","dia":"TER","aula":3,"sigla":"BL","opcao":1},{"professor":"P0028","dia":"TER","aula":4,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEG","aula":1,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEG","aula":2,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEG","aula":3,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEG","aula":4,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEG","aula":5,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEG","aula":6,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEG","aula":7,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEG","aula":8,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"TER","aula":1,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"TER","aula":2,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"TER","aula":3,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"TER","aula":4,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"TER","aula":5,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"TER","aula":6,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"TER","aula":7,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"TER","aula":8,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEX","aula":1,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEX","aula":2,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEX","aula":3,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEX","aula":4,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEX","aula":5,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEX","aula":6,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEX","aula":7,"sigla":"BL","opcao":1},{"professor":"P0030","dia":"SEX","aula":8,"sigla":"BL","opcao":1},{"professor":"P0029","dia":"SEX","aula":1,"sigla":"BL","opcao":1},{"professor":"P0029","dia":"SEX","aula":2,"sigla":"BL","opcao":1},{"professor":"P0029","dia":"SEX","aula":3,"sigla":"BL","opcao":1},{"professor":"P0029","dia":"SEX","aula":4,"sigla":"BL","opcao":1},{"professor":"P0029","dia":"SEX","aula":5,"sigla":"BL","opcao":1},{"professor":"P0029","dia":"SEX","aula":6,"sigla":"BL","opcao":1},{"professor":"P0029","dia":"SEX","aula":7,"sigla":"BL","opcao":1},{"professor":"P0029","dia":"SEX","aula":8,"sigla":"BL","opcao":1}],"turnoPorTurma":{"ADM-1AFASE":"Noturno","ADM-2AFASE":"Noturno","ADM-3AFASE":"Vespertino-noturno","ADM-4AFASE":"Vespertino-noturno"},"parcial":false}', 'da92a0593f041e165053084c1faeb833bfed3e236625d0f3d66dfe0179033350', 'RECEBIDO', 20, 'UDESC-CESFI-DEMO' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM hprof_uni_integracaolote WHERE id_instituicao = @id_inst AND codigo_envio = 'UDESC-ADM-2026-2-001');
INSERT INTO hprof_uni_integracaoauditoria (id_lote, acao, usuario, tipo_ator, detalhe)
SELECT l.id, 'RECEBIDO', 'UDESC-CESFI-DEMO', 'INTEGRACAO', 'Carga de demonstração (Flyway sql-demo)'
  FROM hprof_uni_integracaolote l WHERE l.id_instituicao = @id_inst AND l.codigo_envio = 'UDESC-ADM-2026-2-001'
   AND NOT EXISTS (SELECT 1 FROM hprof_uni_integracaoauditoria a WHERE a.id_lote = l.id AND a.acao = 'RECEBIDO');


SELECT table_name AS tabela_criada FROM information_schema.tables WHERE table_schema = 'droplexdb' AND table_name LIKE 'hprof\_uni\_%' ORDER BY table_name;
SELECT codigo_envio, status FROM hprof_uni_integracaolote;
