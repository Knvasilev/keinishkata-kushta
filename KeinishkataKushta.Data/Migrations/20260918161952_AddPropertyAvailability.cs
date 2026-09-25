using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeinishkataKushta.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvailabilityBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomsUsed = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Arrival = table.Column<DateOnly>(type: "date", nullable: false),
                    Departure = table.Column<DateOnly>(type: "date", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityBlocks", x => x.Id);
                    table.CheckConstraint("CK_AvailabilityBlocks_Dates", "[Departure] > [Arrival]");
                    table.CheckConstraint("CK_AvailabilityBlocks_Kind", "[Kind] IN (0, 1)");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityBlocks_Arrival_Departure",
                table: "AvailabilityBlocks",
                columns: new[] { "Arrival", "Departure" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailabilityBlocks");

        }
    }
}
