using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace car.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmailFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserInfo_UserId",
                table: "UserInfo");

            migrationBuilder.DropIndex(
                name: "IX_UserConnections_UserId",
                table: "UserConnections");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfo_UserId",
                table: "UserInfo",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserConnections_UserId",
                table: "UserConnections",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserInfo_UserId",
                table: "UserInfo");

            migrationBuilder.DropIndex(
                name: "IX_UserConnections_UserId",
                table: "UserConnections");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfo_UserId",
                table: "UserInfo",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConnections_UserId",
                table: "UserConnections",
                column: "UserId");
        }
    }
}
