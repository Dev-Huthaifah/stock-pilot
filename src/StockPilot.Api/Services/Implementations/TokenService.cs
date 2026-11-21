using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using StockPilot.Api.DTOs.Auth;
using StockPilot.Api.Models;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Services.Implementations;

public sealed class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenResponseDto GenerateToken(
        CompanyMembership membership)
    {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException(
                "JWT Secret is missing.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer is missing.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience is missing.");

        var expirationMinutes = int.Parse(
            _configuration["Jwt:ExpirationMinutes"]
            ?? throw new InvalidOperationException(
                "JWT ExpirationMinutes is missing."));

        if (expirationMinutes <= 0)
            throw new InvalidOperationException(
                "JWT expiration must be greater than zero.");

        var keyBytes = Convert.FromBase64String(secret);

        if (keyBytes.Length < 32)
            throw new InvalidOperationException(
                "JWT Secret must contain at least 32 bytes.");

        var key = new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(expirationMinutes);

        var user = membership.User;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("membership_id", membership.Id.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email));
        }

        if (membership.WarehouseId.HasValue)
        {
            claims.Add(new Claim(
                "warehouse_id",
                membership.WarehouseId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        return new TokenResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler()
                .WriteToken(token),
            TokenType = "Bearer",
            ExpiresAt = expiresAt
        };
    }
    public TokenResponseDto GeneratePlatformAdminToken(
        ApplicationUser user)
    {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is missing.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is missing.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience is missing.");

        var expirationMinutes = int.Parse(
            _configuration["Jwt:ExpirationMinutes"]
            ?? throw new InvalidOperationException(
                "JWT ExpirationMinutes is missing."));

        if (expirationMinutes <= 0)
            throw new InvalidOperationException("Invalid JWT expiration.");

        var keyBytes = Convert.FromBase64String(secret);

        if (keyBytes.Length < 32)
            throw new InvalidOperationException("JWT key is too short.");

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(expirationMinutes);

        var key = new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("role", "PlatformAdmin"),
            new("account_type", "platform_admin")
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        return new TokenResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            TokenType = "Bearer",
            ExpiresAt = expiresAt
        };
    }

    
public TokenResponseDto GeneratePasswordChangeToken(ApplicationUser user)
{
    var secret = _configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("JWT Secret is missing.");

    var issuer = _configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("JWT Issuer is missing.");

    var audience = _configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("JWT Audience is missing.");

    var keyBytes = Convert.FromBase64String(secret);

    if (keyBytes.Length < 32)
        throw new InvalidOperationException("JWT key is too short.");

    var now = DateTime.UtcNow;
    var expiresAt = now.AddMinutes(5);

    var key = new SymmetricSecurityKey(keyBytes);
    var credentials = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new("token_use", "password_change")
    };

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        notBefore: now,
        expires: expiresAt,
        signingCredentials: credentials);

    return new TokenResponseDto
    {
        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
        TokenType = "Bearer",
        ExpiresAt = expiresAt
    };
}
}