using System.Reflection;

using Domain.App.Core.Integration.Attributes;
using Domain.App.Core.Utility;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Serilog;

using HttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;


namespace Domain.App.Core.Integration.Configurators;

internal class ApiRequestConfigurator
{
    private readonly Type[] _types;
    private readonly bool _useValidation;
    private readonly bool _useAuth;


    internal ApiRequestConfigurator(AppDomain domain, string applicationName, bool useValidation, bool useAuth)
    {
        _types = domain.GetAssemblies()
            .GetTypesByAttribute<ApiRequestAttribute>(
                t => typeof(IBaseRequest).IsAssignableFrom(t) && t.Namespace is not null && t.Namespace.Contains(applicationName))
            .ToArray();

        _useValidation = useValidation;
        _useAuth = useAuth;

        Log.Debug("{Configurator} starting for {ApplicationName} with UseValidation = {UseValidation}, UseAuth = {UseAuth}",
            nameof(ApiRequestConfigurator),
            applicationName,
            _useValidation,
            _useAuth);
    }

    internal void ConfigureApiRequestRoutes(WebApplication app)
    {
        foreach (Type type in _types)
        {
            this.ConfigureApiRequestRoute(app, type);
        }
    }

    private void ConfigureApiRequestRoute(WebApplication app, Type type)
    {
        Log.Debug("configuring route for {Type}", type.Name);

        ApiRequestAttribute attribute = type.GetCustomAttribute<ApiRequestAttribute>();

        Log.Debug("found ApiRequestAttribute: {@Attribute}", attribute);

        Delegate invoke = this.GetRequestDelegate(type, attribute);

        Log.Debug("mapping {Method} to the {Path} with {Type} request", attribute!.Method, attribute!.Path, type.Name);

        // todo: enrich code to use swagger's annotations on response type
        this.MapApiRoute(app, attribute, invoke);
    }


    private Delegate GetRequestDelegate(Type type, ApiRequestAttribute attribute)
    {
        bool voidRequest = type.GetInterfaces().Contains(typeof(IRequest));

        Type[] genericTypes = !voidRequest
            ? [type, type.GetGenericArgumentsFromImplementedInterfaces(typeof(IRequest<>))[0]]
            : [type];

        // todo add patch method support
        Type handlerType = attribute!.Method switch
        {
            HttpMethod.Get when voidRequest => typeof(RouteHandlers.GetWithoutResponse<>),
            HttpMethod.Get => typeof(RouteHandlers.GetWithResponse<,>),
            HttpMethod.Post or HttpMethod.Put or HttpMethod.Delete when voidRequest =>
                typeof(RouteHandlers.PostPutDeleteWithoutResponse<>),
            HttpMethod.Post or HttpMethod.Put or HttpMethod.Delete =>
                typeof(RouteHandlers.PostPutDeleteWithResponse<,>),
            _ => throw new InvalidOperationException(
                $"Only GET, PUT, POST, DELETE methods are allowed to be used. Requested method: {attribute.Method}")
        };

        // todo update validation mechanism
        string methodName = _useValidation ? RouteHandlers.Handler : RouteHandlers.HandlerWithoutValidation;

        Type genericHandlerType = handlerType.MakeGenericType(genericTypes);

        MethodInfo method = genericHandlerType.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);

        var invoke = (Delegate)method?.Invoke(null, [attribute]);

        return invoke;
    }

    private void MapApiRoute(IEndpointRouteBuilder routeBuilder, ApiRequestAttribute attribute, Delegate requestDelegate)
    {
        bool authorize = attribute.Authorize && _useAuth;

        // todo add patch method support
        RouteHandlerBuilder routeHandlerBuilder = attribute.Method switch
        {
            HttpMethod.Get => routeBuilder.MapGet(attribute.Path, requestDelegate),
            HttpMethod.Post => routeBuilder.MapPost(attribute.Path, requestDelegate),
            HttpMethod.Put => routeBuilder.MapPut(attribute.Path, requestDelegate),
            HttpMethod.Delete => routeBuilder.MapDelete(attribute.Path, requestDelegate),
            _ => throw new InvalidOperationException(
                $"Only GET, PUT, POST, DELETE methods are allowed to be used. Requested method: {attribute.Method}")
        };

        if (!authorize)
            return;

        if (attribute.AllowedRoles.Length > 0)
        {
            routeHandlerBuilder.RequireAuthorization(policyBuilder => policyBuilder.RequireRole(attribute.AllowedRoles));
        }

        if (!string.IsNullOrWhiteSpace(attribute.PolicyName))
        {
            routeHandlerBuilder.RequireAuthorization(attribute.PolicyName);
        }
    }
}