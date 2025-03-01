using System.Threading;
using Domain.App.Core.Database.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Domain.App.Core.Database.Context;

public abstract class IdentityApplicationDbContext
    : IdentityDbContext<
        IdentityUser,
        IdentityRole,
        string,
        IdentityUserClaim<string>,
        ApplicationUserRole,
        IdentityUserLogin<string>,
        IdentityRoleClaim<string>,
        IdentityUserToken<string>>
{
    private readonly ChangeAuditor _auditor;

    protected IdentityApplicationDbContext(DbContextOptions options, ChangeAuditor auditor) : base(options)
    {
        _auditor = auditor ?? throw new ArgumentNullException(nameof(auditor));
    }


    public override async Task<int> SaveChangesAsync(CancellationToken token = new())
    {
        base.ChangeTracker.Entries<IAuditable>().ToList().ForEach(_auditor.SetAuditDetails);

        int result = await base.SaveChangesAsync(token);

        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(EntityConfigurator.ApplyEntityConfigurationsFromCallingAssembly(builder));

        builder.Entity<ApplicationUserRole>(b => b.HasKey(r => new { r.UserId, r.RoleId, r.DomainId }));
    }
}