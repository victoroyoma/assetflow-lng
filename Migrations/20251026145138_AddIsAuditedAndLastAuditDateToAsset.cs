using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace buildone.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAuditedAndLastAuditDateToAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAudited",
                table: "Assets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAuditDate",
                table: "Assets",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAudited",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "LastAuditDate",
                table: "Assets");
        }
    }
}
