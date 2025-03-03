using Domain.App.Core.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Domain.App.Core.Database;

public static class Extensions
{
    public static SqlDbOptions ConfigureSqlDbOptions(this WebApplicationBuilder builder)
    {
        IConfigurationSection options = builder.Configuration.GetSection(nameof(SqlDbOptions));

        SqlDbOptions sqlDbOptions = options.Get<SqlDbOptions>();

        if (sqlDbOptions is null)
            throw new InvalidOperationException($"{nameof(SqlDbOptions)} was not found in configuration!");

        if (string.IsNullOrWhiteSpace(sqlDbOptions.ConnectionString))
            throw new InvalidOperationException($"{nameof(SqlDbOptions)}.{nameof(SqlDbOptions.ConnectionString)} can't be null!");

        builder.Services.Configure<SqlDbOptions>(options);

        return sqlDbOptions;
    }

    public static void AddSqlDb<TDbContext>(this WebApplicationBuilder builder, SqlDbOptions options)
    where TDbContext : DbContext => builder.Services.AddDbContext<DbContext, TDbContext>(o => o.UseSqlServer(options.ConnectionString));

    public static void AddAuditor(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<ChangeAuditor>();
    }

    public static async Task<bool> EnsureApplicationDbCreatedAsync<TDbContext>(this WebApplication app, SqlDbOptions options)
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