using Microsoft.EntityFrameworkCore;
using HfsChargesContainer.Infrastructure.Models;

namespace HfsChargesContainer.Infrastructure
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeeksByYear>().HasNoKey();//.ToView(null);
        }

        public DbSet<WeeksByYear> WeeksByYear { get; set; }
    }
}
