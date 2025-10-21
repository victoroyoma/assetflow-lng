using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace buildone.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStaticSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "admin-user-id" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Full system access and user management", true, "Administrator", "ADMINISTRATOR" },
                    { "2", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Standard user access with limited permissions", true, "User", "USER" },
                    { "3", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Asset management and imaging job access", true, "Technician", "TECHNICIAN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "EmployeeId", "FullName", "IsActive", "LastLoginAt", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "Phone", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "admin-user-id", 0, "STATIC-CONCURRENCY-STAMP-FOR-SEED", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@buildone.com", true, null, "System Administrator", true, null, false, null, "ADMIN@BUILDONE.COM", "ADMIN@BUILDONE.COM", "AQAAAAIAAYagAAAAEGFjOTg4YjAtNDY4Mi00ZjZkLWI5NjMtNzI4ZjY3YzQxN2Q5YOH/7Z0d0z2S0aGgKfUu3Q==", null, null, false, "STATIC-SECURITY-STAMP-FOR-SEED", false, "admin@buildone.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "admin-user-id" });
        }
    }
}
