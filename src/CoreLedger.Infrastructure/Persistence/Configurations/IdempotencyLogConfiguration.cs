using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

public class IdempotencyLogConfiguration : IEntityTypeConfiguration<IdempotencyLog>
{
    public void Configure(EntityTypeBuilder<IdempotencyLog> builder)
    {
        builder.ToTable("idempotency_logs");

        builder.HasKey(i => i.IdempotencyKey);
        // Dikkat: burada default UUID uretimi YOK — key'i istemci (client) gonderir.

        builder.Property(i => i.RequestHash).HasMaxLength(64).IsRequired();
        builder.Property(i => i.ResponseBody).HasColumnType("jsonb");
        builder.Property(i => i.CreatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(i => i.ExpiresAt);
    }
}
