using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Domain.App.Core.Database;

public static class EntityConfigurator
{
    public static ModelBuilder ApplyEntityConfigurationsFromCallingAssembly(ModelBuilder builder)
    {
        return builder.ApplyConfigurationsFromAssembly(Assembly.GetEntryAssembly() ??
            throw new InvalidOperationException("Cannot work in unmanaged application!"));
    }
}