using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace car.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "UserInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpire",
                table: "UserInfo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "UserInfo",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ResetToken", "ResetTokenExpire" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "UserInfo");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpire",
                table: "UserInfo");
        }
    }
}
