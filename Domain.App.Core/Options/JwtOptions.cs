namespace Domain.App.Core.Options;

public class JwtOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public TimeSpan TokenTtl { get; set; } = TimeSpan.FromMinutes(60);
}