using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditShipmentDocumentTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentDocument_Shipments_ShipmentId",
                table: "ShipmentDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShipmentDocument",
                table: "ShipmentDocument");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentDocument_IntegrationMessageId",
                table: "ShipmentDocument");

            migrationBuilder.RenameTable(
                name: "ShipmentDocument",
                newName: "ShipmentDocuments");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocument_UploadedByUserId",
                table: "ShipmentDocuments",
                newName: "IX_ShipmentDocuments_UploadedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocument_UploadedAt",
                table: "ShipmentDocuments",
                newName: "IX_ShipmentDocuments_UploadedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocument_Type",
                table: "ShipmentDocuments",
                newName: "IX_ShipmentDocuments_Type");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocument_ShipmentId_UploadedAt",
                table: "ShipmentDocuments",
                newName: "IX_ShipmentDocuments_ShipmentId_UploadedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocument_ShipmentId_Type",
                table: "ShipmentDocuments",
                newName: "IX_ShipmentDocuments_ShipmentId_Type");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocument_ShipmentId",
                table: "ShipmentDocuments",
                newName: "IX_ShipmentDocuments_ShipmentId");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocument_IsDeleted",
                table: "ShipmentDocuments",
                newName: "IX_ShipmentDocuments_IsDeleted");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShipmentDocuments",
                table: "ShipmentDocuments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocuments_IntegrationMessageId",
                table: "ShipmentDocuments",
                column: "IntegrationMessageId",
                filter: "[IntegrationMessageId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentDocuments_Shipments_ShipmentId",
                table: "ShipmentDocuments",
                column: "ShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentDocuments_Shipments_ShipmentId",
                table: "ShipmentDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShipmentDocuments",
                table: "ShipmentDocuments");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentDocuments_IntegrationMessageId",
                table: "ShipmentDocuments");

            migrationBuilder.RenameTable(
                name: "ShipmentDocuments",
                newName: "ShipmentDocument");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocuments_UploadedByUserId",
                table: "ShipmentDocument",
                newName: "IX_ShipmentDocument_UploadedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocuments_UploadedAt",
                table: "ShipmentDocument",
                newName: "IX_ShipmentDocument_UploadedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocuments_Type",
                table: "ShipmentDocument",
                newName: "IX_ShipmentDocument_Type");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocuments_ShipmentId_UploadedAt",
                table: "ShipmentDocument",
                newName: "IX_ShipmentDocument_ShipmentId_UploadedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocuments_ShipmentId_Type",
                table: "ShipmentDocument",
                newName: "IX_ShipmentDocument_ShipmentId_Type");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocuments_ShipmentId",
                table: "ShipmentDocument",
                newName: "IX_ShipmentDocument_ShipmentId");

            migrationBuilder.RenameIndex(
                name: "IX_ShipmentDocuments_IsDeleted",
                table: "ShipmentDocument",
                newName: "IX_ShipmentDocument_IsDeleted");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShipmentDocument",
                table: "ShipmentDocument",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocument_IntegrationMessageId",
                table: "ShipmentDocument",
                column: "IntegrationMessageId",
                filter: "[ExternalMessageId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentDocument_Shipments_ShipmentId",
                table: "ShipmentDocument",
                column: "ShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
