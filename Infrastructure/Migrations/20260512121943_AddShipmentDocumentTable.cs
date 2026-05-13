using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentDocumentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShipmentDocument",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IntegrationMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentDocument_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocument_IntegrationMessageId",
                table: "ShipmentDocument",
                column: "IntegrationMessageId",
                filter: "[IntegrationMessageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocument_IsDeleted",
                table: "ShipmentDocument",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocument_ShipmentId",
                table: "ShipmentDocument",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocument_ShipmentId_Type",
                table: "ShipmentDocument",
                columns: new[] { "ShipmentId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocument_ShipmentId_UploadedAt",
                table: "ShipmentDocument",
                columns: new[] { "ShipmentId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocument_Type",
                table: "ShipmentDocument",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocument_UploadedAt",
                table: "ShipmentDocument",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocument_UploadedByUserId",
                table: "ShipmentDocument",
                column: "UploadedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShipmentDocument");
        }
    }
}
