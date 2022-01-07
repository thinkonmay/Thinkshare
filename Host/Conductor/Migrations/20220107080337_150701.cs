using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Authenticator.Migrations
{
    public partial class _150701 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TurnIp",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "TurnPassword",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "TurnUser",
                table: "Clusters");

            migrationBuilder.AddColumn<bool>(
                name: "SelfHost",
                table: "Clusters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Unregister",
                table: "Clusters",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "instanceID",
                table: "Clusters",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EC2KeyPair",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PrivateKey = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EC2KeyPair", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Instances",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TurnUser = table.Column<string>(type: "text", nullable: true),
                    TurnPassword = table.Column<string>(type: "text", nullable: true),
                    Registered = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "current_timestamp"),
                    IPAdress = table.Column<string>(type: "text", nullable: true),
                    InstanceID = table.Column<string>(type: "text", nullable: true),
                    InstanceName = table.Column<string>(type: "text", nullable: true),
                    PrivateIP = table.Column<string>(type: "text", nullable: true),
                    keyPairID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instances", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Instances_EC2KeyPair_keyPairID",
                        column: x => x.keyPairID,
                        principalTable: "EC2KeyPair",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PortForward",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocalPort = table.Column<int>(type: "integer", nullable: false),
                    InstancePort = table.Column<int>(type: "integer", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: true),
                    Start = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ClusterInstanceID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortForward", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PortForward_Instances_ClusterInstanceID",
                        column: x => x.ClusterInstanceID,
                        principalTable: "Instances",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_instanceID",
                table: "Clusters",
                column: "instanceID");

            migrationBuilder.CreateIndex(
                name: "IX_Instances_keyPairID",
                table: "Instances",
                column: "keyPairID");

            migrationBuilder.CreateIndex(
                name: "IX_PortForward_ClusterInstanceID",
                table: "PortForward",
                column: "ClusterInstanceID");

            migrationBuilder.AddForeignKey(
                name: "FK_Clusters_Instances_instanceID",
                table: "Clusters",
                column: "instanceID",
                principalTable: "Instances",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clusters_Instances_instanceID",
                table: "Clusters");

            migrationBuilder.DropTable(
                name: "PortForward");

            migrationBuilder.DropTable(
                name: "Instances");

            migrationBuilder.DropTable(
                name: "EC2KeyPair");

            migrationBuilder.DropIndex(
                name: "IX_Clusters_instanceID",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "SelfHost",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "Unregister",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "instanceID",
                table: "Clusters");

            migrationBuilder.AddColumn<string>(
                name: "TurnIp",
                table: "Clusters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TurnPassword",
                table: "Clusters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TurnUser",
                table: "Clusters",
                type: "text",
                nullable: true);
        }
    }
}
