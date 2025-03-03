using Domain.App.Core.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Domain.App.Core.Database;

public static class Extensions
{
    public static DatabaseOptions ConfigureSqlDbOptions(this WebApplicationBuilder builder)
    {
        IConfigurationSection options = builder.Configuration.GetSection(nameof(DatabaseOptions));

        DatabaseOptions databaseOptions = options.Get<DatabaseOptions>();

        if (databaseOptions is null)
            throw new InvalidOperationException($"{nameof(DatabaseOptions)} was not found in configuration!");

        if (string.IsNullOrWhiteSpace(databaseOptions.ConnectionString))
            throw new InvalidOperationException($"{nameof(DatabaseOptions)}.{nameof(DatabaseOptions.ConnectionString)} can't be null!");

        builder.Services.Configure<DatabaseOptions>(options);

        return databaseOptions;
    }

    public static void AddSqlDb<TDbContext>(this WebApplicationBuilder builder, DatabaseOptions options)
    where TDbContext : DbContext => builder.Services.AddDbContext<DbContext, TDbContext>(o => o.UseSqlServer(options.ConnectionString));

    public static void AddAuditor(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<ChangeAuditor>();
    }

    public static async Task<bool> EnsureApplicationDbCreatedAsync<TDbContext>(this WebApplication app, DatabaseOptions options)
    where TDbContext : DbContext
    {
        if (!app.Environment.IsDevelopment())
            return false;

        if (!options.CreateDbInDevelopmentEnvironment)
            return false;

        using IServiceScope scope = app.Services.CreateScope();

        TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        bool created = await dbContext.Database.EnsureCreatedAsync();

        return created;
    }
}