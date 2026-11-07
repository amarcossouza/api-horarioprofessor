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
