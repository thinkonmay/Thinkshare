using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Authenticator.Migrations
{
    public partial class _1130 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "SignallingUrl",
                table: "RemoteSessions");

            migrationBuilder.DropColumn(
                name: "WorkerState",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DefaultSettingID",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "TURN",
                table: "Clusters",
                newName: "turnUSER");

            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "Devices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "turnIP",
                table: "Clusters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "turnPASSWORD",
                table: "Clusters",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "turnIP",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "turnPASSWORD",
                table: "Clusters");

            migrationBuilder.RenameColumn(
                name: "turnUSER",
                table: "Clusters",
                newName: "TURN");

            migrationBuilder.AddColumn<string>(
                name: "SignallingUrl",
                table: "RemoteSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "Devices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "WorkerState",
                table: "Devices",
                type: "text",
                nullable: true);

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
                    audioCodec = table.Column<int>(type: "integer", nullable: true),
                    device = table.Column<int>(type: "integer", nullable: true),
                    mode = table.Column<int>(type: "integer", nullable: true),
                    screenHeight = table.Column<int>(type: "integer", nullable: true),
                    screenWidth = table.Column<int>(type: "integer", nullable: true),
                    videoCodec = table.Column<int>(type: "integer", nullable: true)
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
    }
}
