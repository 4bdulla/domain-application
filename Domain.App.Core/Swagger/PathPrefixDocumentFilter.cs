using Domain.App.Core.Options;

using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;


namespace Domain.App.Core.Swagger;

public class PathPrefixDocumentFilter : IDocumentFilter
{
    private readonly RoutingOptions _options;

    public PathPrefixDocumentFilter(IOptions<RoutingOptions> options)
    {
        _options = options?.Value;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (_options is null || string.IsNullOrEmpty(_options.RoutePrefix))
            return;

        var paths = swaggerDoc.Paths.Keys.ToList();

        foreach (string path in paths)
        {
            OpenApiPathItem pathToChange = swaggerDoc.Paths[path];

            swaggerDoc.Paths.Remove(path);

            string leadingSlash = _options.RoutePrefix.StartsWith("/") ? string.Empty : "/";
            swaggerDoc.Paths.Add($"{leadingSlash}{_options.RoutePrefix}{path}", pathToChange);
        }
    }
}