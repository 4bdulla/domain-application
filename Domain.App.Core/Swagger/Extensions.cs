using Domain.App.Core.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;


namespace Domain.App.Core.Swagger;

public static class Extensions
{
    public static RoutingOptions ConfigureRoutingOptions(this WebApplicationBuilder builder)
    {
        IConfigurationSection options = builder.Configuration.GetSection(nameof(RoutingOptions));

        builder.Services.Configure<RoutingOptions>(options);

        RoutingOptions routingOptions = options.Get<RoutingOptions>();

        return routingOptions;
    }

    /// <summary>
    ///     Adds SwaggerGen
    /// </summary>
    /// <param name="builder">
    ///     <see cref="WebApplicationBuilder" />
    /// </param>
    /// <param name="useJwtSecurityScheme">if true adds <see cref="OpenApiSecurityScheme" /> with "Bearer" scheme</param>
    public static void AddSwagger(this WebApplicationBuilder builder, bool useJwtSecurityScheme)
    {
        builder.Services.AddSwaggerGen(o =>
        {
            if (useJwtSecurityScheme)
            {
                o.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        Description = "Enter your valid token"
                    });

                o.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                o.EnableAnnotations();
            }

            if (!builder.Environment.IsDevelopment())
            {
                o.DocumentFilter<PathPrefixDocumentFilter>();
            }
        });
    }
}