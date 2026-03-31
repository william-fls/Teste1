CREATE TABLE IF NOT EXISTS sala (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS agendamento (
    id SERIAL PRIMARY KEY,
    sala_id INTEGER NOT NULL,
    inicio TIMESTAMP NOT NULL,
    fim TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS log_operacao (
    id SERIAL PRIMARY KEY,
    nome_tabela VARCHAR(50) NOT NULL,
    tipo_operacao VARCHAR(10) NOT NULL,
    data_hora TIMESTAMP NOT NULL DEFAULT NOW()
);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'uq_sala_nome'
    ) THEN
        ALTER TABLE sala
            ADD CONSTRAINT uq_sala_nome UNIQUE (nome);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'chk_sala_nome_nao_vazio'
    ) THEN
        ALTER TABLE sala
            ADD CONSTRAINT chk_sala_nome_nao_vazio
            CHECK (btrim(nome) <> '');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_agendamento_sala'
    ) THEN
        ALTER TABLE agendamento
            ADD CONSTRAINT fk_agendamento_sala
            FOREIGN KEY (sala_id) REFERENCES sala(id) ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'chk_agendamento_datas'
    ) THEN
        ALTER TABLE agendamento
            ADD CONSTRAINT chk_agendamento_datas
            CHECK (fim > inicio);
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS idx_agendamento_sala_periodo
    ON agendamento (sala_id, inicio, fim);

CREATE OR REPLACE FUNCTION fn_log_operacao()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO log_operacao (nome_tabela, tipo_operacao, data_hora)
    VALUES (TG_TABLE_NAME, TG_OP, NOW());

    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_verificar_sobreposicao()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM agendamento
        WHERE sala_id = NEW.sala_id
          AND id <> COALESCE(NEW.id, -1)
          AND (NEW.inicio, NEW.fim) OVERLAPS (inicio, fim)
    ) THEN
        RAISE EXCEPTION 'Ja existe um agendamento para esta sala neste periodo.';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_verificar_exclusao_sala()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM agendamento
        WHERE sala_id = OLD.id
          AND fim > NOW()
    ) THEN
        RAISE EXCEPTION 'Nao e possivel excluir a sala pois ela possui agendamentos futuros.';
    END IF;

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_verificar_exclusao_agendamento()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.fim > NOW() THEN
        RAISE EXCEPTION 'Nao e possivel excluir um agendamento futuro.';
    END IF;

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_log_sala ON sala;
CREATE TRIGGER trg_log_sala
AFTER INSERT OR UPDATE OR DELETE ON sala
FOR EACH ROW
EXECUTE FUNCTION fn_log_operacao();

DROP TRIGGER IF EXISTS trg_log_agendamento ON agendamento;
CREATE TRIGGER trg_log_agendamento
AFTER INSERT OR UPDATE OR DELETE ON agendamento
FOR EACH ROW
EXECUTE FUNCTION fn_log_operacao();

DROP TRIGGER IF EXISTS trg_sobreposicao_agendamento ON agendamento;
CREATE TRIGGER trg_sobreposicao_agendamento
BEFORE INSERT OR UPDATE ON agendamento
FOR EACH ROW
EXECUTE FUNCTION fn_verificar_sobreposicao();

DROP TRIGGER IF EXISTS trg_exclusao_sala ON sala;
CREATE TRIGGER trg_exclusao_sala
BEFORE DELETE ON sala
FOR EACH ROW
EXECUTE FUNCTION fn_verificar_exclusao_sala();

DROP TRIGGER IF EXISTS trg_exclusao_agendamento ON agendamento;
CREATE TRIGGER trg_exclusao_agendamento
BEFORE DELETE ON agendamento
FOR EACH ROW
EXECUTE FUNCTION fn_verificar_exclusao_agendamento();
