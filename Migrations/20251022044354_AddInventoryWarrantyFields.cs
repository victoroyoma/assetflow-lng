using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace buildone.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryWarrantyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyEndDate",
                table: "Inventories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarrantyPeriodMonths",
                table: "Inventories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarrantyProvider",
                table: "Inventories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyStartDate",
                table: "Inventories",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WarrantyEndDate",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "WarrantyPeriodMonths",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "WarrantyProvider",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "WarrantyStartDate",
                table: "Inventories");
        }
    }
}
