using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Conductor.Migrations
{
    public partial class ScriptModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "ShellSession",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "modelID",
                table: "ShellSession",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScriptModels",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Script = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptModels", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShellSession_modelID",
                table: "ShellSession",
                column: "modelID");

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_ScriptModels_modelID",
                table: "ShellSession",
                column: "modelID",
                principalTable: "ScriptModels",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_ScriptModels_modelID",
                table: "ShellSession");

            migrationBuilder.DropTable(
                name: "ScriptModels");

            migrationBuilder.DropIndex(
                name: "IX_ShellSession_modelID",
                table: "ShellSession");

            migrationBuilder.DropColumn(
                name: "modelID",
                table: "ShellSession");

            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "ShellSession",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
