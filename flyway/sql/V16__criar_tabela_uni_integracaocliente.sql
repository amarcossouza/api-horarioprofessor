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
