using Log.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Log.Api.Infrastructure.Persistence
{
    public class LogDbContext : DbContext
    {
        public LogDbContext(DbContextOptions<LogDbContext> options) : base(options) { }
        public DbSet<LogEntry> Logs => Set<LogEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogEntry>(b =>
            {
                b.ToTable("Logs");
                b.HasKey(x => x.Id);
                b.Property(x => x.ServiceName).HasMaxLength(200).IsRequired();
                b.Property(x => x.Level).HasMaxLength(50).IsRequired();
                b.Property(x => x.Message).IsRequired();
                b.HasIndex(x => x.ServiceName);
                b.HasIndex(x => x.Level);
                b.HasIndex(x => x.TimestampUtc);
            });
        }
    }

}
