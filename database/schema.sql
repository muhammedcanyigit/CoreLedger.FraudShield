-- =========================================================
-- Core-Ledger & AI Fraud Shield — Veritabani Semasi (Faz 1)
-- PostgreSQL 15+
-- =========================================================
-- Bu dosya CLAUDE.md'deki "Vizyon" bolumunun birebir SQL karsiligidir:
--   1) Bakiye asla dogrudan guncellenmez (Accounts tablosunda Balance yok)
--   2) Her hareket immutable iki satir olarak LedgerEntries'e yazilir
--   3) Idempotency ve Fraud denetimi icin gerekli alanlar hazirdir

-- NOT: Bu dosya artik gercek semanin "kaynagi" degil, okunabilir tasarim
-- referansidir. Gercek sema Faz 2'de EF Core migration'lari ile uretilir
-- (src/CoreLedger.Infrastructure/Persistence/Migrations). Ikisi senkron tutulur.
--
-- Enum alanlari icin native PostgreSQL ENUM yerine VARCHAR + CHECK constraint
-- tercih edildi. Neden: native ENUM'a sonradan yeni deger eklemek (ALTER TYPE
-- ... ADD VALUE) PostgreSQL'de transaction icinde calismaz ve deployment'lari
-- zorlastirir. VARCHAR + CHECK ise tek satirlik bir ALTER TABLE ile guncellenir.

-- UUID uretimi icin gerekli extension
CREATE EXTENSION IF NOT EXISTS pgcrypto;


-- ---------------------------------------------------------
-- 1) ACCOUNTS — Hesap kimlik bilgisi. BAKIYE SUTUNU YOK.
--    Bakiye her zaman ledger_entries'in toplamindan hesaplanir.
-- ---------------------------------------------------------
CREATE TABLE accounts (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    owner_user_id   UUID NOT NULL,                   -- ileride Users/Auth tablosuna FK olacak
    account_number  VARCHAR(34) NOT NULL UNIQUE,      -- IBAN benzeri tekil hesap no
    currency        CHAR(3) NOT NULL DEFAULT 'TRY',   -- ISO 4217 kodu (TRY, USD, EUR)
    status          VARCHAR(20) NOT NULL DEFAULT 'active', -- active / frozen / closed
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_accounts_owner_user_id ON accounts(owner_user_id);


-- ---------------------------------------------------------
-- 2) TRANSACTIONS — Bir transferin "ust kaydi" / hikayesi.
--    Iki LedgerEntry'yi (gonderen + alici) birbirine baglar.
-- ---------------------------------------------------------
CREATE TABLE transactions (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    type                VARCHAR(20) NOT NULL
                            CHECK (type IN ('Transfer', 'Deposit', 'Withdrawal')),
    status              VARCHAR(20) NOT NULL DEFAULT 'Pending'
                            CHECK (status IN ('Pending', 'Completed', 'Failed', 'Blocked')),
    description         TEXT,

    -- AI Fraud Shield sonucu — audit trail icin saklanir, asla silinmez
    fraud_score         NUMERIC(4,3),              -- 0.000 - 1.000 arasi
    fraud_checked_at    TIMESTAMPTZ,

    initiated_at        TIMESTAMPTZ NOT NULL DEFAULT now(),
    completed_at        TIMESTAMPTZ
);


-- ---------------------------------------------------------
-- 3) LEDGER_ENTRIES — Sistemin kalbi. IMMUTABLE (UPDATE/DELETE yasak).
--    Sadece INSERT edilir. Her transfer icin 2 satir olusur: 1 debit + 1 credit.
-- ---------------------------------------------------------
CREATE TABLE ledger_entries (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    transaction_id  UUID NOT NULL REFERENCES transactions(id),
    account_id      UUID NOT NULL REFERENCES accounts(id),

    direction       VARCHAR(10) NOT NULL
                        CHECK (direction IN ('Debit', 'Credit')),
    amount          NUMERIC(18,2) NOT NULL CHECK (amount > 0), -- her zaman pozitif, yonu direction belirler
    currency        CHAR(3) NOT NULL,

    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
    -- Dikkat: updated_at YOK. Bu tabloya UPDATE hicbir zaman atilmaz.
);

-- Bir hesabin bakiyesini SUM ile hizlica hesaplamak icin en kritik index
CREATE INDEX idx_ledger_entries_account_created ON ledger_entries(account_id, created_at);
CREATE INDEX idx_ledger_entries_transaction_id ON ledger_entries(transaction_id);


-- ---------------------------------------------------------
-- 4) BALANCE_SNAPSHOTS — Performans icin "dondurulmus" bakiye anlari.
--    Guncel bakiye = son snapshot + son snapshot'tan SONRAKI entry'lerin toplami.
-- ---------------------------------------------------------
CREATE TABLE balance_snapshots (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id              UUID NOT NULL REFERENCES accounts(id),
    snapshot_balance        NUMERIC(18,2) NOT NULL,

    -- Bu snapshot alinirken en son islenmis ledger_entry hangisiydi?
    -- Bakiye hesaplanirken bu id'den sonraki kayitlar toplanir, oncesi tekrar toplanmaz.
    last_ledger_entry_id    UUID REFERENCES ledger_entries(id),

    snapshot_at             TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_balance_snapshots_account_time ON balance_snapshots(account_id, snapshot_at DESC);


-- ---------------------------------------------------------
-- 5) IDEMPOTENCY_LOGS — Ayni istegin 2 kez islenmesini engeller.
-- ---------------------------------------------------------
CREATE TABLE idempotency_logs (
    idempotency_key       UUID PRIMARY KEY,          -- istemcinin gonderdigi GUID
    request_hash          VARCHAR(64) NOT NULL,       -- ayni key farkli body ile gelirse cakisma tespiti (SHA-256)
    response_status_code  INT,
    response_body         JSONB,                      -- onceki cevabin aynisini donebilmek icin

    created_at             TIMESTAMPTZ NOT NULL DEFAULT now(),
    expires_at             TIMESTAMPTZ NOT NULL        -- ornek: created_at + 24 saat, sonra temizlenebilir
);

CREATE INDEX idx_idempotency_logs_expires_at ON idempotency_logs(expires_at);
