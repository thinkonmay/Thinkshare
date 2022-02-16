using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Authenticator.Migrations
{
    public partial class _130222 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShellSession");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShellSession",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModelID = table.Column<int>(type: "integer", nullable: true),
                    Output = table.Column<string>(type: "text", nullable: true),
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "current_timestamp"),
                    WorkerNodeID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShellSession", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ShellSession_Devices_WorkerNodeID",
                        column: x => x.WorkerNodeID,
                        principalTable: "Devices",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShellSession_ScriptModels_ModelID",
                        column: x => x.ModelID,
                        principalTable: "ScriptModels",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShellSession_ModelID",
                table: "ShellSession",
                column: "ModelID");

            migrationBuilder.CreateIndex(
                name: "IX_ShellSession_WorkerNodeID",
                table: "ShellSession",
                column: "WorkerNodeID");
        }
    }
}
