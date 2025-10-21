using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace buildone.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ImagingJobs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ImagingJobs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ImagingJobs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ImagingJobs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ImagingJobs",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ImagingJobs",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ImagingJobs",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ImagingJobs",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Assets",
                columns: new[] { "Id", "AssetTag", "AssignedEmployeeId", "Brand", "CreatedAt", "DepartmentId", "DeploymentType", "ImagingType", "Model", "Notes", "PcId", "SerialNumber", "Status", "Type", "UpdatedAt", "WarrantyExpiry" },
                values: new object[,]
                {
                    { 3, "DESKTOP001", null, "Dell", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 1, "OptiPlex 7090", "Available for assignment", "PC003", "DL005678", 0, "Desktop", null, new DateTime(2026, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, "LAPTOP004", null, "HP", new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 1, "ZBook Studio", "High-performance workstation for design work", "PC006", "HP112233", 0, "Desktop", null, new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 8, "DESKTOP003", null, "HP", new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, 2, "EliteDesk 800", "Undergoing hardware upgrade", "PC008", "HP778899", 6, "Desktop", null, new DateTime(2024, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 9, "LAPTOP005", null, "Dell", new DateTime(2024, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 1, "Inspiron 7510", "Budget laptop for temporary assignments", "PC009", "DL334455", 0, "Desktop", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 10, "WORKSTATION001", null, "HP", new DateTime(2023, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 3, "Z4 G4", "End of life - scheduled for disposal", "PC010", "HP990011", 8, "Desktop", null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
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
                table: "Assets",
                columns: new[] { "Id", "AssetTag", "AssignedEmployeeId", "Brand", "CreatedAt", "DepartmentId", "DeploymentType", "ImagingType", "Model", "Notes", "PcId", "SerialNumber", "Status", "Type", "UpdatedAt", "WarrantyExpiry" },
                values: new object[,]
                {
                    { 1, "LAPTOP001", 1, "Dell", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 3, "Latitude 7420", "Primary laptop for IT Manager", "PC001", "DL001234", 5, "Desktop", null, new DateTime(2025, 12, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "LAPTOP002", 2, "HP", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 0, 2, "EliteBook 840", "Development laptop", "PC002", "HP001234", 3, "Desktop", null, new DateTime(2025, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, "LAPTOP003", 3, "HP", new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, 3, "ProBook 450", "HR Department laptop", "PC004", "HP005678", 3, "Desktop", null, new DateTime(2025, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, "DESKTOP002", 4, "Dell", new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), 3, 0, 2, "OptiPlex 5090", "Finance workstation", "PC005", "DL009876", 5, "Desktop", null, new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 7, "TABLET001", 5, "Dell", new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, 3, "Latitude 7320 Detachable", "Mobile device for presentations", "PC007", "DL445566", 3, "Desktop", null, new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "ImagingJobs",
                columns: new[] { "Id", "AssetId", "CompletedAt", "CreatedAt", "ImageVersion", "ImagingType", "Notes", "ScheduledAt", "StartedAt", "Status", "TechnicianId", "UpdatedAt" },
                values: new object[,]
                {
                    { 3, 3, null, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Windows11-2024.1", 1, "Waiting for hardware preparation", new DateTime(2024, 1, 5, 14, 0, 0, 0, DateTimeKind.Utc), null, 1, null, null },
                    { 6, 6, null, new DateTime(2024, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Workstation-2024.1", 1, "High-performance image with CAD software", new DateTime(2024, 1, 15, 13, 0, 0, 0, DateTimeKind.Utc), null, 0, 1, null },
                    { 7, 8, new DateTime(2024, 1, 12, 11, 45, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Windows11-2024.1", 2, "Hardware failure detected during imaging", new DateTime(2024, 1, 12, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 12, 11, 10, 0, 0, DateTimeKind.Utc), 4, 2, null },
                    { 8, 9, null, new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), "BasicOffice-2024.1", 1, "Cancelled due to change in requirements", new DateTime(2024, 1, 18, 14, 0, 0, 0, DateTimeKind.Utc), null, 5, 1, null },
                    { 1, 1, new DateTime(2024, 1, 2, 11, 30, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Windows11-2024.1", 3, "Successfully imaged with standard corporate image", new DateTime(2024, 1, 2, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 2, 9, 15, 0, 0, DateTimeKind.Utc), 3, 1, null },
                    { 2, 2, null, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), "DevEnvironment-2024.1", 2, "Installing development tools and environment", new DateTime(2024, 1, 3, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 3, 10, 5, 0, 0, DateTimeKind.Utc), 2, 2, null },
                    { 4, 4, new DateTime(2024, 1, 8, 10, 45, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), "HROffice-2024.1", 3, "HR suite with specialized applications", new DateTime(2024, 1, 8, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 8, 8, 10, 0, 0, DateTimeKind.Utc), 3, 1, null },
                    { 5, 5, new DateTime(2024, 1, 10, 12, 30, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc), "FinanceSecure-2024.1", 2, "Financial software with enhanced security", new DateTime(2024, 1, 10, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 10, 9, 5, 0, 0, DateTimeKind.Utc), 3, 2, null }
                });
        }
    }
}
