using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Esh3arTech.Migrations
{
    /// <inheritdoc />
    public partial class RmvFileSizeClmMessageAttch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "EtMessageAttachments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "EtMessageAttachments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
