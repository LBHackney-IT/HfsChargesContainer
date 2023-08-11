using Microsoft.EntityFrameworkCore;
using HfsChargesContainer.Infrastructure.Models;

namespace HfsChargesContainer.Infrastructure
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeeksByYear>().HasNoKey();
            modelBuilder.Entity<ChargesAux>().Property(x => x.TimeStamp).HasDefaultValueSql("GETDATE()");
        }

        public DbSet<WeeksByYear> WeeksByYear { get; set; }
        public DbSet<BatchLog> BatchLogs { get; set; }
        public DbSet<BatchLogError> BatchLogErrors { get; set; }
        public DbSet<ChargesBatchYear> ChargesBatchYears { get; set; }
        public DbSet<GoogleFileSetting> GoogleFileSettings { get; set; }

        public async Task LoadCharges()
            => await PerformTransaction($"usp_LoadCharges", 300).ConfigureAwait(false);

        public async Task LoadChargesHistory(int @processingYear)
            => await PerformInterpolatedTransaction($"usp_LoadChargesHistory {@processingYear}", 600).ConfigureAwait(false);

        public async Task TruncateChargesAuxiliary()
        {
            var sql = "DELETE FROM ChargesAux";
            await PerformTransaction(sql).ConfigureAwait(false);
        }

        private async Task PerformTransaction(string sql, int timeout = 0)
        {
            await using var transaction = await Database.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                if (timeout != 0)
                    Database.SetCommandTimeout(timeout);
                await Database.ExecuteSqlRawAsync(sql).ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        private async Task PerformInterpolatedTransaction(FormattableString sql, int timeout = 0)
        {
            await using var transaction = await Database.BeginTransactionAsync().ConfigureAwait(false);

            try
            {
                if (timeout != 0)
                    Database.SetCommandTimeout(timeout);
                await Database.ExecuteSqlInterpolatedAsync(sql).ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
    }
}
