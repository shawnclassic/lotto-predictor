using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredictLottoNZ.Migrations
{
    public partial class AddMissingLottoDrawFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Low",
                table: "LottoDraws",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "High",
                table: "LottoDraws",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Odd",
                table: "LottoDraws",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Even",
                table: "LottoDraws",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Division7Prize",
                table: "LottoDraws",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Division7Winners",
                table: "LottoDraws",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Low",
                table: "LottoDraws");

            migrationBuilder.DropColumn(
                name: "High",
                table: "LottoDraws");

            migrationBuilder.DropColumn(
                name: "Odd",
                table: "LottoDraws");

            migrationBuilder.DropColumn(
                name: "Even",
                table: "LottoDraws");

            migrationBuilder.DropColumn(
                name: "Division7Prize",
                table: "LottoDraws");

            migrationBuilder.DropColumn(
                name: "Division7Winners",
                table: "LottoDraws");
        }
    }
}