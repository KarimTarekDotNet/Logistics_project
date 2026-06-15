using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanFeatures_SubscriptionPlanId",
                table: "SubscriptionPlanFeatures");

            migrationBuilder.RenameColumn(
                name: "MaxValue",
                table: "SubscriptionPlanLimit",
                newName: "LimitMaxValue");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "SubscriptionPlanLimit",
                newName: "LimitCodeSubscription");

            migrationBuilder.RenameIndex(
                name: "IX_SubscriptionPlanLimit_SubscriptionPlanId_Code",
                table: "SubscriptionPlanLimit",
                newName: "IX_SubscriptionPlanLimit_SubscriptionPlanId_LimitCodeSubscription");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "SubscriptionFeatures",
                newName: "FeatureName");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "SubscriptionFeatures",
                newName: "FeatureCode");

            migrationBuilder.RenameIndex(
                name: "IX_SubscriptionFeatures_Code",
                table: "SubscriptionFeatures",
                newName: "IX_SubscriptionFeatures_FeatureCode");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanFeatures_SubscriptionPlanId_SubscriptionFeatureId",
                table: "SubscriptionPlanFeatures",
                columns: new[] { "SubscriptionPlanId", "SubscriptionFeatureId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanFeatures_SubscriptionPlanId_SubscriptionFeatureId",
                table: "SubscriptionPlanFeatures");

            migrationBuilder.RenameColumn(
                name: "LimitMaxValue",
                table: "SubscriptionPlanLimit",
                newName: "MaxValue");

            migrationBuilder.RenameColumn(
                name: "LimitCodeSubscription",
                table: "SubscriptionPlanLimit",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_SubscriptionPlanLimit_SubscriptionPlanId_LimitCodeSubscription",
                table: "SubscriptionPlanLimit",
                newName: "IX_SubscriptionPlanLimit_SubscriptionPlanId_Code");

            migrationBuilder.RenameColumn(
                name: "FeatureName",
                table: "SubscriptionFeatures",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "FeatureCode",
                table: "SubscriptionFeatures",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_SubscriptionFeatures_FeatureCode",
                table: "SubscriptionFeatures",
                newName: "IX_SubscriptionFeatures_Code");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanFeatures_SubscriptionPlanId",
                table: "SubscriptionPlanFeatures",
                column: "SubscriptionPlanId");
        }
    }
}
