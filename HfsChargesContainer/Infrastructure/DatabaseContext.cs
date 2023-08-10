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
        }

        public DbSet<WeeksByYear> WeeksByYear { get; set; }
        public DbSet<BatchLog> BatchLogs { get; set; }
        public DbSet<BatchLogError> BatchLogErrors { get; set; }
        public DbSet<ChargesBatchYear> ChargesBatchYears { get; set; }
        public DbSet<GoogleFileSetting> GoogleFileSettings { get; set; }
    }
}
