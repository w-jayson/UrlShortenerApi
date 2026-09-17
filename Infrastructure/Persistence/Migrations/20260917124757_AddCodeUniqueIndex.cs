using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortenerApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ShortenedUrls",
                newName: "CreatedAtUtc");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ShortenedUrls",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ShortenedUrls_Code",
                table: "ShortenedUrls",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShortenedUrls_Code",
                table: "ShortenedUrls");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ShortenedUrls");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "ShortenedUrls",
                newName: "CreatedAt");
        }
    }
}
