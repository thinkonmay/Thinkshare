using Microsoft.EntityFrameworkCore.Migrations;

namespace Authenticator.Migrations
{
    public partial class _121713 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_Devices_WorkerID",
                table: "ShellSession");

            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_ScriptModels_ModelID",
                table: "ShellSession");

            migrationBuilder.DropIndex(
                name: "IX_ShellSession_WorkerID",
                table: "ShellSession");

            migrationBuilder.DropColumn(
                name: "Script",
                table: "ShellSession");

            migrationBuilder.DropColumn(
                name: "WorkerID",
                table: "ShellSession");

            migrationBuilder.AlterColumn<int>(
                name: "ModelID",
                table: "ShellSession",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "WorkerNodeID",
                table: "ShellSession",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShellSession_WorkerNodeID",
                table: "ShellSession",
                column: "WorkerNodeID");

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_Devices_WorkerNodeID",
                table: "ShellSession",
                column: "WorkerNodeID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_ScriptModels_ModelID",
                table: "ShellSession",
                column: "ModelID",
                principalTable: "ScriptModels",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_Devices_WorkerNodeID",
                table: "ShellSession");

            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_ScriptModels_ModelID",
                table: "ShellSession");

            migrationBuilder.DropIndex(
                name: "IX_ShellSession_WorkerNodeID",
                table: "ShellSession");

            migrationBuilder.DropColumn(
                name: "WorkerNodeID",
                table: "ShellSession");

            migrationBuilder.AlterColumn<int>(
                name: "ModelID",
                table: "ShellSession",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Script",
                table: "ShellSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkerID",
                table: "ShellSession",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShellSession_WorkerID",
                table: "ShellSession",
                column: "WorkerID");

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_Devices_WorkerID",
                table: "ShellSession",
                column: "WorkerID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_ScriptModels_ModelID",
                table: "ShellSession",
                column: "ModelID",
                principalTable: "ScriptModels",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
