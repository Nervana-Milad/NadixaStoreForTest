using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nadixa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseEntityFieldsToBlogAndBlogComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "BlogCategories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BlogCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BlogCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "BlogCategories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null });

            migrationBuilder.UpdateData(
                table: "BlogCategories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null });

            migrationBuilder.UpdateData(
                table: "BlogCategories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "BlogCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BlogCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BlogCategories");
        }
    }
}
