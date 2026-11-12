using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeinishkataKushta.Data.Migrations
{
    public partial class AddEnglishContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CaptionEn", table: "GalleryImages", type: "nvarchar(500)", maxLength: 500, nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "NameEn", table: "Rooms", type: "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn", table: "Rooms", type: "nvarchar(4000)", maxLength: 4000, nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CaptionEn", table: "GalleryImages");
            migrationBuilder.DropColumn(name: "NameEn", table: "Rooms");
            migrationBuilder.DropColumn(name: "DescriptionEn", table: "Rooms");
        }
    }
}
