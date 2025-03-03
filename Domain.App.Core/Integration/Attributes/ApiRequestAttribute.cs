using System.ComponentModel;

using Microsoft.AspNetCore.Mvc.Routing;

using HttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;


namespace Domain.App.Core.Integration.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ApiRequestAttribute : Attribute, IRouteTemplateProvider
{
    public ApiRequestAttribute(
        string path,
        HttpMethod method,
        bool authorize = false,
        string policyName = null,
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
        this.PolicyName = policyName;
        this.Authorize = authorize;
        this.AllowedRoles = allowedRoles;

        if (!this.Authorize ||
            this.AllowedRoles is not null && this.AllowedRoles.Length != 0 && !string.IsNullOrWhiteSpace(this.PolicyName))
            return;

        const string message =
            $"{nameof(this.AllowedRoles)} or {nameof(this.PolicyName)} should be defined when {nameof(this.Authorize)} is true!";

        throw new InvalidOperationException(message);
    }

    public string Path { get; set; }
    public HttpMethod Method { get; set; }
    public bool Authorize { get; set; }
    public string PolicyName { get; set; }
    public string[] AllowedRoles { get; set; }
    public string Template { get; }
    public int? Order => null;
    public string Name { get; }
}