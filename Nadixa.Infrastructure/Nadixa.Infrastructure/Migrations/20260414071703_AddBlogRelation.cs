using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nadixa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlogId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_BlogId",
                table: "Products",
                column: "BlogId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Blogs_BlogId",
                table: "Products",
                column: "BlogId",
                principalTable: "Blogs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Blogs_BlogId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_BlogId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BlogId",
                table: "Products");
        }
    }
}
