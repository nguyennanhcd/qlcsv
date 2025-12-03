using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLCSV.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationTokenIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_users_EmailVerificationToken",
                table: "users",
                column: "EmailVerificationToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_EmailVerificationToken",
                table: "users");
        }
    }
}
