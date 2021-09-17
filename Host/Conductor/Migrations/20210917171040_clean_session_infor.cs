using Microsoft.EntityFrameworkCore.Migrations;

namespace Conductor.Migrations
{
    public partial class clean_session_infor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientOffer",
                table: "RemoteSessions");

            migrationBuilder.DropColumn(
                name: "StunServer",
                table: "RemoteSessions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ClientOffer",
                table: "RemoteSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StunServer",
                table: "RemoteSessions",
                type: "text",
                nullable: true);
        }
    }
}
