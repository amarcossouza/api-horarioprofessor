-- ============================================================================
-- V6 — hprof_publicacao
-- Histórico imutável da versão adotada como grade oficial em cada período.
-- Revogar encerra a vigência; nenhuma publicação anterior é apagada.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_publicacao (
    id                      BIGINT          NOT NULL AUTO_INCREMENT,
    id_cenario              BIGINT          NOT NULL,
    id_cenarioversao        BIGINT          NOT NULL,
    publicacao_anterior_id  BIGINT          NULL,

    status                  VARCHAR(20)     NOT NULL DEFAULT 'VIGENTE'
                                            COMMENT 'VIGENTE, REVOGADA, SUBSTITUIDA',
    vigencia_inicio         DATE            NOT NULL,
    vigencia_fim            DATE            NULL,
    observacao              TEXT            NULL,

    publicado_por           VARCHAR(100)    NOT NULL,
    publicado_em            DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    revogado_por            VARCHAR(100)    NULL,
    revogado_em             DATETIME(3)     NULL,
    motivo_revogacao        TEXT            NULL,

    PRIMARY KEY (id),
    INDEX idx_publicacao_cenario_status (id_cenario, status),
    INDEX idx_publicacao_versao (id_cenarioversao),
    INDEX idx_publicacao_vigencia (id_cenario, vigencia_inicio, vigencia_fim),
    INDEX idx_publicacao_anterior (publicacao_anterior_id),

    CONSTRAINT fk_publicacao_cenario
        FOREIGN KEY (id_cenario) REFERENCES hprof_cenario (id)
        ON DELETE RESTRICT ON UPDATE CASCADE,

    CONSTRAINT fk_publicacao_versao
        FOREIGN KEY (id_cenarioversao) REFERENCES hprof_cenarioversao (id)
        ON DELETE RESTRICT ON UPDATE CASCADE,

    CONSTRAINT fk_publicacao_anterior
        FOREIGN KEY (publicacao_anterior_id) REFERENCES hprof_publicacao (id)
        ON DELETE SET NULL ON UPDATE CASCADE
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Histórico de publicação e vigência da grade oficial';
