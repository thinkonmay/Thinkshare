using Microsoft.EntityFrameworkCore.Migrations;

namespace Conductor.Migrations
{
    public partial class fix_rmss : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RemoteSessions_AspNetUsers_ClientId",
                table: "RemoteSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_RemoteSessions_Devices_SlaveID",
                table: "RemoteSessions");

            migrationBuilder.AlterColumn<int>(
                name: "SlaveID",
                table: "RemoteSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "RemoteSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RemoteSessions_AspNetUsers_ClientId",
                table: "RemoteSessions",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RemoteSessions_Devices_SlaveID",
                table: "RemoteSessions",
                column: "SlaveID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RemoteSessions_AspNetUsers_ClientId",
                table: "RemoteSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_RemoteSessions_Devices_SlaveID",
                table: "RemoteSessions");

            migrationBuilder.AlterColumn<int>(
                name: "SlaveID",
                table: "RemoteSessions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "RemoteSessions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_RemoteSessions_AspNetUsers_ClientId",
                table: "RemoteSessions",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RemoteSessions_Devices_SlaveID",
                table: "RemoteSessions",
                column: "SlaveID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
