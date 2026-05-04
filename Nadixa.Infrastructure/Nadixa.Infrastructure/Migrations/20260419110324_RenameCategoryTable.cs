//using System;
//using Microsoft.EntityFrameworkCore.Migrations;

//#nullable disable

//namespace Nadixa.Infrastructure.Migrations
//{
//    /// <inheritdoc />
//    public partial class RenameCategoryTable : Migration
//    {
//        /// <inheritdoc />
//        protected override void Up(MigrationBuilder migrationBuilder)
//        {
//            migrationBuilder.DropForeignKey(
//                name: "FK_Products_Categories_CategoryId",
//                table: "Products");

//            migrationBuilder.DropTable(
//                name: "Categories");

//            migrationBuilder.CreateTable(
//                name: "ProductCategories",
//                columns: table => new
//                {
//                    Id = table.Column<int>(type: "int", nullable: false)
//                        .Annotation("SqlServer:Identity", "1, 1"),
//                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
//                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
//                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
//                },
//                constraints: table =>
//                {
//                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
//                });

//            migrationBuilder.AddForeignKey(
//                name: "FK_Products_ProductCategories_CategoryId",
//                table: "Products",
//                column: "CategoryId",
//                principalTable: "ProductCategories",
//                principalColumn: "Id",
//                onDelete: ReferentialAction.Cascade);
//        }

//        /// <inheritdoc />
//        protected override void Down(MigrationBuilder migrationBuilder)
//        {
//            migrationBuilder.DropForeignKey(
//                name: "FK_Products_ProductCategories_CategoryId",
//                table: "Products");

//            migrationBuilder.DropTable(
//                name: "ProductCategories");

//            migrationBuilder.CreateTable(
//                name: "Categories",
//                columns: table => new
//                {
//                    Id = table.Column<int>(type: "int", nullable: false)
//                        .Annotation("SqlServer:Identity", "1, 1"),
//                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
//                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
//                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
//                },
//                constraints: table =>
//                {
//                    table.PrimaryKey("PK_Categories", x => x.Id);
//                });

//            migrationBuilder.AddForeignKey(
//                name: "FK_Products_Categories_CategoryId",
//                table: "Products",
//                column: "CategoryId",
//                principalTable: "Categories",
//                principalColumn: "Id",
//                onDelete: ReferentialAction.Cascade);
//        }
//    }
//}


using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nadixa.Infrastructure.Migrations
{
    public partial class RenameCategoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. فك الـ Foreign Key مؤقتًا
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            // 2. إعادة تسمية الجدول فقط (بدون حذف)
            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "ProductCategories");

            // 3. إعادة إضافة الـ Foreign Key بالاسم الجديد
            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // عكس العملية

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategories_CategoryId",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "ProductCategories",
                newName: "Categories");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}