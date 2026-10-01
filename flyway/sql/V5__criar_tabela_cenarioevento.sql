-- ============================================================================
-- V5 — hprof_cenarioevento
-- Trilha de auditoria imutável. Registra toda ação relevante:
-- criação, salvamento, restauração, aprovação, publicação, etc.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_cenarioevento (
    id              BIGINT          NOT NULL AUTO_INCREMENT,
    id_cenario      BIGINT          NOT NULL                COMMENT 'FK para hprof_cenario.id',
    id_versao       BIGINT          NULL                    COMMENT 'FK para hprof_cenarioversao.id (quando aplicável)',
    id_simulacao    BIGINT          NULL                    COMMENT 'FK para hprof_simulacao.id (quando aplicável)',

    -- Tipo do evento
    tipo_evento     VARCHAR(50)     NOT NULL                COMMENT 'CENARIO_CRIADO, VERSAO_SALVA, VERSAO_RESTAURADA, REGRA_ADICIONADA, EXCECAO_APROVADA, SIMULACAO_INICIADA, SIMULACAO_CONCLUIDA, PUBLICACAO_FEITA, PUBLICACAO_REVOGADA, STATUS_ALTERADO',

    -- Detalhes
    descricao       VARCHAR(500)    NOT NULL                COMMENT 'Descrição legível do evento',
    detalhes_json   JSON            NULL                    COMMENT 'Dados extras do evento (antes/depois, parâmetros, etc.)',

    -- Quem e quando
    usuario         VARCHAR(100)    NOT NULL,
    ip_origem       VARCHAR(45)     NULL                    COMMENT 'IPv4 ou IPv6 do solicitante',
    criado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),

    PRIMARY KEY (id),
    INDEX idx_evento_cenario    (id_cenario, criado_em),
    INDEX idx_evento_tipo       (tipo_evento),
    INDEX idx_evento_versao     (id_versao),
    INDEX idx_evento_simulacao  (id_simulacao),
    INDEX idx_evento_usuario    (usuario, criado_em),

    CONSTRAINT fk_evento_cenario
        FOREIGN KEY (id_cenario) REFERENCES hprof_cenario (id)
        ON DELETE RESTRICT ON UPDATE CASCADE,

    CONSTRAINT fk_evento_versao
        FOREIGN KEY (id_versao) REFERENCES hprof_cenarioversao (id)
        ON DELETE SET NULL ON UPDATE CASCADE,

    CONSTRAINT fk_evento_simulacao
        FOREIGN KEY (id_simulacao) REFERENCES hprof_simulacao (id)
        ON DELETE SET NULL ON UPDATE CASCADE
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Trilha de auditoria imutável de ações sobre cenários';
