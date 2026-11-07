-- ============================================================================
-- V11 — hprof_uni_pessoaprofessor
-- Módulo universidade — a pessoa do professor, acima dos cursos
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_pessoaprofessor (
    id              BIGINT       NOT NULL AUTO_INCREMENT,
    id_instituicao  BIGINT       NOT NULL,
    nome            VARCHAR(200) NOT NULL,
    nome_curto      VARCHAR(40)  NOT NULL                COMMENT 'Nome usado no cadastro do motor (antes do hífen); único na instituição',
    documento       VARCHAR(40)  NOT NULL                COMMENT 'Matrícula ou CPF; identifica a pessoa (não usar o nome)',
    carga_maxima    INT          NOT NULL                COMMENT 'Aulas por semana contratadas (ex.: 40)',
    ativo           TINYINT(1)   NOT NULL DEFAULT 1,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_pessoa_documento (id_instituicao, documento),
    UNIQUE KEY uk_uni_pessoa_nomecurto (id_instituicao, nome_curto)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — a pessoa do professor, acima dos cursos';
