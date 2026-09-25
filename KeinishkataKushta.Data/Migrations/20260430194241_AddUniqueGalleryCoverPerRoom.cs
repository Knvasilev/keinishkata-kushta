using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeinishkataKushta.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueGalleryCoverPerRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GalleryImages_RoomId_IsCover",
                table: "GalleryImages",
                columns: new[] { "RoomId", "IsCover" },
                unique: true,
                filter: "[IsCover] = 1 AND [RoomId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GalleryImages_RoomId_IsCover",
                table: "GalleryImages");
        }
    }
}
