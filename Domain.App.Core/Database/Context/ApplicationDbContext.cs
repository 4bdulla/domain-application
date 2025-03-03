using Domain.App.Core.Database.Abstractions;

using Microsoft.EntityFrameworkCore;


namespace Domain.App.Core.Database.Context;

public abstract class ApplicationDbContext : DbContext
{
    private readonly ChangeAuditor _auditor;

    protected ApplicationDbContext(DbContextOptions options, ChangeAuditor auditor) : base(options)
    {
        _auditor = auditor;
    }


    public override async Task<int> SaveChangesAsync(CancellationToken token = new())
    {
        base.ChangeTracker.Entries<IAuditable>().ToList().ForEach(_auditor.SetAuditDetails);

        int result = await base.SaveChangesAsync(token);

        return result;
    }


    protected override void OnModelCreating(ModelBuilder builder) =>
        base.OnModelCreating(EntityConfigurator.ApplyEntityConfigurationsFromCallingAssembly(builder));
}