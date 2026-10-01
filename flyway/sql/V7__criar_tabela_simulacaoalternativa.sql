-- ============================================================================
-- V7 — hprof_simulacaoalternativa
-- Persiste cada alternativa antes da escolha do gestor. As alternativas são
-- imutáveis; a adoção apenas aponta para a versão filha criada posteriormente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_simulacaoalternativa (
    id                      BIGINT          NOT NULL AUTO_INCREMENT,
    id_simulacao            BIGINT          NOT NULL,
    id_cenario              BIGINT          NOT NULL,
    codigo_alternativa      VARCHAR(20)     NOT NULL COMMENT 'ALT_A, ALT_B, ALT_C',
    titulo                  VARCHAR(200)    NOT NULL,
    estrategia              VARCHAR(500)    NULL,

    snapshot_json           JSON            NOT NULL,
    snapshot_hash           VARCHAR(64)     NOT NULL,
    concessoes_json         JSON            NULL,
    movimentacoes_json      JSON            NULL,

    total_aulas_distribuidas INT            NOT NULL DEFAULT 0,
    total_aulas_pendentes    INT            NOT NULL DEFAULT 0,
    total_movimentacoes      INT            NOT NULL DEFAULT 0,
    total_dobradinhas        INT            NOT NULL DEFAULT 0,

    status                  VARCHAR(20)     NOT NULL DEFAULT 'CANDIDATA'
                                            COMMENT 'CANDIDATA, ADOTADA, DESCARTADA',
    id_versao_adotada       BIGINT          NULL,
    criado_em               DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    adotado_em              DATETIME(3)     NULL,
    adotado_por             VARCHAR(100)    NULL,

    PRIMARY KEY (id),
    UNIQUE KEY uk_alternativa_simulacao_codigo (id_simulacao, codigo_alternativa),
    INDEX idx_alternativa_cenario (id_cenario),
    INDEX idx_alternativa_status (id_simulacao, status),
    INDEX idx_alternativa_versao_adotada (id_versao_adotada),

    CONSTRAINT fk_alternativa_simulacao
        FOREIGN KEY (id_simulacao) REFERENCES hprof_simulacao (id)
        ON DELETE RESTRICT ON UPDATE CASCADE,

    CONSTRAINT fk_alternativa_cenario
        FOREIGN KEY (id_cenario) REFERENCES hprof_cenario (id)
        ON DELETE RESTRICT ON UPDATE CASCADE,

    CONSTRAINT fk_alternativa_versao_adotada
        FOREIGN KEY (id_versao_adotada) REFERENCES hprof_cenarioversao (id)
        ON DELETE SET NULL ON UPDATE CASCADE
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Alternativas imutáveis geradas por uma simulação de fechamento';
