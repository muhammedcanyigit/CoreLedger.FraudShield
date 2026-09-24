using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.Infrastructure.Persistence;

public class CoreLedgerDbContext : DbContext
{
    public CoreLedgerDbContext(DbContextOptions<CoreLedgerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<BalanceSnapshot> BalanceSnapshots => Set<BalanceSnapshot>();
    public DbSet<IdempotencyLog> IdempotencyLogs => Set<IdempotencyLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreLedgerDbContext).Assembly);
    }
}
