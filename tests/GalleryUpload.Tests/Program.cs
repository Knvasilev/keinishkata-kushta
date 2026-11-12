using System.ComponentModel.DataAnnotations;
using KeinishkataKushta.Data;
using KeinishkataKushta.Data.Entities;
using KeinishkataKushta.Web.Areas.Admin.Controllers;
using KeinishkataKushta.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;

var tests = new (string Name, Func<Task> Run)[]
{
    ("English room names and descriptions appear on the homepage, list, and details", async () =>
    {
        await using var fixture = await Fixture.Create();
        var room = await fixture.Db.Rooms.FindAsync(1);
        room!.Name = "Апартамент 2";
        room.NameEn = "Apartment 2";
        room.Description = "Две спални и тераса.";
        room.DescriptionEn = "Two bedrooms and a terrace.";
        await fixture.Db.SaveChangesAsync();
        foreach (var lang in new[] { "bg", "en" })
        {
            var expectedName = lang == "en" ? room.NameEn : room.Name;
            var expectedDescription = lang == "en" ? room.DescriptionEn : room.Description;
            var controller = fixture.InLanguage(new KeinishkataKushta.Controllers.RoomsController(fixture.Db), lang);
            var list = (ViewResult)await controller.Index();
            var card = ((List<KeinishkataKushta.Models.RoomCardViewModel>)list.Model!).Single(item => item.Id == 1);
            Check(card.Name == expectedName && card.Description == expectedDescription);
            var details = (ViewResult)await controller.Details("test-room");
            var model = (KeinishkataKushta.Models.RoomDetailsViewModel)details.Model!;
            Check(model.Name == expectedName && model.Description == expectedDescription);
            var home = (ViewResult)await fixture.InLanguage(new KeinishkataKushta.Controllers.HomeController(
                fixture.Db, NullLogger<KeinishkataKushta.Controllers.HomeController>.Instance), lang).Index();
            var featured = ((KeinishkataKushta.Models.HomeIndexViewModel)home.Model!).FeaturedRooms.Single(item => item.Id == 1);
            Check(featured.Name == expectedName && featured.Description == expectedDescription);
        }
    }),
    ("Both public galleries select captions for the requested language", async () =>
    {
        await using var fixture = await Fixture.Create();
        var property = await fixture.SeedImages(null);
        var room = await fixture.SeedImages(1);
        foreach (var image in property.Concat(room))
        {
            image.Caption = "Механа & Кухня";
            image.CaptionEn = "Tavern & Kitchen";
        }
        await fixture.Db.SaveChangesAsync();
        foreach (var lang in new[] { "bg", "en" })
        {
            var caption = lang == "en" ? "Tavern & Kitchen" : "Механа & Кухня";
            var gallery = (ViewResult)await fixture.InLanguage(new KeinishkataKushta.Controllers.GalleryController(fixture.Db), lang).Index();
            Check(((KeinishkataKushta.Models.PropertyGalleryViewModel)gallery.Model!).Images.All(image => image.Caption == caption));
            var details = (ViewResult)await fixture.InLanguage(new KeinishkataKushta.Controllers.RoomsController(fixture.Db), lang).Details("test-room");
            Check(((KeinishkataKushta.Models.RoomDetailsViewModel)details.Model!).Images.All(image => image.Caption == caption));
        }
    }),
    ("Missing English fields fall back independently to existing content", async () =>
    {
        await using var fixture = await Fixture.Create();
        var room = await fixture.Db.Rooms.FindAsync(1);
        room!.NameEn = "Translated name";
        room.DescriptionEn = null;
        var images = await fixture.SeedImages(null);
        images[0].Caption = "Original caption";
        images[0].CaptionEn = null;
        await fixture.Db.SaveChangesAsync();
        var details = (ViewResult)await fixture.InLanguage(new KeinishkataKushta.Controllers.RoomsController(fixture.Db), "en").Details("test-room");
        var model = (KeinishkataKushta.Models.RoomDetailsViewModel)details.Model!;
        Check(model.Name == "Translated name" && model.Description == room.Description);
        var gallery = (ViewResult)await fixture.InLanguage(new KeinishkataKushta.Controllers.GalleryController(fixture.Db), "en").Index();
        Check(((KeinishkataKushta.Models.PropertyGalleryViewModel)gallery.Model!).Images[0].Caption == "Original caption");
    }),
    ("A saved English language cookie selects translated content", async () =>
    {
        await using var fixture = await Fixture.Create();
        var room = await fixture.Db.Rooms.FindAsync(1);
        room!.NameEn = "English room";
        await fixture.Db.SaveChangesAsync();
        var controller = fixture.InLanguage(new KeinishkataKushta.Controllers.RoomsController(fixture.Db), "en");
        controller.HttpContext.Request.QueryString = QueryString.Empty;
        controller.HttpContext.Request.Headers.Cookie = "kk_lang=en";
        var details = (ViewResult)await controller.Details("test-room");
        Check(((KeinishkataKushta.Models.RoomDetailsViewModel)details.Model!).Name == "English room");
    }),
    ("Existing photo captions can be edited without changing files, order, or cover", async () =>
    {
        foreach (var roomId in new int?[] { null, 1 })
        {
            await using var fixture = await Fixture.Create();
            var image = (await fixture.SeedImages(roomId))[0];
            var result = await fixture.Controller.UpdateCaptions(new GalleryImageCaptionViewModel
            {
                Id = image.Id, RoomId = roomId, Caption = "  Механа & Кухня  ", CaptionEn = "  Tavern & Kitchen  "
            });
            Check(result is RedirectToActionResult);
            fixture.Db.ChangeTracker.Clear();
            var saved = (await fixture.Db.GalleryImages.FindAsync(image.Id))!;
            Check(saved.Caption == "Механа & Кухня" && saved.CaptionEn == "Tavern & Kitchen");
            Check(saved.FileName == image.FileName && saved.SortOrder == image.SortOrder && saved.IsCover == image.IsCover);
            await fixture.Controller.UpdateCaptions(new GalleryImageCaptionViewModel
            {
                Id = saved.Id, RoomId = roomId, Caption = saved.Caption, CaptionEn = "   "
            });
            Check(saved.CaptionEn == null && saved.Caption == "Механа & Кухня");
        }
    }),
    ("Photo caption edits reject invalid forms and another gallery's image", async () =>
    {
        await using var fixture = await Fixture.Create();
        var image = (await fixture.SeedImages(1))[0];
        Check(await fixture.Controller.UpdateCaptions(new GalleryImageCaptionViewModel
        {
            Id = image.Id, RoomId = null, CaptionEn = "Wrong gallery"
        }) is NotFoundResult);
        fixture.Controller.ModelState.AddModelError("CaptionEn", "Too long");
        await fixture.Controller.UpdateCaptions(new GalleryImageCaptionViewModel
        {
            Id = image.Id, RoomId = 1, CaptionEn = new string('x', 501)
        });
        Check(image.CaptionEn == null && fixture.Controller.TempData.ContainsKey("Err"));
    }),
    ("Admin room create and edit preserve both languages and the existing slug", async () =>
    {
        await using var fixture = await Fixture.Create();
        var controller = new RoomController(fixture.Db);
        var model = new RoomFormViewModel
        {
            Name = "Нова стая", NameEn = "  New room  ", Description = "Описание",
            DescriptionEn = "  Description  ", Slug = "new-room", Capacity = 2, PricePerNight = 50, IsPublished = true
        };
        Check(await controller.Create(model) is RedirectToActionResult);
        var room = await fixture.Db.Rooms.SingleAsync(item => item.Slug == "new-room");
        Check(room.Name == model.Name && room.NameEn == "New room" && room.DescriptionEn == "Description");
        var edit = (RoomFormViewModel)((ViewResult)await controller.Edit(room.Id)).Model!;
        Check(edit.NameEn == "New room" && edit.DescriptionEn == "Description");
        edit.NameEn = "Updated room";
        edit.DescriptionEn = " ";
        Check(await controller.Edit(room.Id, edit) is RedirectToActionResult);
        Check(room.NameEn == "Updated room" && room.DescriptionEn == null && room.Slug == "new-room");
    }),
    ("English content has the same maximum lengths as the original fields", () =>
    {
        foreach (var model in new object[]
        {
            new GalleryUploadViewModel { Files = new() { Photo("one.jpg") }, CaptionEn = new string('x', 501) },
            new PropertyGalleryUploadViewModel { Files = new() { Photo("one.jpg") }, CaptionEn = new string('x', 501) },
            new GalleryImageCaptionViewModel { CaptionEn = new string('x', 501) },
            new RoomFormViewModel { Name = "Valid", NameEn = new string('x', 201), Capacity = 2, PricePerNight = 50 },
            new RoomFormViewModel { Name = "Valid", DescriptionEn = new string('x', 4001), Capacity = 2, PricePerNight = 50 }
        })
            Check(!Validator.TryValidateObject(model, new ValidationContext(model), new List<ValidationResult>(), true));
        return Task.CompletedTask;
    }),
    ("Property photos can move first, last, and back to the middle", async () =>
    {
        await using var fixture = await Fixture.Create();
        var images = await fixture.SeedImages(null);
        await fixture.Controller.SetPosition(images[2].Id, 1, null);
        Check((await fixture.PropertyOrder()).SequenceEqual(new[] { images[2].Id, images[0].Id, images[1].Id }));
        await fixture.Controller.SetPosition(images[2].Id, 3, null);
        Check((await fixture.PropertyOrder()).SequenceEqual(images.Select(image => image.Id)));
        Check(await fixture.Controller.SetPosition(images[2].Id, 2, null) is RedirectToActionResult { ActionName: "Property" });
        Check((await fixture.PropertyOrder()).SequenceEqual(new[] { images[0].Id, images[2].Id, images[1].Id }));
        Check((await fixture.Db.GalleryImages.OrderBy(image => image.SortOrder).Select(image => image.SortOrder).ToListAsync())
            .SequenceEqual(new[] { 0, 1, 2 }));
    }),
    ("Room photo ordering is isolated and keeps the selected cover", async () =>
    {
        await using var fixture = await Fixture.Create();
        var images = await fixture.SeedImages(1);
        var otherRoom = await fixture.SeedImages(2);
        var property = await fixture.SeedImages(null);
        var result = await fixture.Controller.SetPosition(images[2].Id, 1, 1);
        Check(result is RedirectToActionResult { ActionName: "Index" });
        fixture.Db.ChangeTracker.Clear();
        Check((await fixture.Db.GalleryImages.Where(image => image.RoomId == 1 && image.IsCover).SingleAsync()).Id == images[0].Id);
        Check((await fixture.Db.GalleryImages.Where(image => image.RoomId == 2).OrderBy(image => image.SortOrder)
            .Select(image => image.Id).ToListAsync()).SequenceEqual(otherRoom.Select(image => image.Id)));
        Check((await fixture.PropertyOrder()).SequenceEqual(property.Select(image => image.Id)));
    }),
    ("Invalid positions and images from another gallery cannot change order", async () =>
    {
        await using var fixture = await Fixture.Create();
        var images = await fixture.SeedImages(null);
        var room = await fixture.SeedImages(1);
        foreach (var position in new[] { 0, -1, 4 })
            Check(await fixture.Controller.SetPosition(images[1].Id, position, null) is BadRequestResult);
        Check(await fixture.Controller.SetPosition(room[0].Id, 1, null) is NotFoundResult);
        Check(await fixture.Controller.SetPosition(images[0].Id, 1, 1) is NotFoundResult);
        Check(await fixture.Controller.SetPosition(999, 1, null) is NotFoundResult);
        Check((await fixture.PropertyOrder()).SequenceEqual(images.Select(image => image.Id)));
    }),
    ("Both public galleries and room-card fallback use the saved order", async () =>
    {
        await using var fixture = await Fixture.Create();
        var property = await fixture.SeedImages(null);
        var room = await fixture.SeedImages(1);
        await fixture.Controller.SetPosition(property[2].Id, 1, null);
        await fixture.Controller.SetPosition(room[2].Id, 1, 1);
        var gallery = (ViewResult)await fixture.InLanguage(new KeinishkataKushta.Controllers.GalleryController(fixture.Db), "bg").Index();
        Check(((KeinishkataKushta.Models.PropertyGalleryViewModel)gallery.Model!).Images[0].FileName == property[2].FileName);
        var roomsController = fixture.InLanguage(new KeinishkataKushta.Controllers.RoomsController(fixture.Db), "bg");
        var details = (ViewResult)await roomsController.Details("test-room");
        Check(((KeinishkataKushta.Models.RoomDetailsViewModel)details.Model!).Images[0].FileName == room[2].FileName);
        var cards = (ViewResult)await roomsController.Index();
        Check(((List<KeinishkataKushta.Models.RoomCardViewModel>)cards.Model!).Single(card => card.Id == 1).CoverFileName == room[0].FileName);
        await fixture.Db.GalleryImages.Where(image => image.RoomId == 1).ExecuteUpdateAsync(setters => setters.SetProperty(image => image.IsCover, false));
        cards = (ViewResult)await roomsController.Index();
        Check(((List<KeinishkataKushta.Models.RoomCardViewModel>)cards.Model!).Single(card => card.Id == 1).CoverFileName == room[2].FileName);
    }),
    ("Uploads append after the saved order within their own gallery", async () =>
    {
        await using var fixture = await Fixture.Create();
        var property = await fixture.SeedImages(null);
        var room = await fixture.SeedImages(1);
        await fixture.Controller.SetPosition(property[2].Id, 1, null);
        await fixture.Controller.UploadProperty(new PropertyGalleryUploadViewModel { Files = new() { Photo("new.jpg"), Photo("other.png") } });
        await fixture.Controller.Upload(new GalleryUploadViewModel { RoomId = 1, Files = new() { Photo("room.jpg") } });
        var propertyOrder = await fixture.PropertyOrder();
        Check(propertyOrder.Take(3).SequenceEqual(new[] { property[2].Id, property[0].Id, property[1].Id }));
        Check((await fixture.Db.GalleryImages.Where(image => image.RoomId == null).OrderBy(image => image.SortOrder)
            .Select(image => image.SortOrder).ToListAsync()).SequenceEqual(new[] { 0, 1, 2, 3, 4 }));
        Check((await fixture.Db.GalleryImages.Where(image => image.RoomId == 1).OrderBy(image => image.SortOrder)
            .Select(image => image.SortOrder).ToListAsync()).SequenceEqual(new[] { 0, 1, 2, 3 }));
    }),
    ("Ordering migration backfills old display order in each gallery", async () =>
    {
        await using var fixture = await Fixture.Create();
        var property = await fixture.SeedImages(null);
        var room = await fixture.SeedImages(1);
        var otherRoom = await fixture.SeedImages(2);
        var migration = new KeinishkataKushta.Data.Migrations.AddGalleryImageOrdering();
        var sql = migration.UpOperations.OfType<SqlOperation>().Single().Sql;
        await fixture.Db.Database.ExecuteSqlRawAsync(sql);
        fixture.Db.ChangeTracker.Clear();
        Check((await fixture.PropertyOrder()).SequenceEqual(property.AsEnumerable().Reverse().Select(image => image.Id)));
        foreach (var group in new[] { room, otherRoom })
            Check((await fixture.Db.GalleryImages.Where(image => image.RoomId == group[0].RoomId)
                .OrderBy(image => image.SortOrder).Select(image => image.Id).ToListAsync())
                .SequenceEqual(new[] { group[0].Id, group[2].Id, group[1].Id }));
    }),
    ("MVC binds multiple files using the Upload.Files form field", async () =>
    {
        var files = new FormFileCollection { Photo("one.jpg"), Photo("two.png") };
        var httpContext = new DefaultHttpContext();
        httpContext.Request.ContentType = "multipart/form-data; boundary=test";
        httpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);
        var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(List<IFormFile>));
        var bindingContext = DefaultModelBindingContext.CreateBindingContext(
            new ActionContext { HttpContext = httpContext }, new CompositeValueProvider(), metadata, null, "Upload.Files");
        await new FormFileModelBinder(NullLoggerFactory.Instance).BindModelAsync(bindingContext);
        Check(bindingContext.Result.IsModelSet);
        Check(bindingContext.Result.Model is List<IFormFile> bound && bound.Count == 2
            && bound[0].FileName == "one.jpg" && bound[1].FileName == "two.png");
    }),
    ("Property batch saves every file with unique names and one shared caption", async () =>
    {
        await using var fixture = await Fixture.Create();
        var result = await fixture.Controller.UploadProperty(new PropertyGalleryUploadViewModel
        {
            Files = new() { Photo("duplicate.JPG"), Photo("duplicate.JPG"), Photo("third.webp") },
            Caption = "  Courtyard  ", CaptionEn = "  English courtyard  "
        });
        Check(result is RedirectToActionResult { ActionName: "Property" });
        var images = await fixture.Db.GalleryImages.OrderBy(image => image.Id).ToListAsync();
        Check(images.Count == 3 && images.Select(image => image.FileName).Distinct().Count() == 3);
        Check(images.All(image => image.RoomId == null && !image.IsCover && image.Caption == "Courtyard" && image.CaptionEn == "English courtyard"));
        Check(images[0].FileName.EndsWith(".jpg") && images[2].FileName.EndsWith(".webp"));
        foreach (var image in images)
            Check((await File.ReadAllBytesAsync(Path.Combine(fixture.Root, "uploads", "property", image.FileName)))
                .SequenceEqual(new byte[] { 1, 2, 3, 4 }));
    }),
    ("An empty room automatically uses only the first uploaded photo as cover", async () =>
    {
        await using var fixture = await Fixture.Create();
        await fixture.Controller.Upload(new GalleryUploadViewModel
        {
            RoomId = 1, CaptionEn = "English room", Files = new() { Photo("first.jpg"), Photo("second.png"), Photo("third.webp") }
        });
        var images = await fixture.Db.GalleryImages.OrderBy(image => image.Id).ToListAsync();
        Check(images.Count == 3 && images[0].IsCover && images.Count(image => image.IsCover) == 1);
        Check(images.All(image => image.RoomId == 1 && image.CaptionEn == "English room"));
        Check(Directory.GetFiles(Path.Combine(fixture.Root, "uploads", "rooms", "1")).Length == 3);
    }),
    ("A normal room batch preserves the existing cover", async () =>
    {
        await using var fixture = await Fixture.Create();
        var cover = await fixture.SeedCover(1);
        await fixture.Controller.Upload(new GalleryUploadViewModel
        {
            RoomId = 1, Files = new() { Photo("one.jpg"), Photo("two.jpg") }
        });
        fixture.Db.ChangeTracker.Clear();
        Check(await fixture.Db.GalleryImages.CountAsync() == 3);
        Check((await fixture.Db.GalleryImages.SingleAsync(image => image.IsCover)).Id == cover.Id);
    }),
    ("Cover replacement affects only the chosen room and first new photo", async () =>
    {
        await using var fixture = await Fixture.Create();
        var oldCover = await fixture.SeedCover(1);
        var otherCover = await fixture.SeedCover(2);
        await fixture.Controller.Upload(new GalleryUploadViewModel
        {
            RoomId = 1, IsCover = true, Files = new() { Photo("one.png"), Photo("two.webp") }
        });
        fixture.Db.ChangeTracker.Clear();
        var images = await fixture.Db.GalleryImages.Where(image => image.RoomId == 1).OrderBy(image => image.Id).ToListAsync();
        Check(!images[0].IsCover && images[0].Id == oldCover.Id);
        Check(images[1].IsCover && !images[2].IsCover);
        Check((await fixture.Db.GalleryImages.FindAsync(otherCover.Id))!.IsCover);
    }),
    ("Both galleries reject an invalid file anywhere in the batch before saving", async () =>
    {
        foreach (var bad in new[] { Photo("bad.txt"), Photo("empty.jpg", 0), Photo("large.png", 5 * 1024 * 1024 + 1) })
        {
            await using var fixture = await Fixture.Create();
            var files = new List<IFormFile> { Photo("good.jpg"), bad };
            await fixture.Controller.UploadProperty(new PropertyGalleryUploadViewModel { Files = files });
            Check(fixture.Controller.TempData.ContainsKey("Err"));
            await fixture.Controller.Upload(new GalleryUploadViewModel { RoomId = 1, Files = files, IsCover = true });
            Check(await fixture.Db.GalleryImages.CountAsync() == 0);
            Check(Directory.GetFiles(fixture.Root, "*", SearchOption.AllDirectories).Length == 0);
        }
    }),
    ("Empty and over-limit selections are rejected in both galleries", async () =>
    {
        foreach (var count in new[] { 0, 21 })
        {
            await using var fixture = await Fixture.Create();
            var files = Enumerable.Range(0, count).Select(index => Photo($"{index}.jpg")).ToList();
            await fixture.Controller.UploadProperty(new PropertyGalleryUploadViewModel { Files = files });
            await fixture.Controller.Upload(new GalleryUploadViewModel { RoomId = 1, Files = files });
            Check(fixture.Controller.TempData.ContainsKey("Err"));
            Check(await fixture.Db.GalleryImages.CountAsync() == 0);
            Check(!Directory.Exists(Path.Combine(fixture.Root, "uploads")));
        }
    }),
    ("The full twenty-photo batch is accepted", async () =>
    {
        await using var fixture = await Fixture.Create();
        await fixture.Controller.UploadProperty(new PropertyGalleryUploadViewModel
        {
            Files = Enumerable.Range(0, 20).Select(index => Photo($"{index}.png")).ToList()
        });
        Check(await fixture.Db.GalleryImages.CountAsync() == 20);
    }),
    ("An unknown room returns NotFound without writing files", async () =>
    {
        await using var fixture = await Fixture.Create();
        Check(await fixture.Controller.Upload(new GalleryUploadViewModel
        {
            RoomId = 999, Files = new() { Photo("one.jpg") }
        }) is NotFoundResult);
        Check(!Directory.Exists(Path.Combine(fixture.Root, "uploads")));
    }),
    ("A partial file-copy failure cleans the entire batch and preserves the old cover", async () =>
    {
        await using var fixture = await Fixture.Create();
        var cover = await fixture.SeedCover(1);
        await ExpectFailure(() => fixture.Controller.Upload(new GalleryUploadViewModel
        {
            RoomId = 1, IsCover = true,
            Files = new() { Photo("good.jpg"), new FailingPhoto() }
        }));
        Check(await fixture.Db.GalleryImages.CountAsync() == 1 && cover.IsCover);
        Check(Directory.GetFiles(fixture.Root, "*", SearchOption.AllDirectories).Length == 0);
    }),
    ("A database failure rolls back cover replacement and cleans all new files", async () =>
    {
        await using var fixture = await Fixture.Create();
        var cover = await fixture.SeedCover(1);
        fixture.Interceptor.Fail = true;
        await ExpectFailure(() => fixture.Controller.Upload(new GalleryUploadViewModel
        {
            RoomId = 1, IsCover = true, Files = new() { Photo("one.jpg"), Photo("two.jpg") }
        }));
        fixture.Db.ChangeTracker.Clear();
        Check(await fixture.Db.GalleryImages.CountAsync() == 1);
        Check((await fixture.Db.GalleryImages.FindAsync(cover.Id))!.IsCover);
        Check(Directory.GetFiles(fixture.Root, "*", SearchOption.AllDirectories).Length == 0);
    }),
    ("MVC collection and caption validation rejects missing files and oversized captions", () =>
    {
        foreach (var model in new object[]
        {
            new GalleryUploadViewModel(), new PropertyGalleryUploadViewModel(),
            new GalleryUploadViewModel { Files = new() { Photo("one.jpg") }, Caption = new string('x', 501) },
            new PropertyGalleryUploadViewModel { Files = new() { Photo("one.jpg") }, Caption = new string('x', 501) }
        })
            Check(!Validator.TryValidateObject(model, new ValidationContext(model), new List<ValidationResult>(), true));
        return Task.CompletedTask;
    })
};

