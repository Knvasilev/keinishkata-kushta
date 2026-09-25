using KeinishkataKushta.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeinishkataKushta.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]
    public class ContactMessagesController : Controller
    {
        private readonly AppDbContext _db;

        public ContactMessagesController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _db.ContactMessages
                .AsNoTracking()
                .OrderByDescending(m => m.CreatedUtc)
                .ThenByDescending(m => m.Id)
                .ToListAsync();

            return View(messages);
        }

        public async Task<IActionResult> Details(int id)
        {
            var message = await _db.ContactMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null) return NotFound();

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _db.ContactMessages.FindAsync(id);
            if (message == null) return NotFound();

            _db.ContactMessages.Remove(message);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
