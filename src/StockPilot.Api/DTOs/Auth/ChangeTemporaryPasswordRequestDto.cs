
using System.ComponentModel.DataAnnotations;

namespace StockPilot.Api.DTOs.Auth;

public sealed class ChangeTemporaryPasswordRequestDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    public string NewPassword { get; set; } = string.Empty;
}