var failures = 0;
foreach (var test in tests)
{
    try { await test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception ex) { failures++; Console.Error.WriteLine($"FAIL {test.Name}: {ex}"); }
}
Console.WriteLine($"{tests.Length - failures}/{tests.Length} checks passed.");
Environment.ExitCode = failures == 0 ? 0 : 1;

static IFormFile Photo(string name, long length = 4) => new FormFile(
    new MemoryStream(new byte[] { 1, 2, 3, 4 }), 0, length, "Upload.Files", name);

static void Check(bool condition)
{
    if (!condition) throw new InvalidOperationException("Assertion failed.");
}

static async Task ExpectFailure(Func<Task<IActionResult>> action)
{
    try { await action(); }
    catch (IOException) { return; }
    throw new InvalidOperationException("Expected an injected failure.");
}

sealed class Fixture : IAsyncDisposable
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), $"kk-gallery-tests-{Guid.NewGuid():N}");
    public SaveFailureInterceptor Interceptor { get; } = new();
    public AppDbContext Db { get; private set; } = default!;
    public GalleryController Controller { get; private set; } = default!;
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public static async Task<Fixture> Create()
    {
        var fixture = new Fixture();
        Directory.CreateDirectory(fixture.Root);
        await fixture._connection.OpenAsync();
        fixture.Db = new GalleryTestDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(fixture._connection).AddInterceptors(fixture.Interceptor).Options);
        await fixture.Db.Database.EnsureCreatedAsync();
        fixture.Db.Rooms.AddRange(
            new Room { Id = 1, Name = "Test room", Slug = "test-room", Description = "Fixture", IsPublished = true },
            new Room { Id = 2, Name = "Other room", Slug = "other-room", Description = "Fixture", IsPublished = true });
        await fixture.Db.SaveChangesAsync();
        var httpContext = new DefaultHttpContext();
        fixture.Controller = new GalleryController(fixture.Db, new TestEnvironment { WebRootPath = fixture.Root })
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, new MemoryTempDataProvider())
        };
        return fixture;
    }

    public async Task<GalleryImage> SeedCover(int roomId)
    {
        var image = new GalleryImage { RoomId = roomId, FileName = "existing.jpg", IsCover = true };
        Db.GalleryImages.Add(image);
        await Db.SaveChangesAsync();
        return image;
    }

    public async Task<List<GalleryImage>> SeedImages(int? roomId)
    {
        var images = Enumerable.Range(0, 3).Select(index => new GalleryImage
        {
            RoomId = roomId, FileName = $"fixture-{roomId}-{index}.jpg", SortOrder = index,
            IsCover = roomId.HasValue && index == 0
        }).ToList();
        Db.GalleryImages.AddRange(images);
        await Db.SaveChangesAsync();
        return images;
    }

    public Task<List<int>> PropertyOrder() => Db.GalleryImages.Where(image => image.RoomId == null)
        .OrderBy(image => image.SortOrder).ThenByDescending(image => image.Id).Select(image => image.Id).ToListAsync();

    public T InLanguage<T>(T controller, string language) where T : Controller
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?lang={language}");
        httpContext.Request.Headers.Cookie = language == "en" ? "kk_lang=bg" : "kk_lang=en";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _connection.DisposeAsync();
        if (Path.GetDirectoryName(Path.GetFullPath(Root)) !=
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath())))
            throw new InvalidOperationException("Fixture directory is outside the temporary folder.");
        Directory.Delete(Root, recursive: true);
    }
}

