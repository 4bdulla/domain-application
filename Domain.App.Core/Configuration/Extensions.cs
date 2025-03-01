using ConfigurationSubstitution;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Domain.App.Core.Configuration;

public static class Extensions
{
    public static void LoadConfiguration(this WebApplicationBuilder builder)
    {
        string configurationFile = builder.Environment.IsDevelopment()
            ? "appsettings.Development.json"
            : "appsettings.json";

        builder.Configuration
            // .AddJsonFile(configurationFile)
            .EnableSubstitutionsWithDelimitedFallbackDefaults("{", "}", ":")
            .AddEnvironmentVariables("ENV_");
    }

    /// <summary>
    /// Configures and validates options to IOC in <see cref="IOptions{TOptions}"/> pattern
    /// </summary>
    /// <param name="builder"><see cref="WebApplicationBuilder"/></param>
    /// <param name="validator">Validator that can be used to validate options class and its properties (if not specified will not be invoked)</param>
    /// <typeparam name="TOptions">Type of options that needs to be configured inside of <see cref="IOptions{TOptions}"/> generic parameter</typeparam>
    /// <exception cref="InvalidOperationException">Will throw error if validator is not null &amp; validator returned false</exception>
    public static TOptions ConfigureOptions<TOptions>(this WebApplicationBuilder builder, Predicate<TOptions> validator = null)
    where TOptions : class
    {
        string optionsName = typeof(TOptions).Name;

        IConfigurationSection optionsConfiguration = builder.Configuration.GetSection(optionsName);

        TOptions options = optionsConfiguration.Get<TOptions>();

        if (validator is not null && !validator.Invoke(options))
            throw new InvalidOperationException($"{optionsName} is not found in configuration or misconfigured!");

        builder.Services.Configure<TOptions>(optionsConfiguration);

        return options;
    }
}