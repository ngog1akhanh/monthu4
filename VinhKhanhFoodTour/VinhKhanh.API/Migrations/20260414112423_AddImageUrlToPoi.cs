using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhKhanh.API.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToPoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "POIs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "POIs");
        }
    }
}
