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
