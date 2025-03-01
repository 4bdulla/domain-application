using System.Reflection;
using System.Text.Json;
using CorrelationId.DependencyInjection;
using Domain.App.Core.Exceptions.Abstraction;
using Domain.App.Core.Integration.Attributes;
using Domain.App.Core.Integration.Configurators;
using Domain.App.Core.Utility;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Domain.App.Core.Integration;

public static class Extensions
{
    public static void UseApplicationExceptionHandler(this WebApplication app)
    {
        bool isDevelopment = app.Environment.IsDevelopment();

        app.UseExceptionHandler(config => config.Run(async context =>
        {
            Exception exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

            ApiResponse<object> response = exception switch
            {
                IApplicationException appException =>
                    ApiResponse.CreateResponse(appException.ErrorCode, exception, isDevelopment),

                _ => ApiResponse.Error(exception, isDevelopment),
            };

            await context.Response.WriteAsJsonAsync(response);
        }));
    }

    public static void MapApiRoutes(this WebApplication app, bool useValidation, bool useAuthInDevelopmentEnvironment)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        string appDomainSimplifiedName = AppDomain.CurrentDomain.FriendlyName
            .Replace(".dll", string.Empty)
            .Replace(".exe", string.Empty);

        Type[] types = assemblies
            .SelectMany(a => a.GetTypesByAttribute<ApiRequestAttribute>(t => typeof(IBaseRequest).IsAssignableFrom(t)))
            .Where(t => t.Namespace?.Contains(appDomainSimplifiedName) == true)
            .ToArray();

        var configurator = new ApiRequestConfigurator(useValidation, useAuthInDevelopmentEnvironment);

        foreach (Type type in types)
        {
            configurator.ConfigureApiRequestRoute(app, type);
        }
    }

    public static void ConfigureRefitClients(this WebApplicationBuilder builder, JsonSerializerOptions settings)
    {
        var configurator = new IntegrationClientsConfigurator(AppDomain.CurrentDomain);

        configurator.ConfigureClients(builder, settings);
    }

    public static void ConfigureCorrelationId(this WebApplicationBuilder builder)
    {
        builder.Services.AddDefaultCorrelationId(options =>
        {
            options.AddToLoggingScope = true;
            options.UpdateTraceIdentifier = true;
        });
    }

    public static void AddFluentValidation(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
    }
}