using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Esh3arTech.Migrations
{
    /// <inheritdoc />
    public partial class AddFkToAttachmentAltrClmNullableMsgContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "EtMessageAttachments");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "EtMessageAttachments");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "EtMessageAttachments");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "EtMessageAttachments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EtMessageAttachments");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "EtMessageAttachments");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "EtMessageAttachments");

            migrationBuilder.AlterColumn<string>(
                name: "MessageContent",
                table: "EtMessages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "EtMessages",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "EtMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "MessageId",
                table: "EtMessageAttachments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtMessageAttachments_MessageId",
                table: "EtMessageAttachments",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_EtMessageAttachments_EtMessages_MessageId",
                table: "EtMessageAttachments",
                column: "MessageId",
                principalTable: "EtMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EtMessageAttachments_EtMessages_MessageId",
                table: "EtMessageAttachments");

            migrationBuilder.DropIndex(
                name: "IX_EtMessageAttachments_MessageId",
                table: "EtMessageAttachments");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "EtMessages");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "EtMessages");

            migrationBuilder.AlterColumn<string>(
                name: "MessageContent",
                table: "EtMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MessageId",
                table: "EtMessageAttachments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "EtMessageAttachments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "EtMessageAttachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "EtMessageAttachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "EtMessageAttachments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EtMessageAttachments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "EtMessageAttachments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "EtMessageAttachments",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
