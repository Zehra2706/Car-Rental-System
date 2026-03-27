using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace car.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Licences",
                columns: new[] { "Id", "Date", "LicenceNumber", "Score", "UserId" },
                values: new object[] { 1, new DateTime(2024, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "L123456789", 100, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Licences",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
