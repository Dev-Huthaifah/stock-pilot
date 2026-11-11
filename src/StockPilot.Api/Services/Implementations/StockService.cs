using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StockPilot.Api.Data;
using StockPilot.Api.DTOs.Stock;
using StockPilot.Api.Enums;
using StockPilot.Api.Models;
using StockPilot.Api.Services.Interfaces;

namespace StockPilot.Api.Services.Implementations;

public class StockService : IStockService
{
    private readonly StockPilotDbContext _context;

    public StockService(StockPilotDbContext context)
    {
        _context = context;
    }

    public Task<StockResponseDto> StockInAsync(
        Guid companyId,
        StockInDto dto)
    {
        return ApplyMovementAsync(
            companyId,
            dto.ProductId,
            dto.WarehouseId,
            dto.Quantity,
            StockMovementType.StockIn,
            dto.Notes);
    }

    public Task<StockResponseDto> StockOutAsync(
        Guid companyId,
        StockOutDto dto)
    {
        return ApplyMovementAsync(
            companyId,
            dto.ProductId,
            dto.WarehouseId,
            -dto.Quantity,
            StockMovementType.StockOut,
            dto.Notes);
    }

    public Task<StockResponseDto> AdjustAsync(
        Guid companyId,
        StockAdjustmentDto dto)
    {
        if (dto.Difference == 0)
            throw new InvalidOperationException(
                "Adjustment difference cannot be zero.");

        var movementType = dto.Difference > 0
            ? StockMovementType.AdjustmentIn
            : StockMovementType.AdjustmentOut;

        return ApplyMovementAsync(
            companyId,
            dto.ProductId,
            dto.WarehouseId,
            dto.Difference,
            movementType,
            dto.Notes);
    }

    private async Task<StockResponseDto> ApplyMovementAsync(
        Guid companyId,
        Guid productId,
        Guid warehouseId,
        decimal difference,
        StockMovementType movementType,
        string? notes)
    {
        if (difference == 0)
            throw new InvalidOperationException(
                "Movement quantity cannot be zero.");

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(x => x.Id == companyId);

            if (company == null)
                throw new KeyNotFoundException("Company not found.");

            if (!company.IsActive)
                throw new InvalidOperationException(
                    "Company is inactive.");

            var product = await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == productId &&
                    x.CompanyId == companyId);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            if (!product.IsActive)
                throw new InvalidOperationException(
                    "Product is inactive.");

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.Id == warehouseId &&
                    x.CompanyId == companyId);

            if (warehouse == null)
                throw new KeyNotFoundException("Warehouse not found.");

            if (!warehouse.IsActive)
                throw new InvalidOperationException(
                    "Warehouse is inactive.");

            var stock = await _context.ProductStocks
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    x.ProductId == productId &&
                    x.WarehouseId == warehouseId);

            if (stock == null)
            {
                stock = new ProductStock
                {
                    CompanyId = companyId,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    Quantity = 0,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ProductStocks.Add(stock);
            }

            var newQuantity = stock.Quantity + difference;

            if (newQuantity < 0)
                throw new InvalidOperationException(
                    $"Insufficient stock. Available: {stock.Quantity}, requested decrease: {Math.Abs(difference)}.");

            stock.Quantity = newQuantity;
            stock.UpdatedAt = DateTime.UtcNow;

            var movement = new StockMovement
            {
                CompanyId = companyId,
                ProductId = productId,
                WarehouseId = warehouseId,
                MovementType = movementType,
                Quantity = Math.Abs(difference),
                Notes = string.IsNullOrWhiteSpace(notes)
                    ? null
                    : notes.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.StockMovements.Add(movement);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapStock(stock);
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
    }

    public async Task<List<StockResponseDto>> GetAllAsync(
        Guid companyId,
        Guid? warehouseId = null,
        Guid? productId = null)
    {
        var query = _context.ProductStocks
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId);

        if (warehouseId.HasValue)
            query = query.Where(x =>
                x.WarehouseId == warehouseId.Value);

        if (productId.HasValue)
            query = query.Where(x =>
                x.ProductId == productId.Value);

        var stocks = await query
            .OrderBy(x => x.WarehouseId)
            .ThenBy(x => x.ProductId)
            .ToListAsync();

        return stocks.Select(MapStock).ToList();
    }

    public async Task<List<StockMovementResponseDto>> GetMovementsAsync(
        Guid companyId,
        Guid? warehouseId = null,
        Guid? productId = null)
    {
        var query = _context.StockMovements
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId);

        if (warehouseId.HasValue)
            query = query.Where(x =>
                x.WarehouseId == warehouseId.Value);

        if (productId.HasValue)
            query = query.Where(x =>
                x.ProductId == productId.Value);

        var movements = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return movements.Select(MapMovement).ToList();
    }

    private static StockResponseDto MapStock(ProductStock stock)
    {
        return new StockResponseDto
        {
            Id = stock.Id,
            CompanyId = stock.CompanyId,
            ProductId = stock.ProductId,
            WarehouseId = stock.WarehouseId,
            Quantity = stock.Quantity,
            UpdatedAt = stock.UpdatedAt
        };
    }

    private static StockMovementResponseDto MapMovement(
        StockMovement movement)
    {
        return new StockMovementResponseDto
        {
            Id = movement.Id,
            CompanyId = movement.CompanyId,
            ProductId = movement.ProductId,
            WarehouseId = movement.WarehouseId,
            StockTransferItemId = movement.StockTransferItemId,
            MovementType = movement.MovementType,
            Quantity = movement.Quantity,
            CreatedAt = movement.CreatedAt,
            CreatedBy = movement.CreatedBy,
            Notes = movement.Notes
        };
    }
}