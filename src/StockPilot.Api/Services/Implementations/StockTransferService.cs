using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Data;
using StockPilot.Api.DTOs.StockTransfers;
using StockPilot.Api.Enums;
using StockPilot.Api.Models;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Services.Implementations;

public class StockTransferService : IStockTransferService
{
    private readonly StockPilotDbContext _context;

    public StockTransferService(StockPilotDbContext context)
    {
        _context = context;
    }

    public async Task<StockTransferResponseDto> CreateAsync(
        Guid companyId,
        CreateStockTransferDto dto)
    {
        if (dto.SourceWarehouseId == dto.DestinationWarehouseId)
            throw new InvalidOperationException(
                "Source and destination warehouses must be different.");

        if (dto.Items == null || dto.Items.Count == 0)
            throw new InvalidOperationException(
                "At least one product is required.");

        if (dto.Items.GroupBy(x => x.ProductId).Any(x => x.Count() > 1))
            throw new InvalidOperationException(
                "Duplicate products are not allowed in a transfer.");

        if (dto.Items.Any(x =>
            x.Quantity <= 0 ||
            x.Quantity > 999999999999999.999m ||
            decimal.Round(x.Quantity, 3) != x.Quantity))
        {
            throw new InvalidOperationException(
                "Quantities must be positive and have at most 3 decimal places.");
        }

        var company = await _context.Companies
            .FirstOrDefaultAsync(x => x.Id == companyId);

        if (company == null)
            throw new KeyNotFoundException("Company not found.");

        if (!company.IsActive)
            throw new InvalidOperationException("Company is inactive.");

        var warehouses = await _context.Warehouses
            .Where(x =>
                x.CompanyId == companyId &&
                (x.Id == dto.SourceWarehouseId ||
                 x.Id == dto.DestinationWarehouseId))
            .ToListAsync();

        var source = warehouses.FirstOrDefault(
            x => x.Id == dto.SourceWarehouseId);

        var destination = warehouses.FirstOrDefault(
            x => x.Id == dto.DestinationWarehouseId);

        if (source == null || destination == null)
            throw new KeyNotFoundException("Warehouse not found.");

        if (!source.IsActive || !destination.IsActive)
            throw new InvalidOperationException(
                "Both warehouses must be active.");

        var productIds = dto.Items
            .Select(x => x.ProductId)
            .ToList();

        var products = await _context.Products
            .Where(x =>
                x.CompanyId == companyId &&
                productIds.Contains(x.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
            throw new KeyNotFoundException(
                "One or more products were not found.");

        if (products.Any(x => !x.IsActive))
            throw new InvalidOperationException(
                "All products must be active.");

        var transfer = new StockTransfer
        {
            CompanyId = companyId,
            TransferNumber = $"TR-{Guid.NewGuid():N}",
            SourceWarehouseId = source.Id,
            DestinationWarehouseId = destination.Id,
            Status = StockTransferStatus.Draft,
            Notes = string.IsNullOrWhiteSpace(dto.Notes)
                ? null
                : dto.Notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            Items = dto.Items.Select(x => new StockTransferItem
            {
                CompanyId = companyId,
                ProductId = x.ProductId,
                Quantity = x.Quantity
            }).ToList()
        };

        _context.StockTransfers.Add(transfer);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(companyId, transfer.Id);
    }

    public async Task<List<StockTransferResponseDto>> GetAllAsync(
        Guid companyId,
        Guid? warehouseId = null)
    {
        var query = _context.StockTransfers
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId);

        if (warehouseId.HasValue)
        {
            var id = warehouseId.Value;

            query = query.Where(x =>
                x.SourceWarehouseId == id ||
                x.DestinationWarehouseId == id);
        }

        var transfers = await query
            .Include(x => x.SourceWarehouse)
            .Include(x => x.DestinationWarehouse)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return transfers.Select(MapTransfer).ToList();
    }

    public async Task<StockTransferResponseDto> GetByIdAsync(
        Guid companyId,
        Guid transferId,
        Guid? warehouseId = null)
    {
        var query = _context.StockTransfers
            .AsNoTracking()
            .Where(x =>
                x.CompanyId == companyId &&
                x.Id == transferId);

        if (warehouseId.HasValue)
        {
            var id = warehouseId.Value;

            query = query.Where(x =>
                x.SourceWarehouseId == id ||
                x.DestinationWarehouseId == id);
        }

        var transfer = await query
            .Include(x => x.SourceWarehouse)
            .Include(x => x.DestinationWarehouse)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync();

        if (transfer is null)
            throw new KeyNotFoundException("Stock transfer not found.");

        return MapTransfer(transfer);
    }

    public async Task<StockTransferResponseDto> PostAsync(
        Guid companyId,
        Guid transferId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

        try
        {
            var transfer = await _context.StockTransfers
                .AsNoTracking()
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.Id == transferId)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync();

            if (transfer == null)
                throw new KeyNotFoundException(
                    "Stock transfer not found.");

            if (transfer.Items.Count == 0)
                throw new InvalidOperationException(
                    "Cannot post an empty transfer.");

            var now = DateTime.UtcNow;

            var claimed = await _context.StockTransfers
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.Id == transferId &&
                    x.Status == StockTransferStatus.Draft)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.Status,
                        StockTransferStatus.Posted)
                    .SetProperty(
                        x => x.PostedAt,
                        now));

            if (claimed == 0)
                throw new InvalidOperationException(
                    "Transfer is no longer in Draft status.");

            var companyActive = await _context.Companies
                .AnyAsync(x => x.Id == companyId && x.IsActive);

            if (!companyActive)
                throw new InvalidOperationException(
                    "Company is inactive.");

            var warehousesActive = await _context.Warehouses
                .Where(x =>
                    x.CompanyId == companyId &&
                    (x.Id == transfer.SourceWarehouseId ||
                    x.Id == transfer.DestinationWarehouseId))
                .AllAsync(x => x.IsActive);

            if (!warehousesActive)
                throw new InvalidOperationException(
                    "Both warehouses must be active.");

            if (transfer.Items.Any(x => !x.Product.IsActive))
                throw new InvalidOperationException(
                    "All products must be active.");

            foreach (var item in transfer.Items)
            {
                var sourceStock = await _context.ProductStocks
                    .FirstOrDefaultAsync(x =>
                        x.CompanyId == companyId &&
                        x.ProductId == item.ProductId &&
                        x.WarehouseId == transfer.SourceWarehouseId);

                if (sourceStock == null ||
                    sourceStock.Quantity < item.Quantity)
                {
                    var available = sourceStock?.Quantity ?? 0;

                    throw new InvalidOperationException(
                        $"Insufficient stock for product {item.Product.SKU}. " +
                        $"Available: {available}, requested: {item.Quantity}.");
                }

                var destinationStock = await _context.ProductStocks
                    .FirstOrDefaultAsync(x =>
                        x.CompanyId == companyId &&
                        x.ProductId == item.ProductId &&
                        x.WarehouseId == transfer.DestinationWarehouseId);

                if (destinationStock == null)
                {
                    destinationStock = new ProductStock
                    {
                        CompanyId = companyId,
                        ProductId = item.ProductId,
                        WarehouseId = transfer.DestinationWarehouseId,
                        Quantity = 0,
                        UpdatedAt = now
                    };

                    _context.ProductStocks.Add(destinationStock);
                }

                sourceStock.Quantity -= item.Quantity;
                sourceStock.UpdatedAt = now;

                destinationStock.Quantity += item.Quantity;
                destinationStock.UpdatedAt = now;

                _context.StockMovements.AddRange(
                    new StockMovement
                    {
                        CompanyId = companyId,
                        ProductId = item.ProductId,
                        WarehouseId = transfer.SourceWarehouseId,
                        StockTransferItemId = item.Id,
                        MovementType = StockMovementType.TransferOut,
                        Quantity = item.Quantity,
                        CreatedAt = now,
                        Notes = $"Transfer {transfer.TransferNumber}"
                    },
                    new StockMovement
                    {
                        CompanyId = companyId,
                        ProductId = item.ProductId,
                        WarehouseId = transfer.DestinationWarehouseId,
                        StockTransferItemId = item.Id,
                        MovementType = StockMovementType.TransferIn,
                        Quantity = item.Quantity,
                        CreatedAt = now,
                        Notes = $"Transfer {transfer.TransferNumber}"
                    });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(companyId, transferId);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            throw new InvalidOperationException(
                "Stock was modified by another operation. Please retry.");
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlException &&
                (sqlException.Number == 2601 ||
                sqlException.Number == 2627))
        {
            await transaction.RollbackAsync();

            throw new InvalidOperationException(
                "A concurrent stock operation caused a database conflict. Please retry.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CancelAsync(
        Guid companyId,
        Guid transferId)
    {
        var affected = await _context.StockTransfers
            .Where(x =>
                x.CompanyId == companyId &&
                x.Id == transferId &&
                x.Status == StockTransferStatus.Draft)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.Status,
                    StockTransferStatus.Cancelled));

        if (affected == 0)
        {
            var exists = await _context.StockTransfers
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.Id == transferId);

            if (!exists)
                throw new KeyNotFoundException(
                    "Stock transfer not found.");

            throw new InvalidOperationException(
                "Only Draft transfers can be cancelled.");
        }
    }

    private static StockTransferResponseDto MapTransfer(
        StockTransfer transfer)
    {
        return new StockTransferResponseDto
        {
            Id = transfer.Id,
            CompanyId = transfer.CompanyId,
            TransferNumber = transfer.TransferNumber,

            SourceWarehouseId = transfer.SourceWarehouseId,
            SourceWarehouseName = transfer.SourceWarehouse.Name,

            DestinationWarehouseId = transfer.DestinationWarehouseId,
            DestinationWarehouseName = transfer.DestinationWarehouse.Name,

            Status = transfer.Status,
            Notes = transfer.Notes,
            CreatedAt = transfer.CreatedAt,
            PostedAt = transfer.PostedAt,

            Items = transfer.Items.Select(x =>
                new StockTransferItemResponseDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    SKU = x.Product.SKU,
                    ProductName = x.Product.Name,
                    Quantity = x.Quantity
                }).ToList()
        };
    }
}