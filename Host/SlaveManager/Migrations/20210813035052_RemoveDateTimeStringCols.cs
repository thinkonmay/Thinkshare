using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SlaveManager.Migrations
{
    public partial class RemoveDateTimeStringCols : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "AspNetUsers",
                type: "timestamp without time zone",
                nullable: true,
                defaultValueSql: "current_timestamp",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldDefaultValueSql: "getUtcDate()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EndTime",
                table: "Sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartTime",
                table: "Sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExitTime",
                table: "SessionCoreExits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorTime",
                table: "GeneralErrors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Register",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Time",
                table: "CommandLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "AspNetUsers",
                type: "timestamp without time zone",
                nullable: true,
                defaultValueSql: "getUtcDate()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldDefaultValueSql: "current_timestamp");
        }
    }
}
