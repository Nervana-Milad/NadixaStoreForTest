using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nadixa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SomeEdits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Blogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Blogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Blogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BlogComments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BlogComments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BlogComments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BlogComments");
        }
    }
}
