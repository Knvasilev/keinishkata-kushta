using KeinishkataKushta.Data;
using KeinishkataKushta.Models;
using KeinishkataKushta.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeinishkataKushta.Controllers
{
    public class RoomsController : Controller
    {
        private readonly AppDbContext _db;

        public RoomsController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var english = SiteText.Lang(HttpContext) == "en";
            var rooms = await _db.Rooms
                .AsNoTracking()
                .Where(r => r.IsPublished)
                .OrderBy(r => r.PricePerNight)
                .Select(r => new RoomCardViewModel
                {
                    Id = r.Id,
                    Name = english && !string.IsNullOrEmpty(r.NameEn) ? r.NameEn : r.Name,
                    Slug = r.Slug,
                    Description = english && !string.IsNullOrEmpty(r.DescriptionEn) ? r.DescriptionEn : r.Description,
                    Capacity = r.Capacity,
                    PricePerNight = r.PricePerNight,
                    CoverFileName = r.GalleryImages
                        .Where(g => g.IsCover)
                        .Select(g => g.FileName)
                        .FirstOrDefault()
                        ?? r.GalleryImages
                            .OrderBy(g => g.SortOrder)
                            .ThenByDescending(g => g.Id)
                            .Select(g => g.FileName)
                            .FirstOrDefault()
                })
                .ToListAsync();

            return View(rooms);
        }

        [Route("Rooms/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var english = SiteText.Lang(HttpContext) == "en";
            var room = await _db.Rooms
                .AsNoTracking()
                .Where(r => r.IsPublished && r.Slug == slug)
                .Select(r => new RoomDetailsViewModel
                {
                    Id = r.Id,
                    Name = english && !string.IsNullOrEmpty(r.NameEn) ? r.NameEn : r.Name,
                    Slug = r.Slug,
                    Description = english && !string.IsNullOrEmpty(r.DescriptionEn) ? r.DescriptionEn : r.Description,
                    Capacity = r.Capacity,
                    PricePerNight = r.PricePerNight,
                    Images = r.GalleryImages
                        .OrderBy(g => g.SortOrder)
                        .ThenByDescending(g => g.Id)
                        .Select(g => new RoomImageViewModel
                        {
                            FileName = g.FileName,
                            Caption = english && !string.IsNullOrEmpty(g.CaptionEn) ? g.CaptionEn : g.Caption,
                            IsCover = g.IsCover
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (room == null) return NotFound();

            return View(room);
        }
    }
}
