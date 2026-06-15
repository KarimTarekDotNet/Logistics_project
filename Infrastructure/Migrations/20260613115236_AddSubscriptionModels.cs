using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_UserId_IsActive",
                table: "UserSubscriptions");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "SubscriptionPlans",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SubscriptionFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlanLimit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaxValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlanLimit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanLimit_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptionUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LimitCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PeriodStart = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PeriodEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptionUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionUsages_UserSubscriptions_UserSubscriptionId",
                        column: x => x.UserSubscriptionId,
                        principalTable: "UserSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlanFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionFeatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlanFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanFeatures_SubscriptionFeatures_SubscriptionFeatureId",
                        column: x => x.SubscriptionFeatureId,
                        principalTable: "SubscriptionFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanFeatures_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId_IsActive",
                table: "UserSubscriptions",
                columns: new[] { "UserId", "IsActive" },
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionFeatures_Code",
                table: "SubscriptionFeatures",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanFeatures_SubscriptionFeatureId",
                table: "SubscriptionPlanFeatures",
                column: "SubscriptionFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanFeatures_SubscriptionPlanId",
                table: "SubscriptionPlanFeatures",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanLimit_SubscriptionPlanId_Code",
                table: "SubscriptionPlanLimit",
                columns: new[] { "SubscriptionPlanId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionUsages_UserSubscriptionId_LimitCode_PeriodStart_PeriodEnd",
                table: "UserSubscriptionUsages",
                columns: new[] { "UserSubscriptionId", "LimitCode", "PeriodStart", "PeriodEnd" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionPlanFeatures");

            migrationBuilder.DropTable(
                name: "SubscriptionPlanLimit");

            migrationBuilder.DropTable(
                name: "UserSubscriptionUsages");

            migrationBuilder.DropTable(
                name: "SubscriptionFeatures");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_UserId_IsActive",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "SubscriptionPlans");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId_IsActive",
                table: "UserSubscriptions",
                columns: new[] { "UserId", "IsActive" });
        }
    }
}
