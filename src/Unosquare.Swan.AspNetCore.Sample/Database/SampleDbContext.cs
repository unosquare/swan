namespace Unosquare.Swan.AspNetCore.Sample.Database
{
    using Microsoft.EntityFrameworkCore;
    using Unosquare.Swan.AspNetCore.Models;

    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions<SampleDbContext> options)
            : base(options)
        { }

        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<AuditTrailEntry> AuditTrailEntries { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}
