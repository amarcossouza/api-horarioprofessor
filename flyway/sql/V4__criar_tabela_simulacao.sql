-- ============================================================================
-- V4 — hprof_simulacao
-- Trabalho de busca / simulação. Cada simulação parte de uma versão-base,
-- executa com determinados parâmetros e pode produzir novas versões
-- candidatas.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_simulacao (
    id                  BIGINT          NOT NULL AUTO_INCREMENT,
    id_cenarioversao    BIGINT          NOT NULL            COMMENT 'FK para versão-base usada como ponto de partida',
    id_cenario          BIGINT          NOT NULL            COMMENT 'FK para hprof_cenario.id (desnormalizado para consulta)',

    -- Modo de operação
    modo                VARCHAR(30)     NOT NULL            COMMENT 'ESTRITO, COM_CONCESSOES, REPARO_LOCALIZADO',
    titulo              VARCHAR(200)    NULL                COMMENT 'Descrição opcional da simulação',

    -- Parâmetros de execução
    parametros_json     JSON            NULL                COMMENT 'Professores liberados, travas, limites, sementes, etc.',
    orcamento_mudancas  INT             NULL                COMMENT 'Máximo de células que podem mudar (NULL = sem limite)',
    tempo_limite_seg    INT             NOT NULL DEFAULT 120 COMMENT 'Timeout em segundos',

    -- Estado e progresso
    status              VARCHAR(30)     NOT NULL DEFAULT 'SOLICITADA'
                                                            COMMENT 'SOLICITADA, EXECUTANDO, CONCLUIDA, CANCELADA, FALHOU, INTERROMPIDA',
    progresso_pct       DECIMAL(5,2)    NULL                COMMENT 'Percentual estimado de progresso',
    mensagem_status     TEXT            NULL                COMMENT 'Mensagem de progresso ou erro',

    -- Resultado
    status_solver       VARCHAR(20)     NULL                COMMENT 'OPTIMAL, FEASIBLE, INFEASIBLE, UNKNOWN (CP-SAT)',
    id_versao_resultado BIGINT          NULL                COMMENT 'FK para versão gerada como resultado',
    total_alternativas  INT             NOT NULL DEFAULT 0  COMMENT 'Quantas alternativas foram geradas',

    -- Métricas
    tempo_execucao_ms   BIGINT          NULL                COMMENT 'Duração real em milissegundos',
    tentativas          INT             NULL                COMMENT 'Número de iterações/tentativas do solver',
    semente             BIGINT          NULL                COMMENT 'Seed usada para reprodutibilidade',

    -- Auditoria
    solicitado_por      VARCHAR(100)    NOT NULL,
    solicitado_em       DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    iniciado_em         DATETIME(3)     NULL,
    concluido_em        DATETIME(3)     NULL,

    PRIMARY KEY (id),
    INDEX idx_simulacao_cenario     (id_cenario),
    INDEX idx_simulacao_versaobase  (id_cenarioversao),
    INDEX idx_simulacao_status      (status),
    INDEX idx_simulacao_resultado   (id_versao_resultado),

    CONSTRAINT fk_simulacao_versaobase
        FOREIGN KEY (id_cenarioversao) REFERENCES hprof_cenarioversao (id)
        ON DELETE RESTRICT ON UPDATE CASCADE,

    CONSTRAINT fk_simulacao_cenario
        FOREIGN KEY (id_cenario) REFERENCES hprof_cenario (id)
        ON DELETE RESTRICT ON UPDATE CASCADE,

    CONSTRAINT fk_simulacao_resultado
        FOREIGN KEY (id_versao_resultado) REFERENCES hprof_cenarioversao (id)
        ON DELETE SET NULL ON UPDATE CASCADE
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Trabalho de simulação — modo, parâmetros, estado e resultado';
