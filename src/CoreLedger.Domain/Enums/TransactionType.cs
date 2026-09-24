namespace CoreLedger.Domain.Enums;

// database/schema.sql'deki transaction_type ENUM tipiyle birebir eslesir.
public enum TransactionType
{
    Transfer,
    Deposit,
    Withdrawal
}
