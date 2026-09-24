using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("ledger_entries", t =>
        {
            t.HasCheckConstraint("CK_LedgerEntries_Direction", "direction IN ('Debit', 'Credit')");
            t.HasCheckConstraint("CK_LedgerEntries_Amount_Positive", "amount > 0");
        });

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(l => l.Direction)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(l => l.Amount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(l => l.Currency).HasMaxLength(3).IsRequired();
        builder.Property(l => l.CreatedAt).HasDefaultValueSql("now()");

        // En kritik index: bir hesabin bakiyesini SUM ile hizlica hesaplamak icin.
        builder.HasIndex(l => new { l.AccountId, l.CreatedAt });

        builder.HasOne(l => l.Transaction)
            .WithMany(t => t.LedgerEntries)
            .HasForeignKey(l => l.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Account)
            .WithMany(a => a.LedgerEntries)
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
