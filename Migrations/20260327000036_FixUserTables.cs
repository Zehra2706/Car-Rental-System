using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace car.Migrations
{
    /// <inheritdoc />
    public partial class FixUserTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Licences_UserId",
                table: "Licences");

            migrationBuilder.CreateIndex(
                name: "IX_Licences_UserId",
                table: "Licences",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Licences_UserId",
                table: "Licences");

            migrationBuilder.CreateIndex(
                name: "IX_Licences_UserId",
                table: "Licences",
                column: "UserId");
        }
    }
}
