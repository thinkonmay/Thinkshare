using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WorkerManager.Migrations
{
    public partial class second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clusters_UserAccount_ManagerAccountId",
                table: "Clusters");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkerNode_Clusters_ClusterID",
                table: "WorkerNode");

            migrationBuilder.DropTable(
                name: "UserAccount");

            migrationBuilder.DropTable(
                name: "DeviceCap");

            migrationBuilder.DropIndex(
                name: "IX_Clusters_ManagerAccountId",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "ManagerAccountId",
                table: "Clusters");

            migrationBuilder.RenameColumn(
                name: "ClusterID",
                table: "WorkerNode",
                newName: "WorkerClusterID");

            migrationBuilder.RenameIndex(
                name: "IX_WorkerNode_ClusterID",
                table: "WorkerNode",
                newName: "IX_WorkerNode_WorkerClusterID");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerNode_Clusters_WorkerClusterID",
                table: "WorkerNode",
                column: "WorkerClusterID",
                principalTable: "Clusters",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerNode_Clusters_WorkerClusterID",
                table: "WorkerNode");

            migrationBuilder.RenameColumn(
                name: "WorkerClusterID",
                table: "WorkerNode",
                newName: "ClusterID");

            migrationBuilder.RenameIndex(
                name: "IX_WorkerNode_WorkerClusterID",
                table: "WorkerNode",
                newName: "IX_WorkerNode_ClusterID");

            migrationBuilder.AddColumn<int>(
                name: "ManagerAccountId",
                table: "Clusters",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeviceCap",
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
                    table.PrimaryKey("PK_DeviceCap", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DefaultSettingID = table.Column<int>(type: "integer", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    Jobs = table.Column<string>(type: "text", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAccount_DeviceCap_DefaultSettingID",
                        column: x => x.DefaultSettingID,
                        principalTable: "DeviceCap",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_ManagerAccountId",
                table: "Clusters",
                column: "ManagerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_DefaultSettingID",
                table: "UserAccount",
                column: "DefaultSettingID");

            migrationBuilder.AddForeignKey(
                name: "FK_Clusters_UserAccount_ManagerAccountId",
                table: "Clusters",
                column: "ManagerAccountId",
                principalTable: "UserAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerNode_Clusters_ClusterID",
                table: "WorkerNode",
                column: "ClusterID",
                principalTable: "Clusters",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
