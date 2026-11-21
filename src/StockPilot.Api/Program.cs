using StockPilot.Api.Data.Seeding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using StockPilot.Api.Models;
using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Data;
using StockPilot.Api.Services.Interfaces;
using StockPilot.Api.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDataProtection();
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<StockPilotDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
builder.Services.AddDbContext<StockPilotDbContext>(options =>
    options.UseSqlServer(connectionString));
    var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT Secret is missing.");

    var jwtIssuer = builder.Configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("JWT Issuer is missing.");

    var jwtAudience = builder.Configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("JWT Audience is missing.");

    var jwtKey = Convert.FromBase64String(jwtSecret);

    if (jwtKey.Length < 32)
    {
        throw new InvalidOperationException(
            "JWT Secret must contain at least 32 bytes.");
    }

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
        })
        
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;

            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,

                    ValidateAudience = true,
                    ValidAudience = jwtAudience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKey),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),

                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var tokenUse = context.Principal?
                        .FindFirst("token_use")?.Value;

                    var isPasswordChangeEndpoint = string.Equals(
                        context.Request.Path.Value,
                        "/api/auth/change-temporary-password",
                        StringComparison.OrdinalIgnoreCase);

                    if (tokenUse == "password_change" &&
                        !isPasswordChangeEndpoint)
                    {
                        context.Fail(
                            "Password change token cannot access this endpoint.");
                    }

                    return Task.CompletedTask;
                }
            };
        });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlatformAdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("PlatformAdmin");
    });
});
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlatformCompanyService, PlatformCompanyService>();
var app = builder.Build();
await PlatformAdminBootstrapper.SeedAsync(
    app.Services,
    app.Configuration);
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<StockPilotDbContext>();

    await PermissionSeeder.SeedAsync(db);
}
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
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
