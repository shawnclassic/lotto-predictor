using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PredictLottoNZ.Migrations
{
    public partial class AddExternalServiceCallLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalServiceCallLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequestPayload = table.Column<string>(type: "text", nullable: false),
                    ResponsePayload = table.Column<string>(type: "text", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalServiceCallLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalServiceCallLogs_CreatedAt",
                table: "ExternalServiceCallLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalServiceCallLogs_ServiceName",
                table: "ExternalServiceCallLogs",
                column: "ServiceName");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalServiceCallLogs_ServiceName_CreatedAt",
                table: "ExternalServiceCallLogs",
                columns: new[] { "ServiceName", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalServiceCallLogs_Success",
                table: "ExternalServiceCallLogs",
                column: "Success");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalServiceCallLogs");
        }
    }
}
