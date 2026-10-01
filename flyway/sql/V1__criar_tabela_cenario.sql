-- ============================================================================
-- V1 — hprof_cenario
-- Identidade do cenário: contexto (escola/ano/período), título e controle
-- de concorrência otimista (revisao).
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_cenario (
    id              BIGINT          NOT NULL AUTO_INCREMENT,
    id_escola       INT             NOT NULL                COMMENT 'FK lógica para hprof_escolas.id',
    ano_letivo      INT             NOT NULL,
    periodo         VARCHAR(20)     NOT NULL                COMMENT 'Manha, Tarde, Noite, Integral',
    titulo          VARCHAR(200)    NOT NULL                COMMENT 'Nome dado pelo diretor/gestor',
    descricao       TEXT            NULL                    COMMENT 'Observações livres',
    status          VARCHAR(20)     NOT NULL DEFAULT 'RASCUNHO'
                                                            COMMENT 'RASCUNHO, ATIVO, ARQUIVADO',
    criado_por      VARCHAR(100)    NOT NULL                COMMENT 'Usuário que criou',
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3)
                                             ON UPDATE CURRENT_TIMESTAMP(3),
    revisao         INT             NOT NULL DEFAULT 1      COMMENT 'Controle otimista de concorrência',

    PRIMARY KEY (id),
    INDEX idx_cenario_contexto (id_escola, ano_letivo, periodo),
    INDEX idx_cenario_status   (status)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Cenário de grade horária — identidade e contexto';