sealed class SaveFailureInterceptor : SaveChangesInterceptor
{
    public bool Fail { get; set; }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (Fail) throw new IOException("Injected database save failure.");
        return ValueTask.FromResult(result);
    }
}

sealed class GalleryTestDbContext : AppDbContext
{
    public GalleryTestDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // SQLite needs a numeric conversion for the existing room-price ORDER BY.
        modelBuilder.Entity<Room>().Property(room => room.PricePerNight)
            .HasConversion<double>().HasColumnType("REAL");
    }
}

sealed class FailingPhoto : IFormFile
{
    public string ContentType => "image/jpeg";
    public string ContentDisposition => "";
    public IHeaderDictionary Headers { get; } = new HeaderDictionary();
    public long Length => 4;
    public string Name => "Upload.Files";
    public string FileName => "failing.jpg";
    public Stream OpenReadStream() => throw new IOException("Injected copy failure.");
    public void CopyTo(Stream target) => throw new IOException("Injected copy failure.");
    public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        await target.WriteAsync(new byte[] { 1, 2 }, cancellationToken);
        throw new IOException("Injected partial copy failure.");
    }
}

sealed class MemoryTempDataProvider : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}

sealed class TestEnvironment : IWebHostEnvironment
{
    public string WebRootPath { get; set; } = "";
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string ApplicationName { get; set; } = "GalleryUpload.Tests";
    public string EnvironmentName { get; set; } = "Testing";
    public string ContentRootPath { get; set; } = "";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
