using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Conductor.Migrations
{
    public partial class shell : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommandLogs");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ShellSession");

            migrationBuilder.DropColumn(
                name: "ProcessID",
                table: "ShellSession");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "ShellSession",
                newName: "Time");

            migrationBuilder.AddColumn<string>(
                name: "Output",
                table: "ShellSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Script",
                table: "ShellSession",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Output",
                table: "ShellSession");

            migrationBuilder.DropColumn(
                name: "Script",
                table: "ShellSession");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "ShellSession",
                newName: "StartTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "ShellSession",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcessID",
                table: "ShellSession",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CommandLogs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Command = table.Column<string>(type: "text", nullable: true),
                    ShellSessionID = table.Column<int>(type: "integer", nullable: true),
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandLogs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CommandLogs_ShellSession_ShellSessionID",
                        column: x => x.ShellSessionID,
                        principalTable: "ShellSession",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommandLogs_ShellSessionID",
                table: "CommandLogs",
                column: "ShellSessionID");
        }
    }
}
