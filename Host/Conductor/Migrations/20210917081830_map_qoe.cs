using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Conductor.Migrations
{
    public partial class map_qoe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QoEID",
                table: "RemoteSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QoE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScreenWidth = table.Column<int>(type: "integer", nullable: false),
                    ScreenHeight = table.Column<int>(type: "integer", nullable: false),
                    AudioCodec = table.Column<int>(type: "integer", nullable: false),
                    VideoCodec = table.Column<int>(type: "integer", nullable: false),
                    QoEMode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QoE", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RemoteSessions_QoEID",
                table: "RemoteSessions",
                column: "QoEID");

            migrationBuilder.AddForeignKey(
                name: "FK_RemoteSessions_QoE_QoEID",
                table: "RemoteSessions",
                column: "QoEID",
                principalTable: "QoE",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RemoteSessions_QoE_QoEID",
                table: "RemoteSessions");

            migrationBuilder.DropTable(
                name: "QoE");

            migrationBuilder.DropIndex(
                name: "IX_RemoteSessions_QoEID",
                table: "RemoteSessions");

            migrationBuilder.DropColumn(
                name: "QoEID",
                table: "RemoteSessions");
        }
    }
}
