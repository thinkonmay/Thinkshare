using Microsoft.EntityFrameworkCore.Migrations;

namespace Conductor.Migrations
{
    public partial class fix_circular_dep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommandLogs_Devices_SlaveID",
                table: "CommandLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_Devices_DeviceID",
                table: "ShellSession");

            migrationBuilder.DropIndex(
                name: "IX_CommandLogs_SlaveID",
                table: "CommandLogs");

            migrationBuilder.DropColumn(
                name: "ProcessID",
                table: "CommandLogs");

            migrationBuilder.DropColumn(
                name: "SlaveID",
                table: "CommandLogs");

            migrationBuilder.RenameColumn(
                name: "DeviceID",
                table: "ShellSession",
                newName: "SlaveID");

            migrationBuilder.RenameIndex(
                name: "IX_ShellSession_DeviceID",
                table: "ShellSession",
                newName: "IX_ShellSession_SlaveID");

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_Devices_SlaveID",
                table: "ShellSession",
                column: "SlaveID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_Devices_SlaveID",
                table: "ShellSession");

            migrationBuilder.RenameColumn(
                name: "SlaveID",
                table: "ShellSession",
                newName: "DeviceID");

            migrationBuilder.RenameIndex(
                name: "IX_ShellSession_SlaveID",
                table: "ShellSession",
                newName: "IX_ShellSession_DeviceID");

            migrationBuilder.AddColumn<int>(
                name: "ProcessID",
                table: "CommandLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SlaveID",
                table: "CommandLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommandLogs_SlaveID",
                table: "CommandLogs",
                column: "SlaveID");

            migrationBuilder.AddForeignKey(
                name: "FK_CommandLogs_Devices_SlaveID",
                table: "CommandLogs",
                column: "SlaveID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_Devices_DeviceID",
                table: "ShellSession",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
