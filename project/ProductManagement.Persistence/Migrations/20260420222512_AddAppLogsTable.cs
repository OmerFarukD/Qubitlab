using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "__AppLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___AppLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX___AppLogs_CorrelationId",
                table: "__AppLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX___AppLogs_Level",
                table: "__AppLogs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX___AppLogs_Timestamp",
                table: "__AppLogs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__AppLogs");
        }
    }
}
