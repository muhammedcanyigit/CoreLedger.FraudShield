using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLedger.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions", t =>
        {
            // Not: HasCheckConstraint'e verilen SQL metni ham metindir, snake_case
            // isimlendirme donusumunden gecmez. Bu yuzden gercek (donusmus) sutun
            // adlarini (type, status) kullaniyoruz.
            t.HasCheckConstraint(
                "CK_Transactions_Type",
                "type IN ('Transfer', 'Deposit', 'Withdrawal')");
            t.HasCheckConstraint(
                "CK_Transactions_Status",
                "status IN ('Pending', 'Completed', 'Failed', 'Blocked')");
        });

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(TransactionStatus.Pending)
            .IsRequired();

        builder.Property(t => t.FraudScore).HasColumnType("numeric(4,3)");

        builder.Property(t => t.InitiatedAt).HasDefaultValueSql("now()");
    }
}
