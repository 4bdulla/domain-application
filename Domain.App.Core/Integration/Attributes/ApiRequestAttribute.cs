using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Domain.App.Core.Integration.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ApiRequestAttribute : Attribute, IRouteTemplateProvider
{
    public ApiRequestAttribute(
        string path,
        HttpMethod method,
        bool authorize = false,
        params string[] allowedRoles)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));

        if (!Enum.IsDefined(typeof(HttpMethod), method))
            throw new InvalidEnumArgumentException(nameof(method), (int)method, typeof(HttpMethod));

        this.Path = path;
        this.Template = path;
        this.Name = path;
        this.Method = method;
        this.Authorize = authorize;
        this.AllowedRoles = string.Join(",", allowedRoles);

        if (this.Authorize && string.IsNullOrWhiteSpace(this.AllowedRoles))
            throw new InvalidOperationException(
                $"{nameof(this.AllowedRoles)} should be defined when {nameof(this.Authorize)} is true!");
    }

    public string Path { get; set; }
    public HttpMethod Method { get; set; }
    public bool Authorize { get; set; }
    public string AllowedRoles { get; set; }
    public string Template { get; }
    public int? Order => default;
    public string Name { get; }
}