using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Esh3arTech.Migrations
{
    /// <inheritdoc />
    public partial class AlterTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EtRegistrationRequest");

            migrationBuilder.CreateTable(
                name: "EtRegistretionRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OS = table.Column<byte>(type: "tinyint", nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Verified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MobileUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtRegistretionRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtRegistretionRequest_EtMobileUsers_MobileUserId",
                        column: x => x.MobileUserId,
                        principalTable: "EtMobileUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EtRegistretionRequest_MobileUserId",
                table: "EtRegistretionRequest",
                column: "MobileUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EtRegistretionRequest");

            migrationBuilder.CreateTable(
                name: "EtRegistrationRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MobileUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OS = table.Column<byte>(type: "tinyint", nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Verified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtRegistrationRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtRegistrationRequest_EtMobileUsers_MobileUserId",
                        column: x => x.MobileUserId,
                        principalTable: "EtMobileUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EtRegistrationRequest_MobileUserId",
                table: "EtRegistrationRequest",
                column: "MobileUserId");
        }
    }
}
