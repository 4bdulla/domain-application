namespace Domain.App.Core.Options;

public class AuthOptions
{
    public string Server { get; set; }
    public bool IsAuthServer { get; set; }
    public bool UseAuthInDevelopmentEnvironment { get; set; } = true;
    public JwtOptions Jwt { get; set; } = new();
}