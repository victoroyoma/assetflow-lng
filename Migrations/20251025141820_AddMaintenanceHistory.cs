using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace buildone.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintenanceHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    TechnicianId = table.Column<int>(type: "int", nullable: true),
                    MaintenanceType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ActionsTaken = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PartsReplaced = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PartsCost = table.Column<decimal>(type: "decimal(18,2)", maxLength: 1000, nullable: true),
                    LaborHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StatusBefore = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusAfter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PerformedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    HasAttachments = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceHistory_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceHistory_Employees_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MaintenanceHistory_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceHistory_AssetId",
                table: "MaintenanceHistory",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceHistory_AssetId_MaintenanceDate",
                table: "MaintenanceHistory",
                columns: new[] { "AssetId", "MaintenanceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceHistory_JobId",
                table: "MaintenanceHistory",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceHistory_MaintenanceDate",
                table: "MaintenanceHistory",
                column: "MaintenanceDate");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceHistory_TechnicianId",
                table: "MaintenanceHistory",
                column: "TechnicianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceHistory");
        }
    }
}
