using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockPilot.Api.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleCompanyPerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyMemberships_UserId_CompanyId",
                table: "CompanyMemberships");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMemberships_UserId",
                table: "CompanyMemberships",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyMemberships_UserId",
                table: "CompanyMemberships");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMemberships_UserId_CompanyId",
                table: "CompanyMemberships",
                columns: new[] { "UserId", "CompanyId" },
                unique: true);
        }
    }
}
