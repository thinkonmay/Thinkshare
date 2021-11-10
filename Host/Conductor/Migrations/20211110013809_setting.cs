using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Conductor.Migrations
{
    public partial class setting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultSettingID",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DefaultSettings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    device = table.Column<int>(type: "integer", nullable: false),
                    audioCodec = table.Column<int>(type: "integer", nullable: false),
                    videoCodec = table.Column<int>(type: "integer", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    screenWidth = table.Column<int>(type: "integer", nullable: false),
                    screenHeight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultSettings", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DefaultSettingID",
                table: "AspNetUsers",
                column: "DefaultSettingID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_DefaultSettings_DefaultSettingID",
                table: "AspNetUsers",
                column: "DefaultSettingID",
                principalTable: "DefaultSettings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_DefaultSettings_DefaultSettingID",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DefaultSettings");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DefaultSettingID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultSettingID",
                table: "AspNetUsers");
        }
    }
}
