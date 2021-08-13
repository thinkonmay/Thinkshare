using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SlaveManager.Migrations
{
    public partial class AddedDateTimeCols : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Sessions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Sessions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExitTime",
                table: "SessionCoreExits",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ErrorTime",
                table: "GeneralErrors",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Register",
                table: "Devices",
                type: "timestamp without time zone",
                nullable: true,
                defaultValueSql: "current_timestamp");

            migrationBuilder.AddColumn<DateTime>(
                name: "Time",
                table: "CommandLogs",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "ExitTime",
                table: "SessionCoreExits");

            migrationBuilder.DropColumn(
                name: "ErrorTime",
                table: "GeneralErrors");

            migrationBuilder.DropColumn(
                name: "Register",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "CommandLogs");
        }
    }
}
