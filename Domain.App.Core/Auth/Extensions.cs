using Domain.App.Core.Options;

using FirebaseAdmin;

using Google.Apis.Auth.OAuth2;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using NetDevPack.Security.JwtExtensions;


namespace Domain.App.Core.Auth;

public static class Extensions
{
    public static void AddJwtGenerator(this WebApplicationBuilder builder) =>
        builder.Services.AddTransient<JwtGenerator>();

    public static AuthOptions ConfigureAuthOptions(this WebApplicationBuilder builder)
    {
        IConfigurationSection optionsConfig = builder.Configuration.GetSection(nameof(AuthOptions));

        builder.Services.Configure<AuthOptions>(optionsConfig);

        AuthOptions authOptions = optionsConfig.Get<AuthOptions>() ?? new();

        if (authOptions.UseAuthInDevelopmentEnvironment &&
            !authOptions.IsAuthServer &&
            (string.IsNullOrWhiteSpace(authOptions.Server) || authOptions.Jwt is null))
            throw new InvalidOperationException(
                $"{nameof(AuthOptions)}.{nameof(AuthOptions.Server)} was not found in configuration!");

        return authOptions;
    }

    public static void ConfigureAuthClient(this WebApplicationBuilder builder, AuthOptions options)
    {
        builder.Services.AddJwksManager().UseJwtValidation();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.SaveToken = true;
                o.IncludeErrorDetails = builder.Environment.IsDevelopment();

                var jwkOptions = new JwkOptions(
                    jwksUri: $"{options.Server}/jwks",
                    issuer: options.Jwt.Issuer,
                    audience: options.Jwt.Audience);

                o.SetJwksOptions(jwkOptions);
            });

        builder.Services.AddAuthorization();
    }

    public static void ConfigureAuthServer<TDbContext>(this WebApplicationBuilder builder, AuthOptions options)
    where TDbContext : DbContext
    {
        builder.Services.AddJwksManager().UseJwtValidation();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.IncludeErrorDetails = builder.Environment.IsDevelopment();

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = options.Jwt.Issuer,
                    ValidAudience = options.Jwt.Audience
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
            opt.TokenLifespan = TimeSpan.FromHours(1));

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(o =>
            {
                o.SignIn.RequireConfirmedEmail = true;

                o.Password.RequiredLength = 8;
                o.Password.RequireDigit = true;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<TDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.GetApplicationDefault(),
            ProjectId = string.Empty //todo: update with project id
        }));
    }

    public static void UseAuthDiscovery(this WebApplication app)
    {
        app.UseJwksDiscovery();
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public static async Task UseApplicationRoles(this WebApplication app, AuthOptions authOptions)
    {
        if (!authOptions.IsAuthServer)
            return;

        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await roleManager.CreateApplicationRoleIfNotExist(ApplicationRoles.Admin);
    }


    private static async Task CreateApplicationRoleIfNotExist(
        this RoleManager<IdentityRole> roleManager,
        string roleName)
    {
        bool roleExist = await roleManager.RoleExistsAsync(roleName);

        if (!roleExist)
        {
            var roleCreationResult = await roleManager.CreateAsync(new IdentityRole(roleName));

            if (!roleCreationResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create application role: {roleName}!");
            }
        }
    }
}