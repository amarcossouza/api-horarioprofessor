-- ============================================================================
-- V2 — hprof_cenarioversao
-- Versão imutável de um cenário. Contém o snapshot completo (JSON) com
-- todos os dados necessários para reconstruir a grade sem depender do
-- estado atual dos cadastros.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_cenarioversao (
    id              BIGINT          NOT NULL AUTO_INCREMENT,
    id_cenario      BIGINT          NOT NULL                COMMENT 'FK para hprof_cenario.id',
    versao_pai_id   BIGINT          NULL                    COMMENT 'FK para versão de origem (NULL = primeira)',
    numero_versao   INT             NOT NULL DEFAULT 1      COMMENT 'Sequencial dentro do cenário',

    -- Snapshot completo (matrizes + cadastros + regras + parâmetros)
    snapshot_json   JSON            NOT NULL                COMMENT 'Fotografia imutável de toda a grade e dados',
    formato_versao  VARCHAR(20)     NOT NULL DEFAULT '1.0'  COMMENT 'Versão do schema do JSON',
    snapshot_hash   VARCHAR(64)     NULL                    COMMENT 'SHA-256 do snapshot para verificação de integridade',

    -- Resultado da validação no momento da gravação
    total_aulas_distribuidas   INT  NOT NULL DEFAULT 0,
    total_aulas_pendentes      INT  NOT NULL DEFAULT 0,
    total_conflitos            INT  NOT NULL DEFAULT 0,
    is_fechamento_completo     TINYINT(1) NOT NULL DEFAULT 0 COMMENT '1 = grade fechada sem pendências',

    -- Autoria e rastreabilidade
    criado_por      VARCHAR(100)    NOT NULL,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    observacao      TEXT            NULL                    COMMENT 'Nota do usuário ao salvar',

    PRIMARY KEY (id),
    INDEX idx_versao_cenario    (id_cenario, numero_versao),
    INDEX idx_versao_pai        (versao_pai_id),
    INDEX idx_versao_fechamento (is_fechamento_completo),

    CONSTRAINT fk_versao_cenario
        FOREIGN KEY (id_cenario) REFERENCES hprof_cenario (id)
        ON DELETE RESTRICT ON UPDATE CASCADE,

    CONSTRAINT fk_versao_pai
        FOREIGN KEY (versao_pai_id) REFERENCES hprof_cenarioversao (id)
        ON DELETE SET NULL ON UPDATE CASCADE
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Versão imutável de cenário — snapshot, indicadores e autoria';
