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
                .OrderByDescending(image => image.Id)
                .Select(image => new GalleryImageListItemViewModel
                {
                    Id = image.Id,
                    FileName = image.FileName,
                    Caption = image.Caption,
                    IsCover = image.IsCover
                })
                .ToListAsync();

            return View(new PropertyGalleryIndexViewModel { Images = images });
        }

        // POST: /Admin/Gallery/UploadProperty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProperty(
            [Bind(Prefix = "Upload")] PropertyGalleryUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Err"] = "Please choose an image file.";
                return RedirectToAction(nameof(Property));
            }

            var ext = Path.GetExtension(model.File.FileName);
            if (!AllowedExt.Contains(ext))
            {
                TempData["Err"] = "Only JPG, PNG, and WEBP images are allowed.";
                return RedirectToAction(nameof(Property));
            }

            if (model.File.Length == 0 || model.File.Length > 5 * 1024 * 1024)
            {
                TempData["Err"] = "The image must be between 1 byte and 5 MB.";
                return RedirectToAction(nameof(Property));
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "property");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
            var physicalPath = Path.Combine(uploadsRoot, fileName);

            await using (var stream = System.IO.File.Create(physicalPath))
            {
                await model.File.CopyToAsync(stream);
            }

            _db.GalleryImages.Add(new GalleryImage
            {
                RoomId = null,
                FileName = fileName,
                Caption = model.Caption?.Trim(),
                IsCover = false
            });

            await _db.SaveChangesAsync();
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
                .OrderByDescending(i => i.Id)
                .Select(i => new GalleryImageListItemViewModel
                {
                    Id = i.Id,
                    FileName = i.FileName,
                    Caption = i.Caption,
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
        public async Task<IActionResult> Upload([Bind(Prefix = "Upload")] GalleryUploadViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index), new { roomId = model.RoomId });

            var roomExists = await _db.Rooms.AnyAsync(r => r.Id == model.RoomId);
            if (!roomExists) return NotFound();

            var ext = Path.GetExtension(model.File.FileName);
            if (!AllowedExt.Contains(ext))
            {
                TempData["Err"] = "Only JPG, PNG, WEBP images are allowed.";
                return RedirectToAction(nameof(Index), new { roomId = model.RoomId });
            }

            if (model.File.Length > 5 * 1024 * 1024)
            {
                TempData["Err"] = "Max file size is 5 MB.";
                return RedirectToAction(nameof(Index), new { roomId = model.RoomId });
            }

            // ✅ If it's the first image, make it cover automatically (nice UX)
            var hasAny = await _db.GalleryImages.AnyAsync(i => i.RoomId == model.RoomId);
            var makeCover = model.IsCover || !hasAny;

            // ✅ If we are setting a new cover, unset all other covers for this room
            if (makeCover)
            {
                await _db.GalleryImages
                    .Where(i => i.RoomId == model.RoomId && i.IsCover)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsCover, false));
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "rooms", model.RoomId.ToString());
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var physicalPath = Path.Combine(uploadsRoot, fileName);

            await using (var stream = System.IO.File.Create(physicalPath))
            {
                await model.File.CopyToAsync(stream);
            }

            var image = new GalleryImage
            {
                RoomId = model.RoomId,   // int -> int? OK
                FileName = fileName,
                Caption = model.Caption,
                IsCover = makeCover       // ✅ save cover state
            };

            _db.GalleryImages.Add(image);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { roomId = model.RoomId });
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
