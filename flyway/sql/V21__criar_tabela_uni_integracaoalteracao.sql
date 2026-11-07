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
