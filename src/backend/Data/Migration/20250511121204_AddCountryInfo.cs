using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriviumParkingApp.Backend.Data.Migration
{
    /// <inheritdoc />
    public partial class AddCountryInfo : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ParkingRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryIsoCode",
                table: "ParkingRequests",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ParkingLots",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryIsoCode",
                table: "ParkingLots",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryIsoCode",
                table: "AspNetUsers",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "ParkingRequests");

            migrationBuilder.DropColumn(
                name: "CountryIsoCode",
                table: "ParkingRequests");

            migrationBuilder.DropColumn(
                name: "City",
                table: "ParkingLots");

            migrationBuilder.DropColumn(
                name: "CountryIsoCode",
                table: "ParkingLots");

            migrationBuilder.DropColumn(
                name: "CountryIsoCode",
                table: "AspNetUsers");
        }
    }
}
