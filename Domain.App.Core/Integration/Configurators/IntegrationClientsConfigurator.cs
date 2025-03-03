using System.Reflection;
using System.Text.Json;

using CorrelationId.HttpClient;

using Domain.App.Core.Integration.Attributes;
using Domain.App.Core.Options;
using Domain.App.Core.Utility;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Prometheus;

using Refit;

using Serilog;


namespace Domain.App.Core.Integration.Configurators;

internal class IntegrationClientsConfigurator
{
    private readonly Type[] _types;

    public IntegrationClientsConfigurator(AppDomain domain)
    {
        string appDomainSimplifiedName = domain.FriendlyName
            .Replace(".dll", string.Empty)
            .Replace(".exe", string.Empty);

        Log.Debug("{Configurator} starting for {ApplicationName}", nameof(IntegrationClientsConfigurator), appDomainSimplifiedName);

        _types = domain.GetAssemblies()
            .SelectMany(a => a.GetTypesByAttribute<IntegrationClientAttribute>())
            .Where(t => !t.Name.TrimStart('I').Equals(appDomainSimplifiedName))
            .ToArray();
    }

    public void ConfigureClients(WebApplicationBuilder builder, JsonSerializerOptions settings)
    {
        foreach (Type type in _types)
        {
            string clientName = type.Name.TrimStart('I');

            ClientOptions clientOptions = GetClientOptions(builder, type, clientName);

            Log.Debug("configuring {Client} options: {@Options}", clientName, clientOptions);

            ConfigureClient(builder, type, clientOptions, settings);
        }
    }


    private static ClientOptions GetClientOptions(WebApplicationBuilder builder, Type type, string clientName)
    {
        ClientOptions clientOptions = builder.Configuration
            .GetSection($"{nameof(ClientOptions)}:{clientName}")
            .Get<ClientOptions>();

        if (clientOptions is not null && !string.IsNullOrWhiteSpace(clientOptions.BaseAddress))
        {
            Log.Debug("using {Client} options from configuration", clientName);

            return clientOptions;
        }

        IntegrationClientAttribute integrationClientAttribute = type.GetCustomAttribute<IntegrationClientAttribute>();

        clientOptions = integrationClientAttribute!.Options;

        if (clientOptions is not null && !string.IsNullOrWhiteSpace(clientOptions.BaseAddress))
        {
            Log.Debug("using {Client} options from {Attribute}", clientName, nameof(IntegrationClientAttribute));

            return clientOptions;
        }

        var errorMessage =
            $"{clientName} options should be either configured in settings file or at least {nameof(ClientOptions.BaseAddress)} should be defined in {nameof(IntegrationClientAttribute)} constructor!";

        throw new InvalidOperationException(errorMessage);
    }


    private static void ConfigureClient(
        WebApplicationBuilder builder,
        Type type,
        ClientOptions options,
        JsonSerializerOptions serializerOptions)
    {
        builder.Services
            .AddRefitClient(type, new RefitSettings(new SystemTextJsonContentSerializer(serializerOptions)))
            .AddCorrelationIdForwarding()
            .ConfigureHttpClient(
                client =>
                {
                    client.BaseAddress = new Uri(options.BaseAddress);
                    client.Timeout = options.Timeout;
                })
            .UseHttpClientMetrics();
    }
}