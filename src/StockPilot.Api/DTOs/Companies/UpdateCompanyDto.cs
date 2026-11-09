
namespace StockPilot.Api.DTOs.Companies;

public class UpdateCompanyDto
{
    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }
}
