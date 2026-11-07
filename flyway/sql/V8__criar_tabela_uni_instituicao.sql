-- ============================================================================
-- V8 — hprof_uni_instituicao
-- Módulo universidade — instituição (universidade/campus) que agrupa cursos
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_instituicao (
    id                  BIGINT       NOT NULL AUTO_INCREMENT,
    sigla               VARCHAR(40)  NOT NULL                COMMENT 'Ex.: UDESC-CESFI',
    nome                VARCHAR(200) NOT NULL,
    id_escola_principal INT          NOT NULL                COMMENT 'hprof_escolas.EscolaId usada no login da instituição',
    ativo               TINYINT(1)   NOT NULL DEFAULT 1,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_instituicao_sigla (sigla),
    UNIQUE KEY uk_uni_instituicao_escola (id_escola_principal)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — instituição (universidade/campus) que agrupa cursos';
