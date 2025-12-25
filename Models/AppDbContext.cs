using Microsoft.EntityFrameworkCore;

namespace Monolypix.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<GameSession> GameSessions => Set<GameSession>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameSession>()
                .HasIndex(gs => gs.Name)
                .IsUnique();

            modelBuilder.Entity<Wallet>()
                .HasIndex(w => new { w.UserId, w.GameSessionId })
                .IsUnique();

            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.GameSession)
                .WithMany()
                .HasForeignKey(w => w.GameSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Wallet>()
                .Property(w => w.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .HasOne(tr => tr.FromWallet)
                .WithMany()
                .HasForeignKey(tr => tr.FromWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(tr => tr.ToWallet)
                .WithMany()
                .HasForeignKey(tr => tr.ToWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasIndex(tr => tr.GameSessionId);

            modelBuilder.Entity<Transaction>()
                .HasIndex(tr => tr.FromWalletId);

            modelBuilder.Entity<Transaction>()
                .HasIndex(tr => tr.ToWalletId);

            modelBuilder.Entity<Transaction>()
                .Property(tr => tr.Amount)
                .HasPrecision(18, 2);
        }
    }
}
