using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReservationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    NumberOfPeople = table.Column<int>(type: "int", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Occasion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SeatingPreference = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SpecialRequests = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    BudgetPerGuest = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    EstimatedTotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "Id", "BudgetPerGuest", "CancelledAt", "CreatedAt", "CustomerName", "EstimatedTotal", "NumberOfPeople", "Occasion", "PhoneNumber", "ReservationDate", "SeatingPreference", "SpecialRequests", "UpdatedAt", "UserEmail", "UserId" },
                values: new object[] { new Guid("0b24b232-5d9e-4e8e-91b1-94ea0c6c0f4a"), 32m, null, new DateTime(2024, 12, 31, 12, 0, 0, 0, DateTimeKind.Utc), "Елена Сидорова", 128m, 4, "business", "+37360000003", new DateTime(2025, 1, 8, 12, 0, 0, 0, DateTimeKind.Utc), "quiet", "Проектор", new DateTime(2024, 12, 31, 12, 0, 0, 0, DateTimeKind.Utc), "elena@example.com", null });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "Id", "BudgetPerGuest", "CancelledAt", "CreatedAt", "CustomerName", "EstimatedTotal", "NumberOfPeople", "Occasion", "PhoneNumber", "ReservationDate", "SeatingPreference", "SpecialRequests", "Status", "UpdatedAt", "UserEmail", "UserId" },
                values: new object[,]
                {
                    { new Guid("2c792c55-79f2-4d20-a5b9-56664fee5b8b"), 25.5m, null, new DateTime(2024, 12, 29, 12, 0, 0, 0, DateTimeKind.Utc), "Мария Иванова", 51m, 2, "romantic", "+37360000001", new DateTime(2025, 1, 3, 12, 0, 0, 0, DateTimeKind.Utc), "window", "Свечи и цветы", "Confirmed", new DateTime(2024, 12, 30, 12, 0, 0, 0, DateTimeKind.Utc), "maria@example.com", null },
                    { new Guid("c7b75c6b-adbd-4872-83f9-82e927cbf97f"), 18m, null, new DateTime(2024, 12, 12, 12, 0, 0, 0, DateTimeKind.Utc), "Алексей Петров", 108m, 6, "birthday", "+37360000002", new DateTime(2024, 12, 27, 12, 0, 0, 0, DateTimeKind.Utc), "indoor", "Торт и шарики", "Completed", new DateTime(2024, 12, 28, 12, 0, 0, 0, DateTimeKind.Utc), "alexey@example.com", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationDate",
                table: "Reservations",
                column: "ReservationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Status",
                table: "Reservations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserEmail",
                table: "Reservations",
                column: "UserEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");
        }
    }
}
