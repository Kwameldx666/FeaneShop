using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Migrations
{
    public partial class InitialCreate : Migration
    {
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
                values: new object[,]
                {
                    { new Guid("3F1A3C78-556C-4E8B-B9DD-5E72D6A5D82F"), "burger", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), "Juicy beef patty with cheddar cheese, lettuce, tomato and signature sauce.", null, null, true, true, "Classic Burger", 85, 12.50m, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("E5B28A3D-ED0C-4F4F-A26A-F879C0DEB9C6"), "pizza", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), "Thin crust pizza topped with spicy chicken, mozzarella and jalapeños.", null, null, true, true, "Spicy Chicken Pizza", 92, 15.90m, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("8C41C36C-C5C6-49DF-B3B7-16E785AC6F75"), "pasta", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), "Tagliatelle tossed in a creamy mushroom sauce with parmesan flakes.", null, null, true, false, "Creamy Mushroom Pasta", 74, 13.40m, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                });

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dishes");
        }
    }
}
