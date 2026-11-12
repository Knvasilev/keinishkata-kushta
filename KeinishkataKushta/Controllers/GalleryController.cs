using KeinishkataKushta.Data;
using KeinishkataKushta.Models;
using KeinishkataKushta.Infrastructure;
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
            var english = SiteText.Lang(HttpContext) == "en";
            var images = await _db.GalleryImages
                .AsNoTracking()
                .Where(image => image.RoomId == null)
                .OrderBy(image => image.SortOrder)
                .ThenByDescending(image => image.Id)
                .Select(image => new PropertyGalleryImageViewModel
                {
                    FileName = image.FileName,
                    Caption = english && !string.IsNullOrEmpty(image.CaptionEn) ? image.CaptionEn : image.Caption
                })
                .ToListAsync();

            return View(new PropertyGalleryViewModel { Images = images });
        }
    }
}
