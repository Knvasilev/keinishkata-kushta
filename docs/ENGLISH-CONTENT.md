# English Room Content and Photo Captions

Room names, descriptions, and gallery captions are editable business content.
The Admin forms now store an optional English version for each alongside the
existing Bulgarian/default version. These translations are entered by the admin;
there is no machine-translation service or external API dependency.

Switching the public site to EN selects the saved English fields on the homepage,
room list, room details, gallery tiles, image alternative text, and lightbox
captions. An empty English field falls back to the original field independently,
so a missing description translation does not hide an English room name.

## Entering Translations

1. Open Admin > Rooms > Edit. Fill in Name (English) and Description (English),
   then Save. The original fields retain their Bulgarian/default text.
2. Open a room's gallery or House & Grounds. Edit captions under any number of
   existing photos, then choose **Save all changes** once. Both languages are
   independently editable; clearing English restores the public Bulgarian fallback.
3. For new uploads, both caption fields apply to all photos in the selected batch.
4. Switch to EN on the public site and check the room cards, details, and galleries.

For example, `Механа & Кухня` can have the English caption `Tavern & Kitchen`.
Photo files do not need to be uploaded again. Caption editing preserves file
names, cover choices, room associations, and photo ordering. Room translation
editing preserves the existing slug and its URL.

With JavaScript enabled, select photos and choose a language and shared caption,
then **Apply to selected photos**. This fills empty fields by default. To replace
nonempty captions, explicitly check **Replace existing nonempty captions in this
language** before applying; that permission resets after each application or
language change. Applying only edits the fields. Review or adjust each photo,
then choose **Save all changes** to persist the batch. Empty shared text cannot
clear a selection accidentally; clear individual fields when needed.

Bulk saving works without JavaScript. Validation errors retain the draft and save
nothing. If another session changed an edited field, the whole batch rolls back;
copy your draft before reloading and reconciling the newer captions. Unchanged
fields preserve newer values. Save before uploading, moving, deleting, or changing
a cover; these remain separate actions. JavaScript warns before leaving unsaved
caption edits. No additional migration or dependency is required for this editor.

## Database Update

The included `20261010130000_AddEnglishContent` migration adds nullable
`Rooms.NameEn`, `Rooms.DescriptionEn`, and `GalleryImages.CaptionEn` columns.
Existing text and photos are preserved; the new English fields start empty.

Rebuild and restart the updated web project to apply pending migrations through
the existing startup migration code. For a manual update from the solution folder:

```powershell
dotnet ef database update --project .\KeinishkataKushta.Data --startup-project .\KeinishkataKushta\KeinishkataKushta.Web.csproj
```

If `dotnet-ef` is not installed:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.11
```

## Verification

The gallery test harness checks both languages across public controllers,
cookie/query language selection, fallback behavior, bilingual room create/edit,
caption edits on existing photos, validation limits, and bilingual batch uploads.
It uses an isolated SQLite database. The availability harness verifies that the
SQL Server model and migration snapshot agree. The real LocalDB migration must
be applied when starting the updated website under the developer's account.

```powershell
dotnet run --project .\tests\GalleryUpload.Tests\GalleryUpload.Tests.csproj
dotnet run --project .\tests\Availability.Tests\Availability.Tests.csproj
```

## Files Changed For English Content

- `KeinishkataKushta.Data/AppDbContext.cs`
- `KeinishkataKushta.Data/Entities/Room.cs`
- `KeinishkataKushta.Data/Entities/GalleryImage.cs`
- `KeinishkataKushta.Data/Migrations/20261010130000_AddEnglishContent.cs`
- `KeinishkataKushta.Data/Migrations/20261010130000_AddEnglishContent.Designer.cs`
- `KeinishkataKushta.Data/Migrations/AppDbContextModelSnapshot.cs`
- `KeinishkataKushta/Areas/Admin/Controllers/RoomController.cs`
- `KeinishkataKushta/Areas/Admin/Controllers/GalleryController.cs`
- `KeinishkataKushta/Areas/Admin/Models/RoomFormViewModel.cs`
- `KeinishkataKushta/Areas/Admin/Models/GalleryUploadViewModel.cs`
- `KeinishkataKushta/Areas/Admin/Models/PropertyGalleryViewModels.cs`
- `KeinishkataKushta/Areas/Admin/Models/GalleryIndexViewModel.cs`
- `KeinishkataKushta/Areas/Admin/Views/Room/Create.cshtml`
- `KeinishkataKushta/Areas/Admin/Views/Room/Edit.cshtml`
- `KeinishkataKushta/Areas/Admin/Views/Gallery/Index.cshtml`
- `KeinishkataKushta/Areas/Admin/Views/Gallery/Property.cshtml`
- `KeinishkataKushta/Areas/Admin/Views/Gallery/_ImageCaptions.cshtml`
- `KeinishkataKushta/Controllers/HomeController.cs`
- `KeinishkataKushta/Controllers/RoomsController.cs`
- `KeinishkataKushta/Controllers/GalleryController.cs`
- `tests/GalleryUpload.Tests/Program.cs`
- `README.md`
- `docs/ENGLISH-CONTENT.md`
