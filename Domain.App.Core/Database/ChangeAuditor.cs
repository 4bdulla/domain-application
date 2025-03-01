using System.Security.Claims;
using Domain.App.Core.Database.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Domain.App.Core.Database;

public class ChangeAuditor
{
    private readonly IHttpContextAccessor _contextAccessor;

    public ChangeAuditor(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    }

    public void SetAuditDetails(EntityEntry<IAuditable> entry)
    {
        string userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

        switch (entry.State)
        {
            case EntityState.Added:
                entry.Entity.Creator = userId;
                entry.Entity.CreatedAt = DateTime.Now;

                break;

            case EntityState.Modified:
                entry.Entity.Modifier = userId;
                entry.Entity.ModifiedAt = DateTime.Now;

                break;
        }
    }
}