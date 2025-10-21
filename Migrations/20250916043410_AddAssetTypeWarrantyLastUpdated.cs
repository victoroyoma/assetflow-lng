using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace buildone.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetTypeWarrantyLastUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetTag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PcId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WarrantyExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImagingType = table.Column<int>(type: "int", nullable: false),
                    DeploymentType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedEmployeeId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Assets_Employees_AssignedEmployeeId",
                        column: x => x.AssignedEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AssetHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    ActorId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FromValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ToValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetHistory_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetHistory_Employees_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ImagingJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    TechnicianId = table.Column<int>(type: "int", nullable: true),
                    ImagingType = table.Column<int>(type: "int", nullable: false),
                    ImageVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagingJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImagingJobs_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImagingJobs_Employees_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Assets",
                columns: new[] { "Id", "AssetTag", "AssignedEmployeeId", "Brand", "CreatedAt", "DepartmentId", "DeploymentType", "ImagingType", "Model", "Notes", "PcId", "SerialNumber", "Status", "Type", "UpdatedAt", "WarrantyExpiry" },
                values: new object[,]
                {
                    { 3, "DESKTOP001", null, "Dell", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 1, "OptiPlex 7090", "Available for assignment", "PC003", "DL005678", 0, "Desktop", null, null },
                    { 6, "LAPTOP004", null, "HP", new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 1, "ZBook Studio", "High-performance workstation for design work", "PC006", "HP112233", 0, "Desktop", null, null },
                    { 8, "DESKTOP003", null, "HP", new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, 2, "EliteDesk 800", "Undergoing hardware upgrade", "PC008", "HP778899", 6, "Desktop", null, null },
                    { 9, "LAPTOP005", null, "Dell", new DateTime(2024, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 1, "Inspiron 7510", "Budget laptop for temporary assignments", "PC009", "DL334455", 0, "Desktop", null, null },
                    { 10, "WORKSTATION001", null, "HP", new DateTime(2023, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 3, "Z4 G4", "End of life - scheduled for disposal", "PC010", "HP990011", 8, "Desktop", null, null }
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "IT", "Information Technology" },
                    { 2, "HR", "Human Resources" },
                    { 3, "FIN", "Finance" }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "DepartmentId", "Email", "FullName", "Phone", "Username" },
                values: new object[,]
                {
                    { 1, 1, "alice.wilson@company.com", "Alice Wilson", "555-0101", "awilson" },
                    { 2, 1, "david.brown@company.com", "David Brown", "555-0102", "dbrown" },
                    { 3, 2, "sarah.davis@company.com", "Sarah Davis", "555-0103", "sdavis" },
                    { 4, 3, "michael.johnson@company.com", "Michael Johnson", "555-0104", "mjohnson" },
                    { 5, 2, "jennifer.lee@company.com", "Jennifer Lee", "555-0105", "jlee" }
                });

            migrationBuilder.InsertData(
                table: "ImagingJobs",
                columns: new[] { "Id", "AssetId", "CompletedAt", "CreatedAt", "ImageVersion", "ImagingType", "Notes", "ScheduledAt", "StartedAt", "Status", "TechnicianId", "UpdatedAt" },
                values: new object[] { 3, 3, null, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Windows11-2024.1", 1, "Waiting for hardware preparation", new DateTime(2024, 1, 5, 14, 0, 0, 0, DateTimeKind.Utc), null, 1, null, null });

            migrationBuilder.InsertData(
                table: "Assets",
                columns: new[] { "Id", "AssetTag", "AssignedEmployeeId", "Brand", "CreatedAt", "DepartmentId", "DeploymentType", "ImagingType", "Model", "Notes", "PcId", "SerialNumber", "Status", "Type", "UpdatedAt", "WarrantyExpiry" },
                values: new object[,]
                {
                    { 1, "LAPTOP001", 1, "Dell", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 3, "Latitude 7420", "Primary laptop for IT Manager", "PC001", "DL001234", 5, "Desktop", null, null },
                    { 2, "LAPTOP002", 2, "HP", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 0, 2, "EliteBook 840", "Development laptop", "PC002", "HP001234", 3, "Desktop", null, null },
                    { 4, "LAPTOP003", 3, "HP", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, 3, "ProBook 450", "HR Department laptop", "PC004", "HP005678", 3, "Desktop", null, null },
                    { 5, "DESKTOP002", 4, "Dell", new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), 3, 0, 2, "OptiPlex 5090", "Finance workstation", "PC005", "DL009876", 5, "Desktop", null, null },
                    { 7, "TABLET001", 5, "Dell", new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, 3, "Latitude 7320 Detachable", "Mobile device for presentations", "PC007", "DL445566", 3, "Desktop", null, null }
                });

            migrationBuilder.InsertData(
                table: "ImagingJobs",
                columns: new[] { "Id", "AssetId", "CompletedAt", "CreatedAt", "ImageVersion", "ImagingType", "Notes", "ScheduledAt", "StartedAt", "Status", "TechnicianId", "UpdatedAt" },
                values: new object[,]
                {
                    { 6, 6, null, new DateTime(2024, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Workstation-2024.1", 1, "High-performance image with CAD software", new DateTime(2024, 1, 15, 13, 0, 0, 0, DateTimeKind.Utc), null, 0, 1, null },
                    { 7, 8, new DateTime(2024, 1, 12, 11, 45, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Windows11-2024.1", 2, "Hardware failure detected during imaging", new DateTime(2024, 1, 12, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 12, 11, 10, 0, 0, DateTimeKind.Utc), 4, 2, null },
                    { 8, 9, null, new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), "BasicOffice-2024.1", 1, "Cancelled due to change in requirements", new DateTime(2024, 1, 18, 14, 0, 0, 0, DateTimeKind.Utc), null, 5, 1, null },
                    { 1, 1, new DateTime(2024, 1, 2, 11, 30, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Windows11-2024.1", 3, "Successfully imaged with standard corporate image", new DateTime(2024, 1, 2, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 2, 9, 15, 0, 0, DateTimeKind.Utc), 3, 1, null },
                    { 2, 2, null, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), "DevEnvironment-2024.1", 2, "Installing development tools and environment", new DateTime(2024, 1, 3, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 3, 10, 5, 0, 0, DateTimeKind.Utc), 2, 2, null },
                    { 4, 4, new DateTime(2024, 1, 8, 10, 45, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), "HROffice-2024.1", 3, "HR suite with specialized applications", new DateTime(2024, 1, 8, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 8, 8, 10, 0, 0, DateTimeKind.Utc), 3, 1, null },
                    { 5, 5, new DateTime(2024, 1, 10, 12, 30, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc), "FinanceSecure-2024.1", 2, "Financial software with enhanced security", new DateTime(2024, 1, 10, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 10, 9, 5, 0, 0, DateTimeKind.Utc), 3, 2, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetHistory_Action",
                table: "AssetHistory",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AssetHistory_ActorId",
                table: "AssetHistory",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetHistory_AssetId",
                table: "AssetHistory",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetHistory_CreatedAt",
                table: "AssetHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetTag",
                table: "Assets",
                column: "AssetTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssignedEmployeeId",
                table: "Assets",
                column: "AssignedEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Brand_Model",
                table: "Assets",
                columns: new[] { "Brand", "Model" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_DepartmentId",
                table: "Assets",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_PcId",
                table: "Assets",
                column: "PcId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_SerialNumber",
                table: "Assets",
                column: "SerialNumber",
                unique: true,
                filter: "[SerialNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Status",
                table: "Assets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_FullName",
                table: "Employees",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Username",
                table: "Employees",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImagingJobs_AssetId_Status",
                table: "ImagingJobs",
                columns: new[] { "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ImagingJobs_ImagingType",
                table: "ImagingJobs",
                column: "ImagingType");

            migrationBuilder.CreateIndex(
                name: "IX_ImagingJobs_ScheduledAt",
                table: "ImagingJobs",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_ImagingJobs_Status",
                table: "ImagingJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ImagingJobs_TechnicianId",
                table: "ImagingJobs",
                column: "TechnicianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetHistory");

            migrationBuilder.DropTable(
                name: "ImagingJobs");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
