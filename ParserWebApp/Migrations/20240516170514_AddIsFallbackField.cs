using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParserWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFallbackField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFallback",
                table: "LogEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFallback",
                table: "LogEntries");
        }
    }
}
