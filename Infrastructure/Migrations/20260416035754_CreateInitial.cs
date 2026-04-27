using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Carriers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carriers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContainerTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromPortId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToPortId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routes_Ports_FromPortId",
                        column: x => x.FromPortId,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Routes_Ports_ToPortId",
                        column: x => x.ToPortId,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RouteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContainerTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotes_ContainerTypes_ContainerTypeId",
                        column: x => x.ContainerTypeId,
                        principalTable: "ContainerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotes_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CarrierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContainerTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ValidTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rates_Carriers_CarrierId",
                        column: x => x.CarrierId,
                        principalTable: "Carriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rates_ContainerTypes_ContainerTypeId",
                        column: x => x.ContainerTypeId,
                        principalTable: "ContainerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rates_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteItems_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Carriers",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000010"), "MAEU", "Maersk Line" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "MSCU", "Mediterranean Shipping Company" }
                });

            migrationBuilder.InsertData(
                table: "ContainerTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000020"), "20ft Standard" },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "40ft Standard" },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "40ft High Cube" }
                });

            migrationBuilder.InsertData(
                table: "Ports",
                columns: new[] { "Id", "Code", "Country", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "CNSHA", "China", "Shanghai" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "NLRTM", "Netherlands", "Rotterdam" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "AEJEA", "UAE", "Dubai" }
                });

            migrationBuilder.InsertData(
                table: "Routes",
                columns: new[] { "Id", "FromPortId", "ToPortId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000030"), new Guid("00000000-0000-0000-0000-000000000001"), new Guid("00000000-0000-0000-0000-000000000002") },
                    { new Guid("00000000-0000-0000-0000-000000000031"), new Guid("00000000-0000-0000-0000-000000000001"), new Guid("00000000-0000-0000-0000-000000000003") }
                });

            migrationBuilder.InsertData(
                table: "Quotes",
                columns: new[] { "Id", "ContainerTypeId", "CreatedAt", "Currency", "CustomerName", "FinalPrice", "RouteId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000060"), new Guid("00000000-0000-0000-0000-000000000020"), new DateTimeOffset(new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "USD", "Alpha Trading Co.", 1650.00m, new Guid("00000000-0000-0000-0000-000000000030") },
                    { new Guid("00000000-0000-0000-0000-000000000061"), new Guid("00000000-0000-0000-0000-000000000021"), new DateTimeOffset(new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "USD", "Beta Logistics Ltd.", 3100.00m, new Guid("00000000-0000-0000-0000-000000000031") }
                });

            migrationBuilder.InsertData(
                table: "Rates",
                columns: new[] { "Id", "CarrierId", "ContainerTypeId", "CreatedAt", "Currency", "DeletedAt", "IsActive", "IsDeleted", "Price", "RouteId", "UpdatedAt", "ValidFrom", "ValidTo" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000040"), new Guid("00000000-0000-0000-0000-000000000010"), new Guid("00000000-0000-0000-0000-000000000020"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "USD", null, true, false, 1500.00m, new Guid("00000000-0000-0000-0000-000000000030"), new DateTimeOffset(new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("00000000-0000-0000-0000-000000000041"), new Guid("00000000-0000-0000-0000-000000000010"), new Guid("00000000-0000-0000-0000-000000000021"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "USD", null, false, false, 2800.00m, new Guid("00000000-0000-0000-0000-000000000030"), new DateTimeOffset(new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("00000000-0000-0000-0000-000000000042"), new Guid("00000000-0000-0000-0000-000000000011"), new Guid("00000000-0000-0000-0000-000000000020"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "USD", null, true, false, 900.00m, new Guid("00000000-0000-0000-0000-000000000031"), null, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "QuoteItems",
                columns: new[] { "Id", "Amount", "Description", "QuoteId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000070"), 1500.00m, "Ocean Freight", new Guid("00000000-0000-0000-0000-000000000060") },
                    { new Guid("00000000-0000-0000-0000-000000000071"), 150.00m, "Bunker Adjustment Factor", new Guid("00000000-0000-0000-0000-000000000060") },
                    { new Guid("00000000-0000-0000-0000-000000000072"), 2800.00m, "Ocean Freight", new Guid("00000000-0000-0000-0000-000000000061") },
                    { new Guid("00000000-0000-0000-0000-000000000073"), 300.00m, "Port Handling Fee", new Guid("00000000-0000-0000-0000-000000000061") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carriers_Code",
                table: "Carriers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ports_Code",
                table: "Ports",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItems_QuoteId",
                table: "QuoteItems",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ContainerTypeId",
                table: "Quotes",
                column: "ContainerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CreatedAt",
                table: "Quotes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_RouteId",
                table: "Quotes",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_CarrierId",
                table: "Rates",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_ContainerTypeId",
                table: "Rates",
                column: "ContainerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_IsActive",
                table: "Rates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_RouteId",
                table: "Rates",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_ValidTo",
                table: "Rates",
                column: "ValidTo");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_FromPortId",
                table: "Routes",
                column: "FromPortId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_FromPortId_ToPortId",
                table: "Routes",
                columns: new[] { "FromPortId", "ToPortId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_ToPortId",
                table: "Routes",
                column: "ToPortId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuoteItems");

            migrationBuilder.DropTable(
                name: "Rates");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "Carriers");

            migrationBuilder.DropTable(
                name: "ContainerTypes");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Ports");
        }
    }
}
