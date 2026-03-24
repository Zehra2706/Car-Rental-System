using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace car.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "DailyPrice",
                table: "Cars");

            migrationBuilder.RenameColumn(
                name: "ColorId",
                table: "Cars",
                newName: "Color");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsInsured",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "IsInsured",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Cars");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "Cars",
                newName: "ColorId");

            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "Cars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DailyPrice",
                table: "Cars",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
