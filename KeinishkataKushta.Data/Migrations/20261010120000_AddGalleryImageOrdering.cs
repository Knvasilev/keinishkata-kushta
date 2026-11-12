using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeinishkataKushta.Data.Migrations
{
    public partial class AddGalleryImageOrdering : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "GalleryImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Preserve the existing public order within each room and the property gallery.
            migrationBuilder.Sql("""
                WITH OrderedImages AS
                (
                    SELECT [Id], ROW_NUMBER() OVER
                    (
                        PARTITION BY [RoomId]
                        ORDER BY CASE WHEN [RoomId] IS NOT NULL AND [IsCover] = 1 THEN 0 ELSE 1 END, [Id] DESC
                    ) - 1 AS [Position]
                    FROM [GalleryImages]
                )
                UPDATE [GalleryImages]
                SET [SortOrder] =
                (
                    SELECT ordered.[Position]
                    FROM OrderedImages AS ordered
                    WHERE ordered.[Id] = [GalleryImages].[Id]
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "SortOrder", table: "GalleryImages");
        }
    }
}
