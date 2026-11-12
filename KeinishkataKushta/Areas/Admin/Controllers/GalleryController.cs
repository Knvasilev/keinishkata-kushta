using KeinishkataKushta.Data;
using KeinishkataKushta.Data.Entities;
using KeinishkataKushta.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeinishkataKushta.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]
    public class GalleryController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp" };

        public GalleryController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: /Admin/Gallery/Property
        public async Task<IActionResult> Property()
        {
            var images = await _db.GalleryImages
                .AsNoTracking()
                .Where(image => image.RoomId == null)
                .OrderBy(image => image.SortOrder)
                .ThenByDescending(image => image.Id)
                .Select(image => new GalleryImageListItemViewModel
                {
                    Id = image.Id,
                    FileName = image.FileName,
                    Caption = image.Caption,
                    CaptionEn = image.CaptionEn,
                    IsCover = image.IsCover
                })
                .ToListAsync();

            return View(new PropertyGalleryIndexViewModel { Images = images });
        }

        // POST: /Admin/Gallery/UploadProperty
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(105 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 105 * 1024 * 1024)]
        public async Task<IActionResult> UploadProperty(
            [Bind(Prefix = "Upload")] PropertyGalleryUploadViewModel model)
        {
            var error = GetUploadError(model.Files);
            if (!ModelState.IsValid || error != null)
            {
                TempData["Err"] = error ?? "Each caption must be no more than 500 characters.";
                return RedirectToAction(nameof(Property));
            }

            await SaveUploadsAsync(model.Files, null, model.Caption, model.CaptionEn, false);
            TempData["Ok"] = $"Uploaded {model.Files.Count} photo(s).";
            return RedirectToAction(nameof(Property));
        }

        // POST: /Admin/Gallery/DeleteProperty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var image = await _db.GalleryImages
                .FirstOrDefaultAsync(item => item.Id == id && item.RoomId == null);

            if (image == null) return NotFound();

            var physicalPath = Path.Combine(
                _env.WebRootPath,
                "uploads",
                "property",
                image.FileName);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            _db.GalleryImages.Remove(image);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Property));
        }

        // GET: /Admin/Gallery?roomId=5
        public async Task<IActionResult> Index(int roomId)
        {
            var room = await _db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null) return NotFound();

            var images = await _db.GalleryImages
                .AsNoTracking()
                .Where(i => i.RoomId == roomId) // <-- if your FK name differs, change here
                .OrderBy(i => i.SortOrder)
                .ThenByDescending(i => i.Id)
                .Select(i => new GalleryImageListItemViewModel
                {
                    Id = i.Id,
                    FileName = i.FileName,
                    Caption = i.Caption,
                    CaptionEn = i.CaptionEn,
                    IsCover = i.IsCover
                })
                .ToListAsync();

            var vm = new GalleryIndexViewModel
            {
                RoomId = room.Id,
                RoomName = room.Name,
                Upload = new GalleryUploadViewModel { RoomId = room.Id },
                Images = images
            };

            return View(vm);
        }

        // POST: /Admin/Gallery/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(105 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 105 * 1024 * 1024)]
        public async Task<IActionResult> Upload([Bind(Prefix = "Upload")] GalleryUploadViewModel model)
        {
            var error = GetUploadError(model.Files);
            if (!ModelState.IsValid || error != null)
            {
                TempData["Err"] = error ?? "Please check the room and caption (maximum 500 characters).";
                return RedirectToAction(nameof(Index), new { roomId = model.RoomId });
            }

            var roomExists = await _db.Rooms.AnyAsync(r => r.Id == model.RoomId);
            if (!roomExists) return NotFound();

            await SaveUploadsAsync(model.Files, model.RoomId, model.Caption, model.CaptionEn, model.IsCover);
            TempData["Ok"] = $"Uploaded {model.Files.Count} photo(s).";

            return RedirectToAction(nameof(Index), new { roomId = model.RoomId });
        }

        private static string? GetUploadError(IReadOnlyCollection<IFormFile>? files)
        {
            if (files == null || files.Count == 0)
                return "Please choose at least one image file.";
            if (files.Count > 20)
                return "Please upload no more than 20 photos at a time.";

            foreach (var file in files)
            {
                if (!AllowedExt.Contains(Path.GetExtension(file.FileName)))
                    return "Only JPG, PNG, and WEBP images are allowed. No photos were uploaded.";
                if (file.Length == 0 || file.Length > 5 * 1024 * 1024)
                    return "Each image must be between 1 byte and 5 MB. No photos were uploaded.";
            }

            return null;
        }

        private async Task SaveUploadsAsync(List<IFormFile> files, int? roomId, string? caption, string? captionEn, bool isCover)
        {
            var uploadsRoot = roomId.HasValue
                ? Path.Combine(_env.WebRootPath, "uploads", "rooms", roomId.Value.ToString())
                : Path.Combine(_env.WebRootPath, "uploads", "property");
            Directory.CreateDirectory(uploadsRoot);

            var physicalPaths = new List<string>();
            var images = new List<GalleryImage>();
            try
            {
                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var physicalPath = Path.Combine(uploadsRoot, fileName);
                    physicalPaths.Add(physicalPath);

                    await using (var stream = System.IO.File.Create(physicalPath))
                    {
                        await file.CopyToAsync(stream);
                    }

                    images.Add(new GalleryImage
                    {
                        RoomId = roomId,
                        FileName = fileName,
                        Caption = caption?.Trim(),
                        CaptionEn = string.IsNullOrWhiteSpace(captionEn) ? null : captionEn.Trim(),
                        IsCover = false
                    });
                }

                // Cover replacement and the whole batch commit together.
                await using var transaction = await _db.Database.BeginTransactionAsync();
                var lastPosition = await _db.GalleryImages
                    .Where(image => image.RoomId == roomId)
                    .MaxAsync(image => (int?)image.SortOrder) ?? -1;
                for (var index = 0; index < images.Count; index++)
                    images[index].SortOrder = lastPosition + index + 1;

                if (roomId.HasValue)
                {
                    var hasAny = await _db.GalleryImages.AnyAsync(image => image.RoomId == roomId);
                    if (isCover || !hasAny)
                    {
                        await _db.GalleryImages
                            .Where(image => image.RoomId == roomId && image.IsCover)
                            .ExecuteUpdateAsync(setters => setters.SetProperty(image => image.IsCover, false));
                        images[0].IsCover = true;
                    }
                }

                _db.GalleryImages.AddRange(images);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                foreach (var physicalPath in physicalPaths)
                {
                    if (System.IO.File.Exists(physicalPath))
                        System.IO.File.Delete(physicalPath);
                }

                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPosition(int id, int position, int? roomId)
        {
            if (!ModelState.IsValid || position < 1) return BadRequest();

            var images = await _db.GalleryImages
                .Where(image => image.RoomId == roomId)
                .OrderBy(image => image.SortOrder)
                .ThenByDescending(image => image.Id)
                .ToListAsync();
            var image = images.FirstOrDefault(item => item.Id == id);
            if (image == null) return NotFound();
            if (position > images.Count) return BadRequest();

            images.Remove(image);
            images.Insert(position - 1, image);
            for (var index = 0; index < images.Count; index++)
                images[index].SortOrder = index;

            await _db.SaveChangesAsync();
            TempData["Ok"] = $"Photo moved to position {position}.";
            return roomId.HasValue
                ? RedirectToAction(nameof(Index), new { roomId })
                : RedirectToAction(nameof(Property));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCaptions(GalleryImageCaptionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Err"] = "Each caption must be no more than 500 characters.";
                return model.RoomId.HasValue
                    ? RedirectToAction(nameof(Index), new { roomId = model.RoomId })
                    : RedirectToAction(nameof(Property));
            }

            var image = await _db.GalleryImages
                .FirstOrDefaultAsync(item => item.Id == model.Id && item.RoomId == model.RoomId);
            if (image == null) return NotFound();

            image.Caption = string.IsNullOrWhiteSpace(model.Caption) ? null : model.Caption.Trim();
            image.CaptionEn = string.IsNullOrWhiteSpace(model.CaptionEn) ? null : model.CaptionEn.Trim();
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Photo captions saved.";
            return model.RoomId.HasValue
                ? RedirectToAction(nameof(Index), new { roomId = model.RoomId })
                : RedirectToAction(nameof(Property));
        }



        // POST: /Admin/Gallery/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int roomId)
        {
            var image = await _db.GalleryImages.FirstOrDefaultAsync(i => i.Id == id && i.RoomId == roomId);
            if (image == null) return NotFound();

            var wasCover = image.IsCover;

            var physicalPath = Path.Combine(_env.WebRootPath, "uploads", "rooms", roomId.ToString(), image.FileName);
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            _db.GalleryImages.Remove(image);

            if (wasCover)
            {
                var nextCover = await _db.GalleryImages
                    .Where(i => i.RoomId == roomId && i.Id != id)
                    .OrderByDescending(i => i.Id)
                    .FirstOrDefaultAsync();

                if (nextCover != null)
                {
                    nextCover.IsCover = true;
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { roomId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeCover(int id)
        {
            var image = await _db.GalleryImages.FirstOrDefaultAsync(i => i.Id == id);
            if (image == null) return NotFound();
            if (!image.RoomId.HasValue) return BadRequest();

            var roomId = image.RoomId.Value;

            await _db.GalleryImages
                .Where(i => i.RoomId == roomId && i.Id != id && i.IsCover)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsCover, false));

            image.IsCover = true;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { roomId });
        }

    }
}
