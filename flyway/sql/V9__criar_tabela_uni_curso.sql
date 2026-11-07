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
