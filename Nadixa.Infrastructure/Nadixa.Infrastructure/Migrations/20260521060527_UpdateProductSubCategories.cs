using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nadixa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductSubCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ProductSubCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProductSubCategories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ProductSubCategories");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ProductSubCategories");
        }
    }
}
