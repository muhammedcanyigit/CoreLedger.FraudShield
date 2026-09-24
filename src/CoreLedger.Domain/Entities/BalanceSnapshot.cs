namespace CoreLedger.Domain.Entities;

// Performans icin "dondurulmus" bakiye ani.
// Guncel bakiye = bu snapshot_balance + LastLedgerEntryId'den SONRAKI entry'lerin toplami.
public class BalanceSnapshot
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public decimal SnapshotBalance { get; set; }
    public Guid? LastLedgerEntryId { get; set; }
    public DateTimeOffset SnapshotAt { get; set; }

    public Account? Account { get; set; }
}
