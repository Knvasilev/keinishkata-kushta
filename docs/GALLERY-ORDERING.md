# Gallery Photo Ordering

Each photo in the Admin room gallery and House & Grounds gallery has a Position
field and a Move button. Positions start at 1. Enter the desired position and
submit to save it; the other photos shift to make room. This works without JavaScript.

Room and property galleries have separate orders. Public galleries, including
click/swipe navigation, follow the saved order. The selected cover still controls
the room cards on the homepage and room list; it does not override gallery order.
New photos append to their gallery in the order received by the upload form.

## Database Update

The included `20261010120000_AddGalleryImageOrdering` migration adds `SortOrder`
and initializes existing photos to their previous public order: newest first for
the property gallery, and cover first followed by newest first within each room.
It does not change photo files, captions, or cover selections.

Rebuild and restart the updated web project. `Program.cs` applies pending
migrations automatically before serving requests.

For a manual database update, run from the solution directory, with `dotnet-ef`
installed:

```powershell
dotnet ef database update --project .\KeinishkataKushta.Data --startup-project .\KeinishkataKushta\KeinishkataKushta.Web.csproj
```

If the CLI tool is not installed:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.11
```

## Verification

1. Open a room's Admin gallery, move its last photo to position 1, and reload.
2. Open the public room page and check the first photo and click/swipe sequence.
3. Move a different photo to the last position; verify the middle photos retain
   their relative order and the room-card cover has not changed.
4. Repeat in House & Grounds. Changing one gallery must not reorder another.
5. Upload several more photos; they should appear after the existing photos.

The automated gallery tests cover reordering, invalid positions, gallery
isolation, public queries, cover preservation, appending uploads, migration order
backfill, batch validation, and failure cleanup using sample data in SQLite.
The availability tests also verify that the SQL Server model and migration
snapshot agree. A real LocalDB update was not run in the agent session.

```powershell
dotnet run --project .\tests\GalleryUpload.Tests\GalleryUpload.Tests.csproj
dotnet run --project .\tests\Availability.Tests\Availability.Tests.csproj
```

## Files Changed For Ordering

- `KeinishkataKushta.Data/Entities/GalleryImage.cs`
- `KeinishkataKushta.Data/Migrations/20261010120000_AddGalleryImageOrdering.cs`
- `KeinishkataKushta.Data/Migrations/20261010120000_AddGalleryImageOrdering.Designer.cs`
- `KeinishkataKushta.Data/Migrations/AppDbContextModelSnapshot.cs`
- `KeinishkataKushta/Areas/Admin/Controllers/GalleryController.cs`
- `KeinishkataKushta/Areas/Admin/Controllers/RoomController.cs`
- `KeinishkataKushta/Areas/Admin/Models/GalleryIndexViewModel.cs`
- `KeinishkataKushta/Areas/Admin/Views/Gallery/Index.cshtml`
- `KeinishkataKushta/Areas/Admin/Views/Gallery/Property.cshtml`
- `KeinishkataKushta/Areas/Admin/Views/Gallery/_ImagePosition.cshtml`
- `KeinishkataKushta/Controllers/GalleryController.cs`
- `KeinishkataKushta/Controllers/HomeController.cs`
- `KeinishkataKushta/Controllers/RoomsController.cs`
- `KeinishkataKushta/Views/Rooms/Details.cshtml`
- `tests/GalleryUpload.Tests/Program.cs`
- `README.md`
- `docs/GALLERY-ORDERING.md`
