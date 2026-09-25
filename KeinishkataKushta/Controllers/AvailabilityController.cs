using System.Globalization;
using KeinishkataKushta.Data;
using KeinishkataKushta.Infrastructure;
using KeinishkataKushta.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeinishkataKushta.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AvailabilityController : Controller
{
    private readonly AppDbContext _db;

    public AvailabilityController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(string? month)
    {
        if (!ModelState.IsValid) return BadRequest();

        var today = AvailabilityCalendar.Today();
        var first = new DateOnly(today.Year, today.Month, 1);
        var last = first.AddMonths(11);
        var selected = first;
        if (month != null && (!DateOnly.TryParseExact(month + "-01", "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out selected) || selected < first || selected > last))
            return BadRequest();

        var end = selected.AddMonths(selected == last ? 1 : 2);
        // Only dates and status reach the public view. Notes never leave Admin.
        var blocks = await _db.AvailabilityBlocks.AsNoTracking()
            .Where(AvailabilityRules.Conflicts(selected, end))
            .Select(block => new BlockedNights(block.Arrival, block.Departure, block.Kind)).ToListAsync();
        var months = new List<CalendarMonth> { AvailabilityCalendar.BuildMonth(selected, today, blocks) };
        if (selected < last) months.Add(AvailabilityCalendar.BuildMonth(selected.AddMonths(1), today, blocks));

        var model = new AvailabilityViewModel
        {
            Today = today, Month = selected, FirstMonth = first, LastMonth = last, Months = months
        };
        return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
            ? PartialView("_Calendar", model)
            : View(model);
    }
}
