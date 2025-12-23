using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Esh3arTech.Migrations
{
    /// <inheritdoc />
    public partial class RmvBlobNameClmMessageAttch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlobName",
                table: "EtMessageAttachments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlobName",
                table: "EtMessageAttachments",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");
        }
    }
}
