using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.App.Core.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.Jwt.Core.Interfaces;

namespace Domain.App.Core.Auth;

public class JwtGenerator
{
    private readonly IJwtService _jwt;
    private readonly AuthOptions _options;

    public JwtGenerator(IJwtService jwt, IOptions<AuthOptions> options)
    {
        _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }


    public async Task<string> GenerateToken(string email, IEnumerable<string> roles)
    {
        var claims = new ClaimsIdentity();

        claims.AddClaims(new Claim[]
        {
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, string.Join(",", roles.ToArray()))
        });

        var tokenHandler = new JwtSecurityTokenHandler();

        SecurityToken token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _options.Jwt.Issuer,
            Audience = _options.Jwt.Audience,
            Subject = claims,
            Expires = DateTime.UtcNow.Add(_options.Jwt.TokenTtl),
            NotBefore = DateTime.UtcNow,
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = await _jwt.GetCurrentSigningCredentials()
        });

        return tokenHandler.WriteToken(token);
    }
}