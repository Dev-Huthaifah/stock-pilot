using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<StockPilotDbContext>(options =>
    options.UseSqlServer(connectionString));
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    name = "Stock Pilot",
    description = "Inventory and Products Management System",
    status = "Running",
    version = "1.0.0"
}));

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow
}));


app.MapGet("/api/health/database", async (
    StockPilotDbContext db,
    CancellationToken cancellationToken) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync(
            cancellationToken);

        return canConnect
            ? Results.Ok(new { status = "Connected" })
            : Results.Problem(
                title: "Database connection failed",
                statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (Microsoft.Data.SqlClient.SqlException ex)
    {
        app.Logger.LogError(
            ex,
            "SQL Server connection check failed.");

        return Results.Problem(
            title: "Database connection failed",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});


app.MapControllers();

app.Run();
