using KeinishkataKushta.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

static class CaptionBatchChecks
{
    static void Check(bool condition) { if (!condition) throw new Exception("Bulk caption assertion failed."); }
    static GalleryCaptionBatchViewModel Batch(IEnumerable<KeinishkataKushta.Data.Entities.GalleryImage> images, int? roomId) => new()
    {
        RoomId = roomId,
        Rows = images.Select(image => new GalleryCaptionRowViewModel
        {
            Id = image.Id, Caption = image.Caption, CaptionEn = image.CaptionEn,
            OriginalCaption = image.Caption, OriginalCaptionEn = image.CaptionEn
        }).ToList()
    };

    public static async Task MultipleAndPublic()
    {
        foreach (var roomId in new int?[] { null, 1 })
        {
            await using var fixture = await Fixture.Create();
            var images = await fixture.SeedImages(roomId);
            var before = images.Select(image => (image.Id, image.FileName, image.RoomId, image.SortOrder, image.IsCover)).ToArray();
            var batch = Batch(images, roomId);
            batch.Rows[0].Caption = "  Двор  "; batch.Rows[0].CaptionEn = "  Courtyard  ";
            batch.Rows[1].Caption = "Механа";
            Check(await fixture.Controller.SaveCaptions(batch) is RedirectToActionResult);
            fixture.Db.ChangeTracker.Clear();
            var saved = await fixture.Db.GalleryImages.OrderBy(image => image.Id).ToListAsync();
            Check(saved.Select(image => (image.Id, image.FileName, image.RoomId, image.SortOrder, image.IsCover)).SequenceEqual(before));
            Check(saved[0].Caption == "Двор" && saved[0].CaptionEn == "Courtyard");
            Check(saved[1].Caption == "Механа" && saved[1].CaptionEn == null && saved[2].Caption == null);
            foreach (var lang in new[] { "bg", "en" })
            {
                var captions = roomId.HasValue
                    ? ((KeinishkataKushta.Models.RoomDetailsViewModel)((ViewResult)await fixture.InLanguage(
                        new KeinishkataKushta.Controllers.RoomsController(fixture.Db), lang).Details("test-room")).Model!).Images.Select(image => image.Caption).ToList()
                    : ((KeinishkataKushta.Models.PropertyGalleryViewModel)((ViewResult)await fixture.InLanguage(
                        new KeinishkataKushta.Controllers.GalleryController(fixture.Db), lang).Index()).Model!).Images.Select(image => image.Caption).ToList();
                Check(captions[0] == (lang == "en" ? "Courtyard" : "Двор") && captions[1] == "Механа");
            }
        }
    }

    public static async Task SelectiveAndStale()
    {
        await using var fixture = await Fixture.Create();
        var images = await fixture.SeedImages(null);
        images[0].Caption = "Original"; images[0].CaptionEn = "English";
        await fixture.Db.SaveChangesAsync();
        var batch = Batch(images, null);
        // Another session changes an untouched field: the batch must preserve it.
        await fixture.Db.GalleryImages.Where(image => image.Id == images[0].Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(image => image.CaptionEn, "Other session"));
        batch.Rows[0].Caption = "Edited Bulgarian";
        Check(await fixture.Controller.SaveCaptions(batch) is RedirectToActionResult);
        fixture.Db.ChangeTracker.Clear();
        var saved = (await fixture.Db.GalleryImages.FindAsync(images[0].Id))!;
        Check(saved.Caption == "Edited Bulgarian" && saved.CaptionEn == "Other session");
        batch = Batch(await fixture.Db.GalleryImages.ToListAsync(), null);
        batch.Rows[0].Caption = "Must roll back";
        batch.Rows[1].CaptionEn = "Stale edit";
        await fixture.Db.GalleryImages.Where(image => image.Id == images[1].Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(image => image.CaptionEn, "Newer translation"));
        Check(await fixture.Controller.SaveCaptions(batch) is ViewResult);
        fixture.Db.ChangeTracker.Clear();
        Check((await fixture.Db.GalleryImages.FindAsync(images[0].Id))!.Caption == "Edited Bulgarian");
        Check((await fixture.Db.GalleryImages.FindAsync(images[1].Id))!.CaptionEn == "Newer translation");
        Check(ReferenceEquals(fixture.Controller.ViewData["CaptionDraft"], batch));
    }

    public static async Task ValidationAndScope()
    {
        await using var fixture = await Fixture.Create();
        var property = await fixture.SeedImages(null);
        var room = await fixture.SeedImages(1);
        var batch = Batch(property, null);
        batch.Rows[0].CaptionEn = "Valid draft";
        batch.Rows[1].Caption = new string('x', 501);
        Check(await fixture.Controller.SaveCaptions(batch) is ViewResult);
        Check(ReferenceEquals(fixture.Controller.ViewData["CaptionDraft"], batch));
        Check(await fixture.Db.GalleryImages.AllAsync(image => image.Caption == null && image.CaptionEn == null));
        fixture.Controller.ModelState.Clear();
        batch = Batch(property, null);
        batch.Rows.Add(Batch(room.Take(1), 1).Rows[0]);
        Check(await fixture.Controller.SaveCaptions(batch) is BadRequestResult);
        batch = Batch(property, null); batch.Rows.Add(batch.Rows[0]);
        Check(await fixture.Controller.SaveCaptions(batch) is BadRequestResult);
        batch = Batch(property, 2);
        Check(await fixture.Controller.SaveCaptions(batch) is BadRequestResult);
        Check(await fixture.Controller.SaveCaptions(new()) is BadRequestResult);
        batch = Batch(property.Take(1), null);
        batch.Rows[0].CaptionEn = "Only this photo";
        Check(await fixture.Controller.SaveCaptions(batch) is RedirectToActionResult);
        Check(await fixture.Db.GalleryImages.CountAsync(image => image.CaptionEn != null) == 1);
        fixture.Db.ChangeTracker.Clear();
        var saved = (await fixture.Db.GalleryImages.FindAsync(property[0].Id))!;
        batch = Batch(new[] { saved }, null); batch.Rows[0].CaptionEn = "  ";
        Check(await fixture.Controller.SaveCaptions(batch) is RedirectToActionResult);
        Check(await fixture.Db.GalleryImages.AllAsync(image => image.CaptionEn == null));
        var action = typeof(KeinishkataKushta.Web.Areas.Admin.Controllers.GalleryController).GetMethod("SaveCaptions")!;
        Check(action.IsDefined(typeof(HttpPostAttribute), true) && action.IsDefined(typeof(ValidateAntiForgeryTokenAttribute), true));
    }
}
