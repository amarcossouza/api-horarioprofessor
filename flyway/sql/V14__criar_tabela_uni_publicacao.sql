-- ============================================================================
-- V14 — hprof_uni_publicacao
-- Módulo universidade — grade salva escolhida como oficial do curso
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_publicacao (
    id            BIGINT       NOT NULL AUTO_INCREMENT,
    id_escola     INT          NOT NULL,
    ano_letivo    INT          NOT NULL,
    periodo       VARCHAR(20)  NOT NULL,
    id_planilha   INT          NOT NULL                COMMENT 'hprof_horariodistribuicaojson.Id',
    ativo         TINYINT(1)   NOT NULL DEFAULT 1      COMMENT 'Só a última publicação fica ativa',
    publicado_por VARCHAR(100) NOT NULL,
    publicado_em  DATETIME(3)  NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    total_aulas   INT          NOT NULL DEFAULT 0,
    PRIMARY KEY (id),
    INDEX idx_uni_publicacao_escola (id_escola, ano_letivo, periodo, ativo)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — grade salva escolhida como oficial do curso';
