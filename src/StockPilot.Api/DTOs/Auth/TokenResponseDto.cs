namespace StockPilot.Api.DTOs.Auth;

public sealed class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTime ExpiresAt { get; set; }
}