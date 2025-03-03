using System.Reflection;

using Domain.App.Core.Integration.Attributes;
using Domain.App.Core.Utility;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Serilog;

using HttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;


namespace Domain.App.Core.Integration.Configurators;

// todo: enrich code to use swagger's annotations on response type
internal class ApiRequestConfigurator
{
    private readonly bool _useValidation;
    private readonly bool _useAuthInDevelopmentEnvironment;


    internal ApiRequestConfigurator(bool useValidation, bool useAuthInDevelopmentEnvironment)
    {
        _useValidation = useValidation;
        _useAuthInDevelopmentEnvironment = useAuthInDevelopmentEnvironment;

        Log.Debug(
            "{Configurator} starting with UseValidation = {UseValidation}, UseAuthInDevelopmentEnvironment = {UseAuthInDevelopmentEnvironment}",
            nameof(ApiRequestConfigurator),
            _useValidation,
            _useAuthInDevelopmentEnvironment);
    }


    internal void ConfigureApiRequestRoute(WebApplication app, Type type)
    {
        Log.Debug("configuring route for {Type}", type.Name);

        ApiRequestAttribute attribute = type.GetCustomAttribute<ApiRequestAttribute>();

        Log.Debug("found ApiRequestAttribute: {@Attribute}", attribute);

        Delegate invoke = this.GetRequestDelegate(type, attribute);

        Log.Debug("mapping {Method} to the {Path} with {Type} request", attribute!.Method, attribute!.Path, type.Name);

        this.MapApiRoute(app, attribute, invoke);
    }


    private Delegate GetRequestDelegate(Type type, ApiRequestAttribute attribute)
    {
        bool voidRequest = type.GetInterfaces().Contains(typeof(IRequest));

        Type[] genericTypes = !voidRequest
            ? new[] { type, type.GetGenericArgumentsFromImplementedInterfaces(typeof(IRequest<>))[0] }
            : new[] { type };

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

        string methodName = _useValidation ? RouteHandlers.Handler : RouteHandlers.HandlerWithoutValidation;

        Type genericHandlerType = handlerType.MakeGenericType(genericTypes);

        MethodInfo method = genericHandlerType.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);

        var invoke = (Delegate)method?.Invoke(null, new object[] { attribute });

        return invoke;
    }

    private void MapApiRoute(
        IEndpointRouteBuilder routeBuilder,
        ApiRequestAttribute attribute,
        Delegate requestDelegate)
    {
        bool authorize = attribute.Authorize && _useAuthInDevelopmentEnvironment;

        RouteHandlerBuilder routeHandlerBuilder = attribute.Method switch
        {
            HttpMethod.Get => routeBuilder.MapGet(attribute.Path, requestDelegate),
            HttpMethod.Post => routeBuilder.MapPost(attribute.Path, requestDelegate),
            HttpMethod.Put => routeBuilder.MapPut(attribute.Path, requestDelegate),
            HttpMethod.Delete => routeBuilder.MapDelete(attribute.Path, requestDelegate),
            _ => throw new InvalidOperationException(
                $"Only GET, PUT, POST, DELETE methods are allowed to be used. Requested method: {attribute.Method}")
        };

        if (authorize)
        {
            routeHandlerBuilder.RequireAuthorization();
        }
    }
}