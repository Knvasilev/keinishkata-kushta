using System.ComponentModel.DataAnnotations;
using System.Reflection;
using KeinishkataKushta.Data;
using KeinishkataKushta.Data.Entities;
using KeinishkataKushta.Infrastructure;
using KeinishkataKushta.Models;
using KeinishkataKushta.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminController = KeinishkataKushta.Web.Areas.Admin.Controllers.AvailabilityController;

if (args.Contains("--preview"))
{
    await PreviewHost.Run(args.Where(arg => arg != "--preview").ToArray());
    return;
}

using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer("Server=localhost;Database=AvailabilityTests;Integrated Security=true;TrustServerCertificate=true").Options);
var arrival = new DateOnly(2028, 2, 28);
var departure = new DateOnly(2028, 3, 2);
var existing = new AvailabilityBlock { Id = 1, Arrival = arrival, Departure = departure, RoomsUsed = "Room A" };
var tests = new (string Name, Action Run)[]
{
    ("Overlap is rejected regardless of rooms included", () =>
    {
        var conflict = AvailabilityRules.Conflicts(arrival.AddDays(1), departure.AddDays(1)).Compile();
        Check(conflict(existing));
        existing.RoomsUsed = "Room B";
        Check(conflict(existing));
        existing.RoomsUsed = null;
        Check(conflict(existing));
    }),
    ("Identical and containing ranges conflict", () =>
    {
        Check(AvailabilityRules.Conflicts(arrival, departure).Compile()(existing));
        Check(AvailabilityRules.Conflicts(arrival.AddDays(-1), departure.AddDays(1)).Compile()(existing));
        Check(AvailabilityRules.Conflicts(arrival.AddDays(1), departure.AddDays(-1)).Compile()(existing));
    }),
    ("Consecutive stays are allowed at either end", () =>
    {
        Check(!AvailabilityRules.Conflicts(departure, departure.AddDays(2)).Compile()(existing));
        Check(!AvailabilityRules.Conflicts(arrival.AddDays(-2), arrival).Compile()(existing));
    }),
    ("Edit excludes only its own record", () =>
    {
        Check(!AvailabilityRules.Conflicts(arrival, departure, 1).Compile()(existing));
        Check(AvailabilityRules.Conflicts(arrival, departure, 2).Compile()(existing));
    }),
    ("Leap day and cross-month occupancy exclude checkout", () =>
    {
        var blocks = new[] { new BlockedNights(arrival, departure, AvailabilityBlockKind.Reservation) };
        var feb = AvailabilityCalendar.BuildMonth(new DateOnly(2028, 2, 1), arrival, blocks);
        Check(feb.Days.Count == 29 && feb.LeadingDays == 1);
        Check(feb.Days[28].Status == "Occupied" && feb.Days[0].Status == "Past");
        var march = AvailabilityCalendar.BuildMonth(new DateOnly(2028, 3, 1), arrival, blocks);
        Check(march.Days[0].Status == "Occupied" && march.Days[1].Status == "Available");
    }),
    ("Maintenance blocks the house and respects night boundaries", () =>
    {
        var blocks = new[] { new BlockedNights(arrival, departure, AvailabilityBlockKind.Maintenance) };
        var month = AvailabilityCalendar.BuildMonth(new DateOnly(2028, 3, 1), arrival, blocks);
        Check(month.Days[0].Status == "Unavailable" && month.Days[1].Status == "Available");
    }),
    ("December and January calendars retain Monday-first positioning", () =>
    {
        var month = AvailabilityCalendar.BuildMonth(new DateOnly(2026, 12, 1), arrival, []);
        var next = AvailabilityCalendar.BuildMonth(month.Month.AddMonths(1), arrival, []);
        Check(month.Days.Count == 31 && month.LeadingDays == 1);
        Check(next.Month.Year == 2027 && next.LeadingDays == 4);
    }),
    ("Public projection contains no private notes or room details", () =>
    {
        var properties = typeof(BlockedNights).GetProperties().Select(property => property.Name).Order().ToArray();
        Check(properties.SequenceEqual(new[] { "Arrival", "Departure", "Kind" }));
    }),
    ("SQL provider translates overlap rules and date projection", () =>
    {
        var sql = db.AvailabilityBlocks.Where(AvailabilityRules.Conflicts(arrival, departure))
            .Select(block => new BlockedNights(block.Arrival, block.Departure, block.Kind)).ToQueryString();
        Check(sql.Contains("[Arrival] <") && sql.Contains("[Departure] >"));
        Check(!sql.Contains("[Notes]") && !sql.Contains("[RoomsUsed]"));
    }),
    ("Migration matches the EF model and includes SQL constraints", () =>
    {
        Check(!db.Database.HasPendingModelChanges());
        var sql = db.Database.GenerateCreateScript();
        Check(sql.Contains("CK_AvailabilityBlocks_Dates") && sql.Contains("CK_AvailabilityBlocks_Kind"));
        Check(sql.Contains("rowversion"));
        Check(db.Database.GetMigrations().Any(migration => migration.EndsWith("_AddPropertyAvailability")));
    }),
    ("Admin writes require authorization, POST, and antiforgery", () =>
    {
        Check(typeof(AdminController).GetCustomAttribute<AuthorizeAttribute>()?.Policy == "AdminOnly");
        foreach (var name in new[] { "Save", "DeleteConfirmed" })
        {
            var method = typeof(AdminController).GetMethod(name)!;
            Check(method.GetCustomAttribute<HttpPostAttribute>() != null);
            Check(method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() != null);
        }
    }),
    ("Form rejects invalid status, oversized notes, and missing dates", () =>
    {
        var model = new AvailabilityFormViewModel { Kind = (AvailabilityBlockKind)9, Notes = new string('x', 1001) };
        var errors = new List<ValidationResult>();
        Check(!Validator.TryValidateObject(model, new ValidationContext(model), errors, true));
        Check(errors.Count >= 4);
    }),
    ("Save rejects zero-night stays before accessing the database", () =>
    {
        var controller = new AdminController(db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var today = AvailabilityCalendar.Today();
        var result = controller.Save(new AvailabilityFormViewModel { Arrival = today, Departure = today })
            .GetAwaiter().GetResult();
        Check(result is ViewResult && !controller.ModelState.IsValid);
    }),
    ("Public calendar rejects malformed and out-of-range months", () =>
    {
        var controller = new KeinishkataKushta.Controllers.AvailabilityController(db);
        foreach (var month in new[] { "not-a-date", "2026-99", "0001-01", "9999-12" })
            Check(controller.Index(month).GetAwaiter().GetResult() is BadRequestResult);
    })
};

var failures = 0;
foreach (var test in tests)
{
    try { test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception ex) { failures++; Console.Error.WriteLine($"FAIL {test.Name}: {ex.Message}"); }
}
Console.WriteLine($"{tests.Length - failures}/{tests.Length} checks passed.");
Environment.ExitCode = failures == 0 ? 0 : 1;

static void Check(bool condition)
{
    if (!condition) throw new InvalidOperationException("Assertion failed.");
}
