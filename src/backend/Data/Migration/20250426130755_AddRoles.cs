using Microsoft.EntityFrameworkCore.Migrations;
using static TriviumParkingApp.Backend.Constants.Constants;


#nullable disable

namespace TriviumParkingApp.Backend.Data.Migration
{
    /// <inheritdoc />
    public partial class AddRoles : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Roles",
                type: "int",
                nullable: false);

            migrationBuilder.AlterColumn<int>(
                name: "IsPrioritySpace",
                table: "ParkingSpaces",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.InsertData(
               table: "ParkingLots",
               columns: new[] { "Name", "Address", "Priority" },
               values: new object[,]
               {
                    { ParkingLotNames.HQ,      "Westerlaan 111A, 8011 CA Zwolle", 0 },
                    { ParkingLotNames.Parkbee, "Stationsplein 14, 8011 CA Zwolle", 1 },
                    { ParkingLotNames.Public,  "Emmawijk 23, 8011 CM Zwolle", 2 }
               });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Name", "Priority" },
                values: new object[,]
                {
                    { RoleNames.Visitor,  0 },
                    { RoleNames.Manager,  1 },
                    { RoleNames.Employee, 2 }
                });

            // HQ – 2 gereserveerde plekken
            migrationBuilder.InsertData(
                table: "ParkingSpaces",
                columns: new[] { "SpaceNumber", "ParkingLotId", "IsPrioritySpace", "Notes" },
                values: new object[,]
                {
                    { "HQ-1",   1, 1,  "" },
                    { "HQ-2",   1, 1,  "" },
                });

            // Parkbee – 6 gereserveerde plekken
            migrationBuilder.InsertData(
                table: "ParkingSpaces",
                columns: new[] { "SpaceNumber", "ParkingLotId", "IsPrioritySpace", "Notes" },
                values: new object[,]
                {
                    { "Parkbee-1", 2, 2,  "" },
                    { "Parkbee-2", 2, 2,  "" },
                    { "Parkbee-3", 2, 2,  "" },
                    { "Parkbee-4", 2, 2,  "" },
                    { "Parkbee-5", 2, 2,  "" },
                    { "Parkbee-6", 2, 2,  "" },
                });

            // Public – onbeperkt
            migrationBuilder.InsertData(
               table: "ParkingSpaces",
               columns: new[] { "SpaceNumber", "ParkingLotId", "IsPrioritySpace", "Notes" },
               values: new object[,]
               {
                    { "Emmawijk-1", 3, 3,  "" },
                    { "Emmawijk-3", 3, 3,  "" },
                    { "Emmawijk-3", 3, 3,  "" },
                    { "Emmawijk-4", 3, 3,  "" },
                    { "Emmawijk-5", 3, 3,  "" },
                    { "Emmawijk-6", 3, 3,  "" },
                    { "Emmawijk-7", 3, 3,  "" },
                    { "Emmawijk-8", 3, 3,  "" },
                    { "Emmawijk-9", 3, 3,  "" },
                    { "Emmawijk-10", 3, 3,  "" },
                    { "Emmawijk-11", 3, 3,  "" },
                    { "Emmawijk-13", 3, 3,  "" },
               });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [dbo].[ParkingSpaces]");
            migrationBuilder.Sql("DELETE FROM [dbo].[ParkingLots]");
            migrationBuilder.Sql("DELETE FROM [dbo].[Roles]");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrioritySpace",
                table: "ParkingSpaces",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Roles");
        }
    }
}
