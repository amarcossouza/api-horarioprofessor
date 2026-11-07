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
