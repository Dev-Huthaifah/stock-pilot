using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockPilot.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProductPriceConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_CostPrice_NonNegative",
                table: "Products",
                sql: "[CostPrice] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_SellingPrice_NonNegative",
                table: "Products",
                sql: "[SellingPrice] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_CostPrice_NonNegative",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_SellingPrice_NonNegative",
                table: "Products");
        }
    }
}
