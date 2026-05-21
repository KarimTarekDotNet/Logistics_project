using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldsAndAddChargeRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AllowedChargeableWeightKg",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AllowedGrossWeightKg",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AllowedNetWeightKg",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AllowedVolumeCbm",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsHazardousAllowed",
                table: "Shipments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalChargeableWeightKg",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalGrossWeightKg",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNetWeightKg",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalVolumeCbm",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RequestedChargeableWeightKg",
                table: "Quotes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ShipmentChargeRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargeType = table.Column<int>(type: "int", nullable: false),
                    PayerType = table.Column<int>(type: "int", nullable: false),
                    CalculationType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentChargeRules", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShipmentChargeRules");

            migrationBuilder.DropColumn(
                name: "AllowedChargeableWeightKg",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "AllowedGrossWeightKg",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "AllowedNetWeightKg",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "AllowedVolumeCbm",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "IsHazardousAllowed",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "TotalChargeableWeightKg",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "TotalGrossWeightKg",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "TotalNetWeightKg",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "TotalVolumeCbm",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "RequestedChargeableWeightKg",
                table: "Quotes");
        }
    }
}
