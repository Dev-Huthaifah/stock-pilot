
using Microsoft.EntityFrameworkCore;

namespace StockPilot.Api.Data;

public sealed class StockPilotDbContext : DbContext
{
    public StockPilotDbContext(
        DbContextOptions<StockPilotDbContext> options)
        : base(options)
    {
    }
}
