using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeinishkataKushta.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260803180000_AddAdminAuthenticationAndEuroPrices")]
public partial class AddAdminAuthenticationAndEuroPrices : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AdminUsers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                NormalizedUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                LockoutEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastLoginUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AdminUsers", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AdminUsers_NormalizedUsername",
            table: "AdminUsers",
            column: "NormalizedUsername",
            unique: true);

        migrationBuilder.Sql(
            "UPDATE [Rooms] SET [PricePerNight] = ROUND([PricePerNight] / CAST(1.95583 AS decimal(10,5)), 2);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "UPDATE [Rooms] SET [PricePerNight] = ROUND([PricePerNight] * CAST(1.95583 AS decimal(10,5)), 2);");

        migrationBuilder.DropTable(name: "AdminUsers");
    }
}
