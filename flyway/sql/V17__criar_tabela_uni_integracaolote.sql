-- ============================================================================
-- V17 — hprof_uni_integracaolote
-- Módulo universidade — cada envio, guardado como chegou
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_integracaolote (
    id              BIGINT       NOT NULL AUTO_INCREMENT,
    id_instituicao  BIGINT       NOT NULL,
    id_cliente      BIGINT       NULL                    COMMENT 'Nulo quando veio da planilha ou da tela',
    origem          VARCHAR(12)  NOT NULL DEFAULT 'API' COMMENT 'API, PLANILHA ou TELA',
    codigo_envio    VARCHAR(100) NOT NULL,
    tipo            VARCHAR(30)  NOT NULL                COMMENT 'PESSOAS ou CURSO',
    ano_letivo      INT          NOT NULL,
    periodo         VARCHAR(20)  NOT NULL,
    conteudo_json   JSON         NOT NULL                COMMENT 'Envio original, imutável',
    hash_conteudo   CHAR(64)     NOT NULL,
    status          VARCHAR(20)  NOT NULL DEFAULT 'RECEBIDO' COMMENT 'RECEBIDO, VALIDADO, COM_ERROS, REJEITADO, APLICADO',
    total_itens     INT          NOT NULL DEFAULT 0,
    total_erros     INT          NOT NULL DEFAULT 0,
    total_avisos    INT          NOT NULL DEFAULT 0,
    enviado_por     VARCHAR(100) NOT NULL,
    recebido_em     DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    recebido_de_ip  VARCHAR(45)  NULL,
    validado_em     DATETIME(3)  NULL,
    aprovado_por    VARCHAR(100) NULL,
    aprovado_em     DATETIME(3)  NULL,
    aplicado_em     DATETIME(3)  NULL,
    motivo_rejeicao VARCHAR(500) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_lote_envio (id_instituicao, codigo_envio),
    INDEX idx_uni_lote_status (id_instituicao, status)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — cada envio, guardado como chegou';
