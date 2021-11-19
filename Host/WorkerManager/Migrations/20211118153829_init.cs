using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WorkerManager.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceCap",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    device = table.Column<int>(type: "integer", nullable: true),
                    audioCodec = table.Column<int>(type: "integer", nullable: true),
                    videoCodec = table.Column<int>(type: "integer", nullable: true),
                    mode = table.Column<int>(type: "integer", nullable: true),
                    screenWidth = table.Column<int>(type: "integer", nullable: true),
                    screenHeight = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCap", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "QoEs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScreenWidth = table.Column<int>(type: "integer", nullable: true),
                    ScreenHeight = table.Column<int>(type: "integer", nullable: true),
                    AudioCodec = table.Column<int>(type: "integer", nullable: true),
                    VideoCodec = table.Column<int>(type: "integer", nullable: true),
                    QoEMode = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QoEs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ScriptModel",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Script = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptModel", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    Jobs = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    DefaultSettingID = table.Column<int>(type: "integer", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    PrivateID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GlobalID = table.Column<int>(type: "integer", nullable: true),
                    PrivateIP = table.Column<string>(type: "text", nullable: true),
                    CPU = table.Column<string>(type: "text", nullable: true),
                    GPU = table.Column<string>(type: "text", nullable: true),
                    RAMcapacity = table.Column<int>(type: "integer", nullable: false),
                    OS = table.Column<string>(type: "text", nullable: true),
                    Register = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "current_timestamp"),
                    _workerState = table.Column<string>(type: "text", nullable: false),
                    agentUrl = table.Column<string>(type: "text", nullable: false),
                    coreUrl = table.Column<string>(type: "text", nullable: false),
                    RemoteToken = table.Column<string>(type: "text", nullable: true),
                    SignallingUrl = table.Column<string>(type: "text", nullable: true),
                    QoEID = table.Column<int>(type: "integer", nullable: true),
                    RAMCapacity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.PrivateID);
                    table.ForeignKey(
                        name: "FK_Devices_QoEs_QoEID",
                        column: x => x.QoEID,
                        principalTable: "QoEs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clusters",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Register = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "current_timestamp"),
                    Private = table.Column<bool>(type: "boolean", nullable: false),
                    ManagerAccountId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Clusters_UserAccount_ManagerAccountId",
                        column: x => x.ManagerAccountId,
                        principalTable: "UserAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkerNode",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false),
                    Register = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WorkerState = table.Column<string>(type: "text", nullable: true),
                    CPU = table.Column<string>(type: "text", nullable: true),
                    GPU = table.Column<string>(type: "text", nullable: true),
                    RAMcapacity = table.Column<int>(type: "integer", nullable: true),
                    OS = table.Column<string>(type: "text", nullable: true),
                    ClusterID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerNode", x => x.ID);
                    table.ForeignKey(
                        name: "FK_WorkerNode_Clusters_ClusterID",
                        column: x => x.ClusterID,
                        principalTable: "Clusters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CachedSession",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Script = table.Column<string>(type: "text", nullable: true),
                    Output = table.Column<string>(type: "text", nullable: true),
                    WorkerID = table.Column<int>(type: "integer", nullable: false),
                    ModelID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedSession", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CachedSession_ScriptModel_ModelID",
                        column: x => x.ModelID,
                        principalTable: "ScriptModel",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CachedSession_WorkerNode_WorkerID",
                        column: x => x.WorkerID,
                        principalTable: "WorkerNode",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CachedSession_ModelID",
                table: "CachedSession",
                column: "ModelID");

            migrationBuilder.CreateIndex(
                name: "IX_CachedSession_WorkerID",
                table: "CachedSession",
                column: "WorkerID");

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_ManagerAccountId",
                table: "Clusters",
                column: "ManagerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_QoEID",
                table: "Devices",
                column: "QoEID");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccount_DefaultSettingID",
                table: "UserAccount",
                column: "DefaultSettingID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerNode_ClusterID",
                table: "WorkerNode",
                column: "ClusterID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedSession");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "ScriptModel");

            migrationBuilder.DropTable(
                name: "WorkerNode");

            migrationBuilder.DropTable(
                name: "QoEs");

            migrationBuilder.DropTable(
                name: "Clusters");

            migrationBuilder.DropTable(
                name: "UserAccount");

            migrationBuilder.DropTable(
                name: "DeviceCap");
        }
    }
}
