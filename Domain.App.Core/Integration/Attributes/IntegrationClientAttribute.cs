using Domain.App.Core.Options;


namespace Domain.App.Core.Integration.Attributes;

[AttributeUsage(AttributeTargets.Interface)]
public class IntegrationClientAttribute : Attribute
{
    public IntegrationClientAttribute(string baseAddress = default)
    {
        if (string.IsNullOrWhiteSpace(baseAddress))
            return;

        this.Options = new() { BaseAddress = baseAddress };
    }

    public ClientOptions Options { get; }
}