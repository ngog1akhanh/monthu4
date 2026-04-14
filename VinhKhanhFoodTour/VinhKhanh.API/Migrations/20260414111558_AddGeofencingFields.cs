using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VinhKhanh.API.Migrations
{
    /// <inheritdoc />
    public partial class AddGeofencingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPlayed",
                table: "POIs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Radius",
                table: "POIs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPlayed",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "Radius",
                table: "POIs");
        }
    }
}
