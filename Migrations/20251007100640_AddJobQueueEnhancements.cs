using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace buildone.Migrations
{
    /// <inheritdoc />
    public partial class AddJobQueueEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Assets_DepartmentId",
                table: "Assets");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "ImagingJobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "ImagingJobs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "ImagingJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "JobComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImagingJobId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsSystemGenerated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobComments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobComments_ImagingJobs_ImagingJobId",
                        column: x => x.ImagingJobId,
                        principalTable: "ImagingJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId_FullName",
                table: "Employees",
                columns: new[] { "DepartmentId", "FullName" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_DepartmentId_Status",
                table: "Assets",
                columns: new[] { "DepartmentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Status_AssignedEmployeeId",
                table: "Assets",
                columns: new[] { "Status", "AssignedEmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Status_CreatedAt",
                table: "Assets",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsActive_Email",
                table: "AspNetUsers",
                columns: new[] { "IsActive", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_JobComments_CreatedAt",
                table: "JobComments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobComments_EmployeeId",
                table: "JobComments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_JobComments_ImagingJobId",
                table: "JobComments",
                column: "ImagingJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobComments");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DepartmentId_FullName",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Assets_DepartmentId_Status",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_Status_AssignedEmployeeId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_Status_CreatedAt",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IsActive_Email",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "ImagingJobs");

            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutes",
                table: "ImagingJobs");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ImagingJobs");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_DepartmentId",
                table: "Assets",
                column: "DepartmentId");
        }
    }
}
