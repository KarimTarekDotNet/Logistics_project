using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditSubFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanLimit_SubscriptionPlans_SubscriptionPlanId",
                table: "SubscriptionPlanLimit");

            migrationBuilder.DropTable(
                name: "SubscriptionPlanFeatures");

            migrationBuilder.RenameColumn(
                name: "SubscriptionPlanId",
                table: "SubscriptionPlanLimit",
                newName: "SubscriptionFeatureId");

            migrationBuilder.RenameIndex(
                name: "IX_SubscriptionPlanLimit_SubscriptionPlanId_LimitCodeSubscription",
                table: "SubscriptionPlanLimit",
                newName: "IX_SubscriptionPlanLimit_SubscriptionFeatureId_LimitCodeSubscription");

            migrationBuilder.CreateTable(
                name: "SubscriptionFeatureSubscriptionPlan",
                columns: table => new
                {
                    FeaturesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionPlansId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionFeatureSubscriptionPlan", x => new { x.FeaturesId, x.SubscriptionPlansId });
                    table.ForeignKey(
                        name: "FK_SubscriptionFeatureSubscriptionPlan_SubscriptionFeatures_FeaturesId",
                        column: x => x.FeaturesId,
                        principalTable: "SubscriptionFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionFeatureSubscriptionPlan_SubscriptionPlans_SubscriptionPlansId",
                        column: x => x.SubscriptionPlansId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionFeatureSubscriptionPlan_SubscriptionPlansId",
                table: "SubscriptionFeatureSubscriptionPlan",
                column: "SubscriptionPlansId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanLimit_SubscriptionFeatures_SubscriptionFeatureId",
                table: "SubscriptionPlanLimit",
                column: "SubscriptionFeatureId",
                principalTable: "SubscriptionFeatures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanLimit_SubscriptionFeatures_SubscriptionFeatureId",
                table: "SubscriptionPlanLimit");

            migrationBuilder.DropTable(
                name: "SubscriptionFeatureSubscriptionPlan");

            migrationBuilder.RenameColumn(
                name: "SubscriptionFeatureId",
                table: "SubscriptionPlanLimit",
                newName: "SubscriptionPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_SubscriptionPlanLimit_SubscriptionFeatureId_LimitCodeSubscription",
                table: "SubscriptionPlanLimit",
                newName: "IX_SubscriptionPlanLimit_SubscriptionPlanId_LimitCodeSubscription");

            migrationBuilder.CreateTable(
                name: "SubscriptionPlanFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionFeatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                name: "IX_SubscriptionPlanFeatures_SubscriptionFeatureId",
                table: "SubscriptionPlanFeatures",
                column: "SubscriptionFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanFeatures_SubscriptionPlanId_SubscriptionFeatureId",
                table: "SubscriptionPlanFeatures",
                columns: new[] { "SubscriptionPlanId", "SubscriptionFeatureId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanLimit_SubscriptionPlans_SubscriptionPlanId",
                table: "SubscriptionPlanLimit",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
