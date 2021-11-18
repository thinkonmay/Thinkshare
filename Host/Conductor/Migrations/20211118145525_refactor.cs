using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Conductor.Migrations
{
    public partial class refactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RemoteSessions_Devices_SlaveID",
                table: "RemoteSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_RemoteSessions_QoE_QoEID",
                table: "RemoteSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_Devices_SlaveID",
                table: "ShellSession");

            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_ScriptModels_ModelID",
                table: "ShellSession");

            migrationBuilder.DropTable(
                name: "QoE");

            migrationBuilder.DropIndex(
                name: "IX_ShellSession_SlaveID",
                table: "ShellSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RemoteSessions",
                table: "RemoteSessions");

            migrationBuilder.DropIndex(
                name: "IX_RemoteSessions_QoEID",
                table: "RemoteSessions");

            migrationBuilder.DropColumn(
                name: "SlaveID",
                table: "ShellSession");

            migrationBuilder.DropColumn(
                name: "SessionSlaveID",
                table: "RemoteSessions");

            migrationBuilder.DropColumn(
                name: "QoEID",
                table: "RemoteSessions");

            migrationBuilder.RenameColumn(
                name: "SlaveID",
                table: "RemoteSessions",
                newName: "WorkerID");

            migrationBuilder.RenameColumn(
                name: "SessionClientID",
                table: "RemoteSessions",
                newName: "ID");

            migrationBuilder.RenameIndex(
                name: "IX_RemoteSessions_SlaveID",
                table: "RemoteSessions",
                newName: "IX_RemoteSessions_WorkerID");

            migrationBuilder.AlterColumn<int>(
                name: "ModelID",
                table: "ShellSession",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "ShellSession",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "WorkerID",
                table: "ShellSession",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "RemoteSessions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "ClusterID",
                table: "Devices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkerState",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RemoteSessions",
                table: "RemoteSessions",
                column: "ID");

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
                        name: "FK_Clusters_AspNetUsers_ManagerAccountId",
                        column: x => x.ManagerAccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShellSession_WorkerID",
                table: "ShellSession",
                column: "WorkerID");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ClusterID",
                table: "Devices",
                column: "ClusterID");

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_ManagerAccountId",
                table: "Clusters",
                column: "ManagerAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Clusters_ClusterID",
                table: "Devices",
                column: "ClusterID",
                principalTable: "Clusters",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RemoteSessions_Devices_WorkerID",
                table: "RemoteSessions",
                column: "WorkerID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_Devices_WorkerID",
                table: "ShellSession",
                column: "WorkerID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_ScriptModels_ModelID",
                table: "ShellSession",
                column: "ModelID",
                principalTable: "ScriptModels",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Clusters_ClusterID",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_RemoteSessions_Devices_WorkerID",
                table: "RemoteSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_Devices_WorkerID",
                table: "ShellSession");

            migrationBuilder.DropForeignKey(
                name: "FK_ShellSession_ScriptModels_ModelID",
                table: "ShellSession");

            migrationBuilder.DropTable(
                name: "Clusters");

            migrationBuilder.DropIndex(
                name: "IX_ShellSession_WorkerID",
                table: "ShellSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RemoteSessions",
                table: "RemoteSessions");

            migrationBuilder.DropIndex(
                name: "IX_Devices_ClusterID",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "WorkerID",
                table: "ShellSession");

            migrationBuilder.DropColumn(
                name: "ClusterID",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "WorkerState",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "WorkerID",
                table: "RemoteSessions",
                newName: "SlaveID");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "RemoteSessions",
                newName: "SessionClientID");

            migrationBuilder.RenameIndex(
                name: "IX_RemoteSessions_WorkerID",
                table: "RemoteSessions",
                newName: "IX_RemoteSessions_SlaveID");

            migrationBuilder.AlterColumn<int>(
                name: "ModelID",
                table: "ShellSession",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "ShellSession",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "SlaveID",
                table: "ShellSession",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SessionClientID",
                table: "RemoteSessions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "SessionSlaveID",
                table: "RemoteSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QoEID",
                table: "RemoteSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RemoteSessions",
                table: "RemoteSessions",
                columns: new[] { "SessionSlaveID", "SessionClientID" });

            migrationBuilder.CreateTable(
                name: "QoE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AudioCodec = table.Column<int>(type: "integer", nullable: true),
                    QoEMode = table.Column<int>(type: "integer", nullable: true),
                    ScreenHeight = table.Column<int>(type: "integer", nullable: true),
                    ScreenWidth = table.Column<int>(type: "integer", nullable: true),
                    VideoCodec = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QoE", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShellSession_SlaveID",
                table: "ShellSession",
                column: "SlaveID");

            migrationBuilder.CreateIndex(
                name: "IX_RemoteSessions_QoEID",
                table: "RemoteSessions",
                column: "QoEID");

            migrationBuilder.AddForeignKey(
                name: "FK_RemoteSessions_Devices_SlaveID",
                table: "RemoteSessions",
                column: "SlaveID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RemoteSessions_QoE_QoEID",
                table: "RemoteSessions",
                column: "QoEID",
                principalTable: "QoE",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_Devices_SlaveID",
                table: "ShellSession",
                column: "SlaveID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShellSession_ScriptModels_ModelID",
                table: "ShellSession",
                column: "ModelID",
                principalTable: "ScriptModels",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
