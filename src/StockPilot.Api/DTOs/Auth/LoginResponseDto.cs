
namespace StockPilot.Api.DTOs.Auth;

public sealed class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTime ExpiresAt { get; set; }

    public bool RequiresPasswordChange { get; set; }
}
