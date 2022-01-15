using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Authenticator.Migrations
{
    public partial class _150122 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "RemoteSessions",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldDefaultValueSql: "current_timestamp");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "User",
                table: "Devices",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "User",
                table: "Devices");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "RemoteSessions",
                type: "timestamp without time zone",
                nullable: true,
                defaultValueSql: "current_timestamp",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
