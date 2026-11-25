namespace StockPilot.Api.Services.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(
        Guid userId,
        Guid companyId,
        string permissionCode,
        Guid? warehouseId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasCompanyPermissionAsync(
        Guid userId,
        Guid companyId,
        string permissionCode,
        CancellationToken cancellationToken = default);
}
