using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Esh3arTech.Migrations
{
    /// <inheritdoc />
    public partial class ClmPlanIdAddedToUsersAndUserPlanTblAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditionId",
                table: "AbpTenants");

            migrationBuilder.AddColumn<string>(
                name: "PlanId",
                table: "AbpUsers",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EtUserPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiringPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DailyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WeeklyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlayPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AnnualPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TrialDayCount = table.Column<int>(type: "int", nullable: true),
                    WaitingDayAfterExpire = table.Column<int>(type: "int", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtUserPlans", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EtUserPlans");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "AbpUsers");

            migrationBuilder.AddColumn<string>(
                name: "EditionId",
                table: "AbpTenants",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);
        }
    }
}
