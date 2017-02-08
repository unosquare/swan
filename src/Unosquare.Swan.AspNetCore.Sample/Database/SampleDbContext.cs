namespace Unosquare.Swan.AspNetCore.Sample.Database
{
    using Microsoft.EntityFrameworkCore;
    using Unosquare.Swan.AspNetCore.Models;

    public class SampleDbContext : BusinessDbContext
    {
        public SampleDbContext(DbContextOptions<SampleDbContext> options)
            : base(options)
        {
            // TODO: Connect user with Identity
            var auditController = new AuditTrailController<SampleDbContext, AuditTrailEntry>(this, "System");
            auditController.AddTypes(ActionFlags.Create, new[] {typeof(Product)});
            auditController.AddTypes(ActionFlags.Update, new[] {typeof(Product)});

            AddController(auditController);
        }

        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<AuditTrailEntry> AuditTrailEntries { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}