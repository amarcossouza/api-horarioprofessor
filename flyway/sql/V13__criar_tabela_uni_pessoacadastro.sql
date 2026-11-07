-- ============================================================================
-- V13 — hprof_uni_pessoacadastro
-- Módulo universidade — liga a pessoa às linhas de hprof_cadastroprofessor
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_pessoacadastro (
    id                   BIGINT      NOT NULL AUTO_INCREMENT,
    id_pessoa            BIGINT      NOT NULL,
    id_cadastroprofessor INT         NOT NULL                COMMENT 'hprof_cadastroprofessor.ProfessorId (tabela existente, não alterada)',
    id_escola            INT         NOT NULL,
    papel                VARCHAR(12) NOT NULL DEFAULT 'PRINCIPAL' COMMENT 'PRINCIPAL ou APOIO (aula com dois professores)',
    ativo                TINYINT(1)  NOT NULL DEFAULT 1,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_pessoacadastro (id_cadastroprofessor, id_pessoa),
    INDEX idx_uni_pessoacadastro_pessoa (id_pessoa)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — liga a pessoa às linhas de hprof_cadastroprofessor';
