using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Authenticator.Migrations
{
    public partial class _120122 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clusters_AspNetUsers_UserAccountId",
                table: "Clusters");

            migrationBuilder.DropForeignKey(
                name: "FK_Clusters_Instances_instanceID",
                table: "Clusters");

            migrationBuilder.DropIndex(
                name: "IX_Clusters_UserAccountId",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "PortForward");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "Clusters");

            migrationBuilder.RenameColumn(
                name: "instanceID",
                table: "Clusters",
                newName: "InstanceID");

            migrationBuilder.RenameIndex(
                name: "IX_Clusters_instanceID",
                table: "Clusters",
                newName: "IX_Clusters_InstanceID");

            migrationBuilder.AddColumn<DateTime>(
                name: "End",
                table: "Instances",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Start",
                table: "Instances",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "Unregister",
                table: "Clusters",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Register",
                table: "Clusters",
                type: "timestamp without time zone",
                nullable: true,
                defaultValueSql: "current_timestamp",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "current_timestamp");

            migrationBuilder.AddColumn<int>(
                name: "OwnerID",
                table: "Clusters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_OwnerID",
                table: "Clusters",
                column: "OwnerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Clusters_AspNetUsers_OwnerID",
                table: "Clusters",
                column: "OwnerID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Clusters_Instances_InstanceID",
                table: "Clusters",
                column: "InstanceID",
                principalTable: "Instances",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clusters_AspNetUsers_OwnerID",
                table: "Clusters");

            migrationBuilder.DropForeignKey(
                name: "FK_Clusters_Instances_InstanceID",
                table: "Clusters");

            migrationBuilder.DropIndex(
                name: "IX_Clusters_OwnerID",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "End",
                table: "Instances");

            migrationBuilder.DropColumn(
                name: "Start",
                table: "Instances");

            migrationBuilder.DropColumn(
                name: "OwnerID",
                table: "Clusters");

            migrationBuilder.RenameColumn(
                name: "InstanceID",
                table: "Clusters",
                newName: "instanceID");

            migrationBuilder.RenameIndex(
                name: "IX_Clusters_InstanceID",
                table: "Clusters",
                newName: "IX_Clusters_instanceID");

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "PortForward",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Unregister",
                table: "Clusters",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Register",
                table: "Clusters",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "current_timestamp",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldDefaultValueSql: "current_timestamp");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Clusters_Instances_instanceID",
                table: "Clusters",
                column: "instanceID",
                principalTable: "Instances",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
