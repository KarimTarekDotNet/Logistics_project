using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Routes",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Routes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Routes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Routes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Quotes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Quotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Quotes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "QuoteItems",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "QuoteItems",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "QuoteItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "QuoteItems",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Ports",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Ports",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Ports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Ports",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ContainerTypes",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "ContainerTypes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ContainerTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ContainerTypes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Carriers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Carriers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Carriers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Carriers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Carriers",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "Carriers",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000011"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "ContainerTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000020"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "ContainerTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000021"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "ContainerTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000022"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "Ports",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "Ports",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "Ports",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "QuoteItems",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000070"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "QuoteItems",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000071"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "QuoteItems",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000072"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "QuoteItems",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000073"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "Quotes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000060"),
                columns: new[] { "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "Quotes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000061"),
                columns: new[] { "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { null, false, null });

            migrationBuilder.UpdateData(
                table: "Routes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000030"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });

            migrationBuilder.UpdateData(
                table: "Routes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000031"),
                columns: new[] { "CreatedAt", "DeletedAt", "IsDeleted", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Ports");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Ports");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Ports");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Ports");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ContainerTypes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ContainerTypes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ContainerTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ContainerTypes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Carriers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Carriers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Carriers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Carriers");
        }
    }
}
