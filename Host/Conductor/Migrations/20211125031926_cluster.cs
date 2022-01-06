using Microsoft.EntityFrameworkCore.Migrations;

namespace Authenticator.Migrations
{
    public partial class cluster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Clusters_ManagedClusterID",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ManagedClusterID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ManagedClusterID",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "UserAccountId",
                table: "Clusters",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_UserAccountId",
                table: "Clusters",
                column: "UserAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clusters_AspNetUsers_UserAccountId",
                table: "Clusters",
                column: "UserAccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clusters_AspNetUsers_UserAccountId",
                table: "Clusters");

            migrationBuilder.DropIndex(
                name: "IX_Clusters_UserAccountId",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "Clusters");

            migrationBuilder.AddColumn<int>(
                name: "ManagedClusterID",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ManagedClusterID",
                table: "AspNetUsers",
                column: "ManagedClusterID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Clusters_ManagedClusterID",
                table: "AspNetUsers",
                column: "ManagedClusterID",
                principalTable: "Clusters",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
