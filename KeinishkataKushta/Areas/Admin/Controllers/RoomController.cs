using KeinishkataKushta.Data;
using KeinishkataKushta.Data.Entities;
using KeinishkataKushta.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeinishkataKushta.Web.Areas.Admin.Controllers  
{
    [Area("Admin")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]
    public class RoomController : Controller
    {
        private readonly AppDbContext _db;

        public RoomController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _db.Rooms
                .AsNoTracking()
                .OrderByDescending(r => r.Id)
                .Select(r => new RoomListItemViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Slug = r.Slug,
                    Capacity = r.Capacity,
                    PricePerNight = r.PricePerNight,
                    IsPublished = r.IsPublished,
                    CoverFileName =
                    r.GalleryImages.Where(g => g.IsCover).Select(g => g.FileName).FirstOrDefault()
                ?? r.GalleryImages.OrderByDescending(g => g.Id).Select(g => g.FileName).FirstOrDefault()
                })
                .ToListAsync();

            return View(rooms);
        }
        public IActionResult Create() => View(new RoomFormViewModel { IsPublished = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var slug = string.IsNullOrWhiteSpace(model.Slug) ? GenerateSlug(model.Name) : GenerateSlug(model.Slug);

            var slugExists = await _db.Rooms.AnyAsync(r => r.Slug == slug);
            if (slugExists)
            {
                ModelState.AddModelError(nameof(model.Slug), "This slug is already used. Please choose a different one.");
                return View(model);
            }

            var room = new KeinishkataKushta.Data.Entities.Room
            {
                Name = model.Name,
                Slug = slug,
                Description = model.Description ?? string.Empty,
                Capacity = model.Capacity,
                PricePerNight = model.PricePerNight,
                IsPublished = model.IsPublished
            };

            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Edit(int id)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            var model = new RoomFormViewModel
            {
                Id = room.Id,
                Name = room.Name,
                Slug = room.Slug,
                Description = room.Description,
                Capacity = room.Capacity,
                PricePerNight = room.PricePerNight,
                IsPublished = room.IsPublished
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoomFormViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            // Generate/normalize slug
            var slug = string.IsNullOrWhiteSpace(model.Slug)
                ? GenerateSlug(model.Name)
                : GenerateSlug(model.Slug);

            // Unique slug check (exclude current room)
            var slugExists = await _db.Rooms.AnyAsync(r => r.Id != model.Id && r.Slug == slug);
            if (slugExists)
            {
                ModelState.AddModelError(nameof(model.Slug), "This slug is already used. Please choose a different one.");
                return View(model);
            }

            // Load tracked entity and update it
            var room = await _db.Rooms.FindAsync(model.Id);
            if (room == null) return NotFound();

            room.Name = model.Name;
            room.Slug = slug;
            room.Description = model.Description ?? string.Empty;
            room.Capacity = model.Capacity;
            room.PricePerNight = model.PricePerNight;
            room.IsPublished = model.IsPublished;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private static string GenerateSlug(string input)
        {
            input = input.Trim().ToLowerInvariant();
            input = string.Join("-", input.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            while (input.Contains("--")) input = input.Replace("--", "-");
            return input;
        }



        public async Task<IActionResult> Delete(int id)
        {
            var room = await _db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (room == null) return NotFound();
            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}
