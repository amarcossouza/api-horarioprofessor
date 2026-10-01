-- ============================================================================
-- V3 — hprof_cenarioregra
-- Regras e exceções projetadas para cada versão do cenário.
-- Cada linha é uma regra classificada (obrigatória, preferência, trava,
-- exceção aprovada ou hipótese) vinculada a uma versão imutável.
-- ============================================================================

CREATE TABLE IF NOT EXISTS hprof_cenarioregra (
    id                  BIGINT          NOT NULL AUTO_INCREMENT,
    id_cenarioversao    BIGINT          NOT NULL            COMMENT 'FK para hprof_cenarioversao.id',

    -- Classificação da regra
    categoria           VARCHAR(30)     NOT NULL            COMMENT 'OBRIGATORIA, PREFERENCIA, TRAVA, EXCECAO_APROVADA, HIPOTESE',
    codigo_regra        VARCHAR(50)     NOT NULL            COMMENT 'Identificador da regra (ex: R1_SEM_CONFLITO_HORARIO)',
    descricao           VARCHAR(500)    NOT NULL            COMMENT 'Texto legível da regra',

    -- Escopo de aplicação
    id_professor        INT             NULL                COMMENT 'NULL = aplica a todos',
    nome_professor      VARCHAR(200)    NULL                COMMENT 'Para referência rápida sem JOIN',
    dia_semana          VARCHAR(10)     NULL                COMMENT 'SEG, TER, QUA, QUI, SEX ou NULL = todos',
    horario_inicio      INT             NULL                COMMENT 'Linha da matriz (1-based) ou NULL = todos',
    horario_fim         INT             NULL                COMMENT 'Linha final ou NULL = mesmo que inicio',
    turma               VARCHAR(50)     NULL                COMMENT 'Código da turma ou NULL = todas',

    -- Exceção e aprovação
    is_excecao          TINYINT(1)      NOT NULL DEFAULT 0  COMMENT '1 = exceção ao padrão original',
    regra_original      VARCHAR(500)    NULL                COMMENT 'O que estava antes da exceção',
    justificativa       TEXT            NULL                COMMENT 'Motivo da exceção',
    aprovado_por        VARCHAR(100)    NULL                COMMENT 'Quem autorizou',
    aprovado_em         DATETIME(3)     NULL,

    -- Vigência
    vigencia_inicio     DATE            NULL                COMMENT 'NULL = desde o início do ano',
    vigencia_fim        DATE            NULL                COMMENT 'NULL = até o fim do ano',

    -- Rastreabilidade
    origem              VARCHAR(50)     NOT NULL DEFAULT 'CADASTRO'
                                                            COMMENT 'CADASTRO, SIMULACAO, MANUAL, IMPORTACAO',
    criado_em           DATETIME(3)     NOT NULL DEFAULT CURRENT_TIMESTAMP(3),

    PRIMARY KEY (id),
    INDEX idx_regra_versao      (id_cenarioversao),
    INDEX idx_regra_categoria   (id_cenarioversao, categoria),
    INDEX idx_regra_professor   (id_cenarioversao, id_professor),
    INDEX idx_regra_codigo      (codigo_regra),

    CONSTRAINT fk_regra_versao
        FOREIGN KEY (id_cenarioversao) REFERENCES hprof_cenarioversao (id)
        ON DELETE CASCADE ON UPDATE CASCADE
)
ENGINE = InnoDB
DEFAULT CHARSET = utf8mb4
COLLATE = utf8mb4_unicode_ci
COMMENT = 'Regras e exceções por versão do cenário';
