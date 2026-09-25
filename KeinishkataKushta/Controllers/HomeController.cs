using System.Diagnostics;
using KeinishkataKushta.Data;
using KeinishkataKushta.Data.Entities;
using KeinishkataKushta.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeinishkataKushta.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var publishedRooms = _db.Rooms
                .AsNoTracking()
                .Where(r => r.IsPublished);

            var summary = await publishedRooms
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    TotalRooms = group.Count(),
                    MaxRoomCapacity = group.Max(room => room.Capacity),
                    StartingPricePerNight = group.Min(room => room.PricePerNight)
                })
                .SingleOrDefaultAsync();

            var rooms = await publishedRooms
                .OrderBy(r => r.PricePerNight)
                .Take(3)
                .Select(r => new RoomCardViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Slug = r.Slug,
                    Description = r.Description,
                    Capacity = r.Capacity,
                    PricePerNight = r.PricePerNight,
                    CoverFileName = r.GalleryImages
                        .Where(g => g.IsCover)
                        .Select(g => g.FileName)
                        .FirstOrDefault()
                        ?? r.GalleryImages
                            .OrderByDescending(g => g.Id)
                            .Select(g => g.FileName)
                            .FirstOrDefault()
                })
                .ToListAsync();

            return View(new HomeIndexViewModel
            {
                FeaturedRooms = rooms,
                TotalRooms = summary?.TotalRooms ?? 0,
                MaxRoomCapacity = summary?.MaxRoomCapacity ?? 0,
                StartingPricePerNight = summary?.StartingPricePerNight ?? 0
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _db.ContactMessages.Add(new ContactMessage
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Message = model.Message,
                CreatedUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            TempData["ContactSuccess"] = true;
            return RedirectToAction(nameof(Contact));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
