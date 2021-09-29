using Microsoft.EntityFrameworkCore.Migrations;

namespace Conductor.Migrations
{
    public partial class ScriptModelHis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_ScriptModels_ScriptModelID",
                table: "ShellSession");

            migrationBuilder.RenameColumn(
                name: "ScriptModelID",
                table: "ShellSession",
                newName: "ModelID");

            migrationBuilder.RenameIndex(
                name: "IX_ShellSession_ScriptModelID",
                table: "ShellSession",
                newName: "IX_ShellSession_ModelID");

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
                name: "FK_ShellSession_ScriptModels_ModelID",
                table: "ShellSession");

            migrationBuilder.RenameColumn(
                name: "ModelID",
                table: "ShellSession",
                newName: "ScriptModelID");

            migrationBuilder.RenameIndex(
                name: "IX_ShellSession_ModelID",
                table: "ShellSession",
                newName: "IX_ShellSession_ScriptModelID");

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_ScriptModels_ScriptModelID",
                table: "ShellSession",
                column: "ScriptModelID",
                principalTable: "ScriptModels",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
