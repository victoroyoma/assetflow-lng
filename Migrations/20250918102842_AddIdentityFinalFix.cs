using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace buildone.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityFinalFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEGFjOTg4YjAtNDY4Mi00ZjZkLWI5NjMtNzI4ZjY3YzQxN2Q5YOH/7Z0d0z2S0aGgKfUu3Q==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEOVK2QV9eUWf29Nb0cIuHXh69/0p9JTm10BCuGagcaXaA9GoQgVypJcw08HN9ofXsQ==");
        }
    }
}
