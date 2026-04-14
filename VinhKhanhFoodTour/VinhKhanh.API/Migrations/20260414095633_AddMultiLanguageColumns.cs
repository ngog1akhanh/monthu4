using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhKhanh.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiLanguageColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description_JA",
                table: "POIs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description_ZH",
                table: "POIs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description_JA",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "Description_ZH",
                table: "POIs");
        }
    }
}
