-- ============================================================================
-- V10 — hprof_uni_turma
-- Módulo universidade — fase/turma com o nome completo usado pela universidade
-- Cria somente uma tabela nova do módulo universidade. Não altera nada existente.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_uni_turma (
    id              BIGINT       NOT NULL AUTO_INCREMENT,
    id_curso        BIGINT       NOT NULL,
    id_turmapadrao  INT          NULL                    COMMENT 'hprof_turmapadraoescola.TurmaId',
    codigo          VARCHAR(60)  NOT NULL                COMMENT 'Código na universidade',
    sigla           VARCHAR(20)  NOT NULL                COMMENT 'Nome curto gravado em hprof_turmapadraoescola.Turmas (ex.: 1F)',
    nome            VARCHAR(120) NOT NULL                COMMENT 'Nome exibido (ex.: 1ª Fase)',
    ordem           INT          NOT NULL DEFAULT 1,
    ativo           TINYINT(1)   NOT NULL DEFAULT 1,
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    criado_por      VARCHAR(100)    NOT NULL DEFAULT 'sistema' COMMENT 'Usuário, cliente de integração ou flyway',
    atualizado_em   DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    atualizado_por  VARCHAR(100)    NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uk_uni_turma_codigo (id_curso, codigo),
    UNIQUE KEY uk_uni_turma_sigla (id_curso, sigla)
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Módulo universidade — fase/turma com o nome completo usado pela universidade';
