using KeinishkataKushta.Data;
using KeinishkataKushta.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeinishkataKushta.Controllers
{
    public class GalleryController : Controller
    {
        private readonly AppDbContext _db;

        public GalleryController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var images = await _db.GalleryImages
                .AsNoTracking()
                .Where(image => image.RoomId == null)
                .OrderByDescending(image => image.Id)
                .Select(image => new PropertyGalleryImageViewModel
                {
                    FileName = image.FileName,
                    Caption = image.Caption
                })
                .ToListAsync();

            return View(new PropertyGalleryViewModel { Images = images });
        }
    }
}
