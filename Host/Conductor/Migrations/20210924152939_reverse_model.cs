using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Conductor.Migrations
{
    public partial class reverse_model : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_ScriptModels_modelID",
                table: "ShellSession");

            migrationBuilder.RenameColumn(
                name: "modelID",
                table: "ShellSession",
                newName: "ScriptModelID");

            migrationBuilder.RenameIndex(
                name: "IX_ShellSession_modelID",
                table: "ShellSession",
                newName: "IX_ShellSession_ScriptModelID");

            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "ScriptModels",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_ScriptModels_ScriptModelID",
                table: "ShellSession",
                column: "ScriptModelID",
                principalTable: "ScriptModels",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_ScriptModels_ScriptModelID",
                table: "ShellSession");

            migrationBuilder.RenameColumn(
                name: "ScriptModelID",
                table: "ShellSession",
                newName: "modelID");

            migrationBuilder.RenameIndex(
                name: "IX_ShellSession_ScriptModelID",
                table: "ShellSession",
                newName: "IX_ShellSession_modelID");

            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "ScriptModels",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_ScriptModels_modelID",
                table: "ShellSession",
                column: "modelID",
                principalTable: "ScriptModels",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
