using System.Globalization;
using KeinishkataKushta.Data;
using KeinishkataKushta.Data.Entities;
using KeinishkataKushta.Infrastructure;
using KeinishkataKushta.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

// Isolated visual fixture host. Never used by the website or connected to its database.
internal static class PreviewHost
{
    public static async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args, ApplicationName = typeof(KeinishkataKushta.Program).Assembly.FullName
        });
        builder.WebHost.UseUrls("http://127.0.0.1:5192");
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
            "Server=localhost;Database=UnusedPreview;Integrated Security=true"));
        builder.Services.AddControllersWithViews(options => options.Filters.Add<FixtureFilter>());
        var app = builder.Build();
        app.UseStaticFiles();
        // Generated preview graphics only; never read or create runtime uploads.
        static IResult FixtureImage(string fileName)
        {
            if (!fileName.StartsWith("fixture-", StringComparison.Ordinal) || !fileName.EndsWith(".svg", StringComparison.Ordinal))
                return Results.NotFound();
            var width = fileName.Contains("-2.") ? 600 : 1200;
            var height = fileName.Contains("-3.") ? 400 : 800;
            return Results.Content($"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}' viewBox='0 0 {width} {height}'><rect width='100%' height='100%' fill='#295333'/><rect x='8' y='8' width='{width - 16}' height='{height - 16}' fill='none' stroke='#b69150' stroke-width='16'/><text x='50%' y='50%' text-anchor='middle' font-size='40' fill='white'>Preview {width} x {height}</text></svg>", "image/svg+xml");
        }
        app.MapGet("/uploads/rooms/0/{fileName}", FixtureImage);
        app.MapGet("/uploads/property/{fileName}", FixtureImage);
        app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
        await app.RunAsync();
    }

    private sealed class FixtureFilter : IAsyncResourceFilter, IOrderedFilter
    {
        public int Order => int.MinValue;
        public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (context.RouteData.Values["controller"]?.ToString() == "Gallery"
                && context.RouteData.Values["action"]?.ToString() == "Index"
                && !context.RouteData.Values.ContainsKey("area") && HttpMethods.IsGet(context.HttpContext.Request.Method))
            {
                var english = SiteText.Lang(context.HttpContext) == "en";
                var query = context.HttpContext.Request.Query;
                var count = int.TryParse(query["photos"], out var requestedCount) ? Math.Clamp(requestedCount, 0, 30) : 9;
                var caption = english ? "House and grounds preview" : "Преглед на къщата и двора";
                if (query.ContainsKey("long")) caption = string.Join(" ", Enumerable.Repeat(caption, 60));
                var galleryData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<PropertyGalleryViewModel>(
                    new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), context.ModelState)
                {
                    Model = new PropertyGalleryViewModel
                    {
                        Images = Enumerable.Range(1, count).Select(index => new PropertyGalleryImageViewModel
                        {
                            FileName = $"fixture-{index}.svg", Caption = $"{caption} — {index}"
                        }).ToList()
                    }
                };
                context.HttpContext.Response.Headers.CacheControl = "no-store";
                context.Result = new ViewResult { ViewName = "/Views/Gallery/Index.cshtml", ViewData = galleryData };
                return Task.CompletedTask;
            }
            if (context.RouteData.Values["controller"]?.ToString() == "Rooms"
                && context.RouteData.Values["action"]?.ToString() == "Details"
                && !context.RouteData.Values.ContainsKey("area") && HttpMethods.IsGet(context.HttpContext.Request.Method))
            {
                var english = SiteText.Lang(context.HttpContext) == "en";
                var query = context.HttpContext.Request.Query;
                var count = int.TryParse(query["photos"], out var requestedCount) ? Math.Clamp(requestedCount, 0, 30) : 9;
                var name = english ? "Preview room" : "Примерна стая";
                var description = english ? "Sample room description for layout review.\nA second paragraph with room information."
                    : "Примерно описание на стая за преглед на оформлението.\nВтори абзац с информация за стаята.";
                if (query.ContainsKey("long"))
                {
                    name = string.Join(" ", Enumerable.Repeat(name, 6));
                    description = string.Join("\n\n", Enumerable.Repeat(description, 12));
                }
                var roomData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<RoomDetailsViewModel>(
                    new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), context.ModelState)
                {
                    Model = new RoomDetailsViewModel
                    {
                        Id = 0, Slug = "preview-room", Name = name, Description = description, Capacity = 4, PricePerNight = 60,
                        Images = Enumerable.Range(1, count).Select(index => new RoomImageViewModel
                        {
                            FileName = query.ContainsKey("missing") ? "missing-preview-photo.jpg" : $"fixture-{index}.svg",
                            Caption = $"{name} — {index}", IsCover = index == 3
                        }).ToList()
                    }
                };
                context.HttpContext.Response.Headers.CacheControl = "no-store";
                context.Result = new ViewResult { ViewName = "/Views/Rooms/Details.cshtml", ViewData = roomData };
                return Task.CompletedTask;
            }
            // Synthetic homepage data; never instantiate a controller or query storage.
            if (context.RouteData.Values["controller"]?.ToString() == "Home"
                && context.RouteData.Values["action"]?.ToString() == "Index"
                && !context.RouteData.Values.ContainsKey("area") && HttpMethods.IsGet(context.HttpContext.Request.Method))
            {
                var english = SiteText.Lang(context.HttpContext) == "en";
                var rooms = Enumerable.Range(1, 4).Select(id => new RoomCardViewModel
                {
                    Id = id, Slug = $"preview-room-{id}", Capacity = id + 1, PricePerNight = 50 + id * 10,
                    Name = english ? $"Preview room {id} with a long descriptive name" : $"Примерна стая {id} с дълго описателно име",
                    Description = id == 1 ? "" : string.Join(" ", Enumerable.Repeat(english
                        ? "Sample content for checking natural wrapping and the details action."
                        : "Примерно съдържание за проверка на пренасянето и връзката към детайлите.", id * 3)),
                    CoverFileName = id == 2 ? "missing-preview-photo.jpg" : null
                }).ToList();
                var homeViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<HomeIndexViewModel>(
                    new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), context.ModelState)
                {
                    Model = new HomeIndexViewModel
                    {
                        FeaturedRooms = context.HttpContext.Request.Query.ContainsKey("empty") ? new() : rooms,
                        TotalRooms = context.HttpContext.Request.Query.ContainsKey("empty") ? 0 : 4,
                        MaxRoomCapacity = 5, StartingPricePerNight = 60
                    }
                };
                context.HttpContext.Response.Headers.CacheControl = "no-store";
                context.Result = new ViewResult { ViewName = "/Views/Home/Index.cshtml", ViewData = homeViewData };
                return Task.CompletedTask;
            }
            // Only fixture pages are previewed; all other actions are blocked.
            if (context.RouteData.Values["controller"]?.ToString() != "Availability"
                || context.RouteData.Values.ContainsKey("area") || !HttpMethods.IsGet(context.HttpContext.Request.Method))
            {
                context.Result = new StatusCodeResult(404);
                return Task.CompletedTask;
            }
            var today = AvailabilityCalendar.Today();
            var first = new DateOnly(today.Year, today.Month, 1);
            var month = first;
            var requested = context.HttpContext.Request.Query["month"].ToString();
            if (requested.Length > 0 && (!DateOnly.TryParseExact(requested + "-01", "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out month) || month < first || month > first.AddMonths(11)))
            {
                context.Result = new BadRequestResult();
                return Task.CompletedTask;
            }
            var blocks = new[]
            {
                new BlockedNights(today.AddDays(2), today.AddDays(5), AvailabilityBlockKind.Reservation),
                new BlockedNights(today.AddDays(8), today.AddDays(10), AvailabilityBlockKind.Maintenance)
            };
            var months = new List<CalendarMonth> { AvailabilityCalendar.BuildMonth(month, today, blocks) };
            if (month < first.AddMonths(11)) months.Add(AvailabilityCalendar.BuildMonth(month.AddMonths(1), today, blocks));
            var viewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<AvailabilityViewModel>(
                    new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), context.ModelState)
                {
                    Model = new AvailabilityViewModel
                    {
                        Today = today, Month = month, FirstMonth = first, LastMonth = first.AddMonths(11), Months = months
                    }
                };
            context.HttpContext.Response.Headers.CacheControl = "no-store";
            context.Result = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                ? new PartialViewResult
                {
                    ViewName = "/Views/Availability/_Calendar.cshtml", ViewData = viewData
                }
                : new ViewResult
                {
                    ViewName = "/Views/Availability/Index.cshtml", ViewData = viewData
                };
            return Task.CompletedTask;
        }
    }
}
