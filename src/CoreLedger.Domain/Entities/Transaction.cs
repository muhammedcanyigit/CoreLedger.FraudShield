using CoreLedger.Domain.Enums;

namespace CoreLedger.Domain.Entities;

// Bir transferin "ust kaydi" / hikayesi. Iki LedgerEntry'yi (gonderen + alici) baglar.
public class Transaction
{
    public Guid Id { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public string? Description { get; set; }

    // AI Fraud Shield sonucu - audit trail icin saklanir, asla silinmez.
    public decimal? FraudScore { get; set; }
    public DateTimeOffset? FraudCheckedAt { get; set; }

    public DateTimeOffset InitiatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
}
