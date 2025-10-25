using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace buildone.Migrations
{
    /// <inheritdoc />
    public partial class AddJobTypeToJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImagingJobs_Assets_AssetId",
                table: "ImagingJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_ImagingJobs_Employees_TechnicianId",
                table: "ImagingJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_JobAttachments_ImagingJobs_JobId",
                table: "JobAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_JobComments_ImagingJobs_ImagingJobId",
                table: "JobComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImagingJobs",
                table: "ImagingJobs");

            migrationBuilder.RenameTable(
                name: "ImagingJobs",
                newName: "Jobs");

            migrationBuilder.RenameIndex(
                name: "IX_ImagingJobs_TechnicianId",
                table: "Jobs",
                newName: "IX_Jobs_TechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_ImagingJobs_Status",
                table: "Jobs",
                newName: "IX_Jobs_Status");

            migrationBuilder.RenameIndex(
                name: "IX_ImagingJobs_ScheduledAt",
                table: "Jobs",
                newName: "IX_Jobs_ScheduledAt");

            migrationBuilder.RenameIndex(
                name: "IX_ImagingJobs_ImagingType",
                table: "Jobs",
                newName: "IX_Jobs_ImagingType");

            migrationBuilder.RenameIndex(
                name: "IX_ImagingJobs_AssetId_Status",
                table: "Jobs",
                newName: "IX_Jobs_AssetId_Status");

            migrationBuilder.AlterColumn<int>(
                name: "ImagingType",
                table: "Jobs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "JobType",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Jobs",
                table: "Jobs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobType",
                table: "Jobs",
                column: "JobType");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobType_Status",
                table: "Jobs",
                columns: new[] { "JobType", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_JobAttachments_Jobs_JobId",
                table: "JobAttachments",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobComments_Jobs_ImagingJobId",
                table: "JobComments",
                column: "ImagingJobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Assets_AssetId",
                table: "Jobs",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Employees_TechnicianId",
                table: "Jobs",
                column: "TechnicianId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobAttachments_Jobs_JobId",
                table: "JobAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_JobComments_Jobs_ImagingJobId",
                table: "JobComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Assets_AssetId",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Employees_TechnicianId",
                table: "Jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Jobs",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_JobType",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_JobType_Status",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobType",
                table: "Jobs");

            migrationBuilder.RenameTable(
                name: "Jobs",
                newName: "ImagingJobs");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_TechnicianId",
                table: "ImagingJobs",
                newName: "IX_ImagingJobs_TechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_Status",
                table: "ImagingJobs",
                newName: "IX_ImagingJobs_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_ScheduledAt",
                table: "ImagingJobs",
                newName: "IX_ImagingJobs_ScheduledAt");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_ImagingType",
                table: "ImagingJobs",
                newName: "IX_ImagingJobs_ImagingType");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_AssetId_Status",
                table: "ImagingJobs",
                newName: "IX_ImagingJobs_AssetId_Status");

            migrationBuilder.AlterColumn<int>(
                name: "ImagingType",
                table: "ImagingJobs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImagingJobs",
                table: "ImagingJobs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImagingJobs_Assets_AssetId",
                table: "ImagingJobs",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImagingJobs_Employees_TechnicianId",
                table: "ImagingJobs",
                column: "TechnicianId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JobAttachments_ImagingJobs_JobId",
                table: "JobAttachments",
                column: "JobId",
                principalTable: "ImagingJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobComments_ImagingJobs_ImagingJobId",
                table: "JobComments",
                column: "ImagingJobId",
                principalTable: "ImagingJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
