using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationWithChargeAndItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShipmentChargesItems",
                columns: table => new
                {
                    ShipmentChargeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShipmentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentChargesItems", x => new { x.ShipmentChargeId, x.ShipmentItemId });
                    table.ForeignKey(
                        name: "FK_ShipmentChargesItems_ShipmentCharges_ShipmentChargeId",
                        column: x => x.ShipmentChargeId,
                        principalTable: "ShipmentCharges",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ShipmentChargesItems_ShipmentItems_ShipmentItemId",
                        column: x => x.ShipmentItemId,
                        principalTable: "ShipmentItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentChargesItems_ShipmentItemId",
                table: "ShipmentChargesItems",
                column: "ShipmentItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShipmentChargesItems");
        }
    }
}
