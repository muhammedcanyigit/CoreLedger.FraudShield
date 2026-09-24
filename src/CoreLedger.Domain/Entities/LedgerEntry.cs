using CoreLedger.Domain.Enums;

namespace CoreLedger.Domain.Entities;

// Sistemin kalbi. IMMUTABLE: bu kayitlar hicbir zaman guncellenmez veya silinmez,
// sadece eklenir (INSERT). Her transfer icin 2 satir olusur: 1 Debit + 1 Credit.
public class LedgerEntry
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }

    public LedgerDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";

    public DateTimeOffset CreatedAt { get; set; }

    public Transaction? Transaction { get; set; }
    public Account? Account { get; set; }
}
