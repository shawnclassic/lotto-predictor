using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PredictLottoNZ.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LottoDraws",
                columns: table => new
                {
                    Draw = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WinningNumber1 = table.Column<int>(type: "integer", nullable: false),
                    WinningNumber2 = table.Column<int>(type: "integer", nullable: false),
                    WinningNumber3 = table.Column<int>(type: "integer", nullable: false),
                    WinningNumber4 = table.Column<int>(type: "integer", nullable: false),
                    WinningNumber5 = table.Column<int>(type: "integer", nullable: false),
                    WinningNumber6 = table.Column<int>(type: "integer", nullable: false),
                    BonusNumber = table.Column<int>(type: "integer", nullable: false),
                    Powerball = table.Column<int>(type: "integer", nullable: false),
                    FromLast = table.Column<string>(type: "text", nullable: true),
                    OneToTen = table.Column<int>(type: "integer", nullable: true),
                    ElevenToTwenty = table.Column<int>(type: "integer", nullable: true),
                    TwentyOneToThirty = table.Column<int>(type: "integer", nullable: true),
                    ThirtyOneToForty = table.Column<int>(type: "integer", nullable: true),
                    Division1Prize = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Division1Winners = table.Column<int>(type: "integer", nullable: true),
                    Division2Prize = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Division2Winners = table.Column<int>(type: "integer", nullable: true),
                    Division3Prize = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Division3Winners = table.Column<int>(type: "integer", nullable: true),
                    Division4Prize = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Division4Winners = table.Column<int>(type: "integer", nullable: true),
                    Division5Prize = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Division5Winners = table.Column<int>(type: "integer", nullable: true),
                    Division6Prize = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Division6Winners = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LottoDraws", x => x.Draw);
                });

            migrationBuilder.CreateTable(
                name: "NumberCombinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number1 = table.Column<int>(type: "integer", nullable: false),
                    Number2 = table.Column<int>(type: "integer", nullable: false),
                    Number3 = table.Column<int>(type: "integer", nullable: false),
                    Number4 = table.Column<int>(type: "integer", nullable: false),
                    Number5 = table.Column<int>(type: "integer", nullable: false),
                    Number6 = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberCombinations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Predictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Number1 = table.Column<int>(type: "integer", nullable: false),
                    Number2 = table.Column<int>(type: "integer", nullable: false),
                    Number3 = table.Column<int>(type: "integer", nullable: false),
                    Number4 = table.Column<int>(type: "integer", nullable: false),
                    Number5 = table.Column<int>(type: "integer", nullable: false),
                    Number6 = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: true),
                    RawRequestPayload = table.Column<string>(type: "text", nullable: true),
                    RawResponsePayload = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predictions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LottoDraws_CreatedAt",
                table: "LottoDraws",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LottoDraws_Date",
                table: "LottoDraws",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_NumberCombinations_CreatedAt",
                table: "NumberCombinations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NumberCombinations_Numbers",
                table: "NumberCombinations",
                columns: new[] { "Number1", "Number2", "Number3", "Number4", "Number5", "Number6" });

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_CreatedAt",
                table: "Predictions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_Numbers",
                table: "Predictions",
                columns: new[] { "Number1", "Number2", "Number3", "Number4", "Number5", "Number6" });

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_Source",
                table: "Predictions",
                column: "Source");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LottoDraws");

            migrationBuilder.DropTable(
                name: "NumberCombinations");

            migrationBuilder.DropTable(
                name: "Predictions");
        }
    }
}
