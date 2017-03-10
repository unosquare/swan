namespace Unosquare.Swan.AspNetCore.Sample.Database
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class SampleDbContext : BusinessDbContext
    {
        public SampleDbContext(DbContextOptions<SampleDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            var auditController = new AuditTrailController<SampleDbContext, AuditTrailEntry>(this,
                httpContextAccessor?.HttpContext?.User?.Identity?.Name);
            auditController.AddTypes(ActionFlags.Create, new[] {typeof(Product)});

            AddController(auditController);
        }

        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<AuditTrailEntry> AuditTrailEntries { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}