using System.Text.Json;

using CorrelationId.DependencyInjection;

using Domain.App.Core.Exceptions.Abstraction;
using Domain.App.Core.Integration.Configurators;

using FluentValidation;

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
                    ResponseFactory.CreateResponse(appException.ErrorCode, exception, isDevelopment),

                _ => ResponseFactory.Error(exception, isDevelopment)
            };

            await context.Response.WriteAsJsonAsync(response);
        }));
    }

    public static void MapApiRoutes(this WebApplication app, bool useValidation, bool useAuth) =>
        new ApiRequestConfigurator(AppDomain.CurrentDomain, app.Environment.ApplicationName, useValidation, useAuth)
            .ConfigureApiRequestRoutes(app);

    public static void ConfigureRefitClients(this WebApplicationBuilder builder, JsonSerializerOptions serializerOptions) =>
        new IntegrationClientsConfigurator(AppDomain.CurrentDomain, builder.Environment.ApplicationName)
            .ConfigureClients(builder, serializerOptions);

    public static void ConfigureCorrelationId(this WebApplicationBuilder builder) =>
        builder.Services.AddDefaultCorrelationId(options =>
        {
            options.AddToLoggingScope = true;
            options.UpdateTraceIdentifier = true;
        });

    public static void AddFluentValidation(this WebApplicationBuilder builder) =>
        builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
}