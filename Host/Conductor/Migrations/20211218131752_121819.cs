using Microsoft.EntityFrameworkCore.Migrations;

namespace Authenticator.Migrations
{
    public partial class _121819 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "turnUSER",
                table: "Clusters",
                newName: "TurnUser");

            migrationBuilder.RenameColumn(
                name: "turnPASSWORD",
                table: "Clusters",
                newName: "TurnPassword");

            migrationBuilder.RenameColumn(
                name: "turnIP",
                table: "Clusters",
                newName: "TurnIp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TurnUser",
                table: "Clusters",
                newName: "turnUSER");

            migrationBuilder.RenameColumn(
                name: "TurnPassword",
                table: "Clusters",
                newName: "turnPASSWORD");

            migrationBuilder.RenameColumn(
                name: "TurnIp",
                table: "Clusters",
                newName: "turnIP");
        }
    }
}
