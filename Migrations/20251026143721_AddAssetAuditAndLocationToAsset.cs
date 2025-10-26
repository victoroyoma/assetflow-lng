using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace buildone.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetAuditAndLocationToAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Assets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssetAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: true),
                    AssetTag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AuditedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsNewAsset = table.Column<bool>(type: "bit", nullable: false),
                    PreviousStatus = table.Column<int>(type: "int", nullable: true),
                    PreviousLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AuditSessionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetAudits_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAudits_AssetId",
                table: "AssetAudits",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAudits_AssetTag",
                table: "AssetAudits",
                column: "AssetTag");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAudits_AuditDate",
                table: "AssetAudits",
                column: "AuditDate");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAudits_AuditDate_AuditedBy",
                table: "AssetAudits",
                columns: new[] { "AuditDate", "AuditedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAudits_AuditSessionId",
                table: "AssetAudits",
                column: "AuditSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetAudits");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Assets");
        }
    }
}
