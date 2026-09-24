using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

public class BalanceSnapshotConfiguration : IEntityTypeConfiguration<BalanceSnapshot>
{
    public void Configure(EntityTypeBuilder<BalanceSnapshot> builder)
    {
        builder.ToTable("balance_snapshots");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(b => b.SnapshotBalance).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(b => b.SnapshotAt).HasDefaultValueSql("now()");

        builder.HasIndex(b => new { b.AccountId, b.SnapshotAt }).IsDescending(false, true);

        builder.HasOne(b => b.Account)
            .WithMany()
            .HasForeignKey(b => b.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LedgerEntry>()
            .WithMany()
            .HasForeignKey(b => b.LastLedgerEntryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
