namespace StockPilot.Api.Security;

public sealed record PermissionDefinition(
    string Code,
    string Name,
    string Category,
    string Description);

public static class PermissionCatalog
{
    public static readonly IReadOnlyList<PermissionDefinition> All =
    [
        new("products.view", "View Products", "Products",
            "View company products"),

        new("products.create", "Create Products", "Products",
            "Create new products"),

        new("products.update", "Update Products", "Products",
            "Update product information"),

        new("warehouses.view", "View Warehouses", "Warehouses",
            "View company warehouses"),

        new("warehouses.manage", "Manage Warehouses", "Warehouses",
            "Create, update and manage warehouses"),

        new("stock.view", "View Stock", "Stock",
            "View stock balances"),

        new("stock.in", "Stock In", "Stock",
            "Record incoming stock"),

        new("stock.out", "Stock Out", "Stock",
            "Record outgoing stock"),

        new("stock.adjust", "Adjust Stock", "Stock",
            "Adjust stock balances"),

        new("stock.transfer.view", "View Transfers", "Transfers",
            "View stock transfers"),

        new("stock.transfer.create", "Create Transfers", "Transfers",
            "Create stock transfers"),

        new("stock.transfer.post", "Post Transfers", "Transfers",
            "Approve and post stock transfers"),

        new("stock.transfer.cancel", "Cancel Transfers", "Transfers",
            "Cancel draft stock transfers"),

        new("employees.view", "View Employees", "Employees",
            "View company employees"),

        new("employees.create", "Create Employees", "Employees",
            "Create employee accounts"),

        new("employees.manage", "Manage Employees", "Employees",
            "Activate, deactivate and manage employees"),

        new("employees.permissions.manage", "Manage Employee Permissions",
            "Employees", "Manage individual employee permission overrides"),

        new("reports.view", "View Reports", "Reports",
            "View company reports")
    ];
}