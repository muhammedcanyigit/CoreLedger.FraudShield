namespace CoreLedger.Domain.Enums;

// database/schema.sql'deki transaction_status ENUM tipiyle birebir eslesir.
public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Blocked
}
