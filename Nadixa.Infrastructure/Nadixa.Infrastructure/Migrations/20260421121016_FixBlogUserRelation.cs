using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nadixa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBlogUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Blogs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_AppUserId",
                table: "Blogs",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blogs_AspNetUsers_AppUserId",
                table: "Blogs",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blogs_AspNetUsers_AppUserId",
                table: "Blogs");

            migrationBuilder.DropIndex(
                name: "IX_Blogs_AppUserId",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Blogs");

            migrationBuilder.InsertData(
                table: "Blogs",
                columns: new[] { "Id", "BlogCategoryId", "Content", "CreateAt", "ImageUrl", "Title" },
                values: new object[,]
                {
                    { 1, 2, "Discover the best bags for your travel needs, combining style and functionality.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "/images/blog-01.jpg", "Best Bags for Travel" },
                    { 2, 2, "Discover the best bags for your travel needs, combining style and functionality.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "/images/blog-02.jpg", "How to style your bag" }
                });
        }
    }
}
