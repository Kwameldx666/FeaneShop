using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnalyticsService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalOrders = table.Column<int>(type: "int", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageOrderValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompletedOrders = table.Column<int>(type: "int", nullable: false),
                    CancelledOrders = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalOrders = table.Column<int>(type: "int", nullable: false),
                    TotalQuantitySold = table.Column<int>(type: "int", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStatistics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_EntityType_EntityId",
                table: "AnalyticsEvents",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_EventType",
                table: "AnalyticsEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_Timestamp",
                table: "AnalyticsEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_UserId",
                table: "AnalyticsEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatistics_CreatedAt",
                table: "OrderStatistics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatistics_Date",
                table: "OrderStatistics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStatistics_Date",
                table: "ProductStatistics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStatistics_ProductId",
                table: "ProductStatistics",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStatistics_ProductId_Date",
                table: "ProductStatistics",
                columns: new[] { "ProductId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsEvents");

            migrationBuilder.DropTable(
                name: "OrderStatistics");

            migrationBuilder.DropTable(
                name: "ProductStatistics");
        }
    }
}
