using Microsoft.AspNetCore.Identity;

namespace Domain.App.Core.Database.Context;

public class ApplicationUserRole : IdentityUserRole<string>
{
    public int DomainId { get; set; }
}