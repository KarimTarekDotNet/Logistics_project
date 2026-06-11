using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditSubscriptionPlanPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "UserSubscriptions",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "UserSubscriptions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserSubscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "UserSubscriptions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "SubscriptionPlans",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "SubscriptionPlans",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionPlanId",
                table: "PaymentTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_SubscriptionPlanId",
                table: "PaymentTransactions",
                column: "SubscriptionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_SubscriptionPlans_SubscriptionPlanId",
                table: "PaymentTransactions",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_SubscriptionPlans_SubscriptionPlanId",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_SubscriptionPlanId",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "PaymentTransactions");
        }
    }
}
