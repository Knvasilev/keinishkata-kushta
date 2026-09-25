using System.Data;
using KeinishkataKushta.Data;
using KeinishkataKushta.Data.Entities;
using KeinishkataKushta.Infrastructure;
using KeinishkataKushta.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KeinishkataKushta.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AvailabilityController : Controller
{
    private readonly AppDbContext _db;
    public AvailabilityController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(bool history = false)
    {
        ViewData["History"] = history;
        var today = AvailabilityCalendar.Today();
        return View(await _db.AvailabilityBlocks.AsNoTracking()
            .Where(block => history || block.Departure > today)
            .OrderBy(block => block.Arrival).ThenBy(block => block.Id).ToListAsync());
    }

    [HttpGet]
    public IActionResult Create()
    {
        var today = AvailabilityCalendar.Today();
        return View("Form", new AvailabilityFormViewModel
        {
            Arrival = today, Departure = today.AddDays(1), RoomsUsed = SiteText.T(HttpContext, "WholeHouse")
        });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var block = await _db.AvailabilityBlocks.AsNoTracking().SingleOrDefaultAsync(block => block.Id == id);
        if (block == null) return NotFound();
        return View("Form", new AvailabilityFormViewModel
        {
            Id = block.Id, RoomsUsed = block.RoomsUsed, Arrival = block.Arrival, Departure = block.Departure,
            Kind = block.Kind, Notes = block.Notes, Version = Convert.ToBase64String(block.Version)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(AvailabilityFormViewModel model)
    {
        if (model.Id < 0) return BadRequest();
        if (model.Arrival.HasValue && model.Departure.HasValue && model.Departure <= model.Arrival)
            ModelState.AddModelError(nameof(model.Departure), SiteText.T(HttpContext, "AvailabilityInvalidDates"));
        var version = ParseVersion(model.Version);
        if (model.Id != 0 && version == null) return BadRequest();
        if (!ModelState.IsValid) return View("Form", model);

        try
        {
            // Serializable range locks protect the overlap check from simultaneous saves.
            await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var block = model.Id == 0 ? new AvailabilityBlock()
                : await _db.AvailabilityBlocks.SingleOrDefaultAsync(block => block.Id == model.Id);
            if (block == null) return NotFound();
            if (await _db.AvailabilityBlocks.AnyAsync(AvailabilityRules.Conflicts(
                model.Arrival!.Value, model.Departure!.Value, model.Id)))
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, SiteText.T(HttpContext, "AvailabilityConflict"));
                return View("Form", model);
            }

            if (model.Id == 0) _db.AvailabilityBlocks.Add(block);
            else _db.Entry(block).Property(block => block.Version).OriginalValue = version!;
            block.RoomsUsed = model.RoomsUsed?.Trim();
            block.Arrival = model.Arrival.Value;
            block.Departure = model.Departure.Value;
            block.Kind = model.Kind;
            block.Notes = model.Notes?.Trim();
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["AvailabilitySaved"] = true;
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError(string.Empty, SiteText.T(HttpContext, "AvailabilityChanged"));
        }
        catch (Exception ex) when (ex is SqlException { Number: 1205 }
            || ex is DbUpdateException { InnerException: SqlException { Number: 1205 or 547 } })
        {
            ModelState.AddModelError(string.Empty, SiteText.T(HttpContext, "AvailabilityRetry"));
        }
        _db.ChangeTracker.Clear();
        return View("Form", model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var block = await _db.AvailabilityBlocks.AsNoTracking()
            .SingleOrDefaultAsync(block => block.Id == id);
        return block == null ? NotFound() : View(block);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, string? version)
    {
        var originalVersion = ParseVersion(version);
        if (originalVersion == null) return BadRequest();
        var block = await _db.AvailabilityBlocks.SingleOrDefaultAsync(block => block.Id == id);
        if (block == null) return NotFound();
        _db.Entry(block).Property(block => block.Version).OriginalValue = originalVersion;
        _db.AvailabilityBlocks.Remove(block);
        try
        {
            await _db.SaveChangesAsync();
            TempData["AvailabilitySaved"] = true;
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["AvailabilityChanged"] = true;
        }
        return RedirectToAction(nameof(Index));
    }

    private static byte[]? ParseVersion(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        try
        {
            var version = Convert.FromBase64String(value);
            return version.Length == 8 ? version : null;
        }
        catch (FormatException) { return null; }
    }
}
