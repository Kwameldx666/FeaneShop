using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Isbn = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    PublishedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CoverImageBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImageMimeType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "CoverImageBase64", "CoverImageMimeType", "CreatedAt", "Description", "Genre", "IsAvailable", "Isbn", "Price", "PublishedOn", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("35b50074-8b94-4c95-9a6a-ebb0e0d49ac8"), "Andrew Hunt, David Thomas", null, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Journey to Mastery with practical advice for modern developers.", "software", true, "9780135957059", 39.90m, new DateTime(2019, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "The Pragmatic Programmer", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("8907a7a7-99f5-4cd7-8a74-8b7caf34a61c"), "Frank Herbert", null, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Epic science fiction saga set on the desert planet Arrakis.", "science-fiction", true, "9780441172719", 18.75m, new DateTime(1965, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dune", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c82d9d87-a518-47ee-b6f8-7af7e9ede91a"), "Robert C. Martin", null, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A Craftsman's Guide to Software Structure and Design.", "software", true, "9780134494166", 42.50m, new DateTime(2017, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Clean Architecture", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_Author",
                table: "Books",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Genre",
                table: "Books",
                column: "Genre");

            migrationBuilder.CreateIndex(
                name: "IX_Books_IsAvailable",
                table: "Books",
                column: "IsAvailable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
