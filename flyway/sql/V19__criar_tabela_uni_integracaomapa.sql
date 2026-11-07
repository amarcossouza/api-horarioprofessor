-- ============================================================================
-- V19 — hprof_uni_integracaomapa
-- Módulo universidade — código da universidade ↔ id do Horário Professor
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_integracaomapa (
    id             BIGINT       NOT NULL AUTO_INCREMENT,
    id_instituicao BIGINT       NOT NULL,
    entidade       VARCHAR(30)  NOT NULL                COMMENT 'PESSOA, CURSO, TURMA, DISCIPLINA, ATRIBUICAO',
    codigo_externo VARCHAR(120) NOT NULL,
    tabela_interna VARCHAR(64)  NOT NULL,
    id_interno     BIGINT       NOT NULL,
    ativo          TINYINT(1)   NOT NULL DEFAULT 1      COMMENT '0 = deixou de vir no reenvio (desativado, não apagado)',
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_mapa (id_instituicao, entidade, codigo_externo)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — código da universidade ↔ id do Horário Professor';
