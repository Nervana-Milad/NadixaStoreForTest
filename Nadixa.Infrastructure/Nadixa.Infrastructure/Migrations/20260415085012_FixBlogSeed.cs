using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nadixa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBlogSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Blogs",
                newName: "CreateAt");

            migrationBuilder.InsertData(
                table: "BlogCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Fashion" },
                    { 2, "Travel" },
                    { 3, "Care" }
                });

            migrationBuilder.InsertData(
                table: "Blogs",
                columns: new[] { "Id", "BlogCategoryId", "Content", "CreateAt", "ImageUrl", "Title" },
                values: new object[,]
                {
                    { 1, 2, "Discover the best bags for your travel needs, combining style and functionality.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "/images/blog-01.jpg", "Best Bags for Travel" },
                    { 2, 2, "Discover the best bags for your travel needs, combining style and functionality.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "/images/blog-02.jpg", "How to style your bag" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BlogCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BlogCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BlogCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "Blogs",
                newName: "CreatedAt");
        }
    }
}
