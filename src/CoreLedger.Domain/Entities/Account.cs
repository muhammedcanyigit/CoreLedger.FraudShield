namespace CoreLedger.Domain.Entities;

// Hesap kimlik bilgisi. BILEREK bir Balance sutunu YOK.
// Bakiye her zaman ilgili LedgerEntries kayitlarinin toplamindan hesaplanir.
public class Account
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "TRY";
    public string Status { get; set; } = "active";
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
}
