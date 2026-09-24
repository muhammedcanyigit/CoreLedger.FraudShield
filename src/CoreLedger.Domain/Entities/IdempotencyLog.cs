namespace CoreLedger.Domain.Entities;

// Ayni istegin (ayni Idempotency-Key ile) 2 kez islenmesini engeller.
public class IdempotencyLog
{
    public Guid IdempotencyKey { get; set; }
    public string RequestHash { get; set; } = string.Empty;
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
