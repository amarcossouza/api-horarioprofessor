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
