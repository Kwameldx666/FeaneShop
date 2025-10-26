using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dishes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ImageBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageMimeType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PopularityScore = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Dishes",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "ImageBase64", "ImageMimeType", "IsAvailable", "IsFeatured", "Name", "PopularityScore", "Price", "UpdatedAt" },
                values: new object[] { new Guid("3f1a3c78-556c-4e8b-b9dd-5e72d6a5d82f"), "burger", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Juicy beef patty with cheddar cheese, lettuce, tomato and signature sauce.", null, null, true, true, "Classic Burger", 85, 12.50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Dishes",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "ImageBase64", "ImageMimeType", "IsAvailable", "Name", "PopularityScore", "Price", "UpdatedAt" },
                values: new object[] { new Guid("8c41c36c-c5c6-49df-b3b7-16e785ac6f75"), "pasta", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tagliatelle tossed in a creamy mushroom sauce with parmesan flakes.", null, null, true, "Creamy Mushroom Pasta", 74, 13.40m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Dishes",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "ImageBase64", "ImageMimeType", "IsAvailable", "IsFeatured", "Name", "PopularityScore", "Price", "UpdatedAt" },
                values: new object[] { new Guid("e5b28a3d-ed0c-4f4f-a26a-f879c0deb9c6"), "pizza", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Thin crust pizza topped with spicy chicken, mozzarella and jalapeños.", null, null, true, true, "Spicy Chicken Pizza", 92, 15.90m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_Category",
                table: "Dishes",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_IsAvailable",
                table: "Dishes",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_IsFeatured",
                table: "Dishes",
                column: "IsFeatured");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dishes");
        }
    }
}
