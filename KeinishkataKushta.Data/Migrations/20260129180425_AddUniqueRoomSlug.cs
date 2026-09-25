using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeinishkataKushta.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueRoomSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Slug",
                table: "Rooms",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_Slug",
                table: "Rooms");
        }
    }
}
