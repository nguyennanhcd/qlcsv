using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLCSV.Migrations
{
    /// <inheritdoc />
    public partial class FixBatchUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_batches_GraduationYear",
                table: "batches");

            migrationBuilder.CreateIndex(
                name: "IX_batches_GraduationYear_Name",
                table: "batches",
                columns: new[] { "GraduationYear", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_batches_GraduationYear_Name",
                table: "batches");

            migrationBuilder.CreateIndex(
                name: "IX_batches_GraduationYear",
                table: "batches",
                column: "GraduationYear",
                unique: true);
        }
    }
}
