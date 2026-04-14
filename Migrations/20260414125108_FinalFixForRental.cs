using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace car.Migrations
{
    public partial class FinalFixForRental : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tablo zaten var olduğu için CreateTable komutunu sildik.
            // Sadece RealReturnDate kolonunu NULL yapılabilir hale getiriyoruz.
            migrationBuilder.AlterColumn<DateTime>(
                name: "RealReturnDate",
                table: "Rentals",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri almak istenirse tekrar zorunlu hale getirir.
            migrationBuilder.AlterColumn<DateTime>(
                name: "RealReturnDate",
                table: "Rentals",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}