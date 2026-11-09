
using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Models;

namespace StockPilot.Api.Data;

public sealed class StockPilotDbContext : DbContext
{
    public StockPilotDbContext(
        DbContextOptions<StockPilotDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductStock> ProductStocks => Set<ProductStock>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCompany(modelBuilder);
        ConfigureWarehouse(modelBuilder);
        ConfigureProduct(modelBuilder);
        ConfigureProductStock(modelBuilder);
        ConfigureStockTransfer(modelBuilder);
        ConfigureStockTransferItem(modelBuilder);
        ConfigureStockMovement(modelBuilder);
    }

    private static void ConfigureCompany(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.Phone).HasMaxLength(30);
        });
    }

    private static void ConfigureWarehouse(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasAlternateKey(x => new { x.CompanyId, x.Id });

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Address).HasMaxLength(500);

            entity.HasIndex(x => new { x.CompanyId, x.Code })
                .IsUnique();

            entity.HasOne(x => x.Company)
                .WithMany(x => x.Warehouses)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasAlternateKey(x => new { x.CompanyId, x.Id });

            entity.Property(x => x.SKU)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Description).HasMaxLength(1000);

            entity.Property(x => x.Unit)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.CostPrice).HasPrecision(18, 2);
            entity.Property(x => x.SellingPrice).HasPrecision(18, 2);

            entity.HasIndex(x => new { x.CompanyId, x.SKU })
                .IsUnique();

            entity.HasOne(x => x.Company)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Product_CostPrice_NonNegative",
                    "[CostPrice] >= 0");

                table.HasCheckConstraint(
                    "CK_Product_SellingPrice_NonNegative",
                    "[SellingPrice] >= 0");
            });
        });
    }

    private static void ConfigureProductStock(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductStock>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.RowVersion).IsRowVersion();

            entity.HasIndex(x => new
            {
                x.CompanyId,
                x.ProductId,
                x.WarehouseId
            }).IsUnique();

            entity.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_ProductStock_Quantity_NonNegative",
                    "[Quantity] >= 0"));

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductStocks)
                .HasForeignKey(x => new { x.CompanyId, x.ProductId })
                .HasPrincipalKey(x => new { x.CompanyId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Warehouse)
                .WithMany(x => x.ProductStocks)
                .HasForeignKey(x => new { x.CompanyId, x.WarehouseId })
                .HasPrincipalKey(x => new { x.CompanyId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureStockTransfer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockTransfer>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasAlternateKey(x => new { x.CompanyId, x.Id });

            entity.Property(x => x.TransferNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Notes).HasMaxLength(1000);

            entity.HasIndex(x => new { x.CompanyId, x.TransferNumber })
                .IsUnique();

            entity.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_StockTransfer_DifferentWarehouses",
                    "[SourceWarehouseId] <> [DestinationWarehouseId]"));

            entity.HasOne(x => x.SourceWarehouse)
                .WithMany(x => x.SourceTransfers)
                .HasForeignKey(x => new { x.CompanyId, x.SourceWarehouseId })
                .HasPrincipalKey(x => new { x.CompanyId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DestinationWarehouse)
                .WithMany(x => x.DestinationTransfers)
                .HasForeignKey(x => new { x.CompanyId, x.DestinationWarehouseId })
                .HasPrincipalKey(x => new { x.CompanyId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureStockTransferItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockTransferItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasAlternateKey(x => new { x.CompanyId, x.Id, x.ProductId });
            entity.Property(x => x.Quantity).HasPrecision(18, 3);

            entity.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_StockTransferItem_Quantity_Positive",
                    "[Quantity] > 0"));

            entity.HasOne(x => x.StockTransfer)
                .WithMany(x => x.Items)
                .HasForeignKey(x => new { x.CompanyId, x.StockTransferId })
                .HasPrincipalKey(x => new { x.CompanyId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.StockTransferItems)
                .HasForeignKey(x => new { x.CompanyId, x.ProductId })
                .HasPrincipalKey(x => new { x.CompanyId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.StockTransferId, x.ProductId })
                .IsUnique();
        });
    }

    private static void ConfigureStockMovement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.Notes).HasMaxLength(1000);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.StockMovements)
                .HasForeignKey(x => new { x.CompanyId, x.ProductId })
                .HasPrincipalKey(x => new { x.CompanyId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Warehouse)
                .WithMany(x => x.StockMovements)
                .HasForeignKey(x => new { x.CompanyId, x.WarehouseId })
                .HasPrincipalKey(x => new { x.CompanyId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.StockTransferItem)
                .WithMany(x => x.StockMovements)
                .HasForeignKey(x => new { x.CompanyId, x.StockTransferItemId, x.ProductId })
                .HasPrincipalKey(x => new { x.CompanyId, x.Id, x.ProductId })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.CompanyId,
                x.ProductId,
                x.WarehouseId,
                x.CreatedAt
            });
            entity.HasIndex(x => new
            {
                x.StockTransferItemId,
                x.MovementType
            })
            .IsUnique()
            .HasFilter("[StockTransferItemId] IS NOT NULL");
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_StockMovement_Quantity_Positive",
                "[Quantity] > 0"
            ));
        });
    }
}
