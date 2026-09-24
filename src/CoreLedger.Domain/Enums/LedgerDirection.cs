namespace CoreLedger.Domain.Enums;

// Bir LedgerEntry'nin yonu: hesaptan mi cikiyor (Debit), hesaba mi giriyor (Credit).
// database/schema.sql'deki ledger_direction ENUM tipiyle birebir eslesir.
public enum LedgerDirection
{
    Debit,
    Credit
}
