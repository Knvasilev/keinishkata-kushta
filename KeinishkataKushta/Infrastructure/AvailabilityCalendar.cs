using KeinishkataKushta.Data.Entities;
using KeinishkataKushta.Models;

namespace KeinishkataKushta.Infrastructure;

public static class AvailabilityCalendar
{
    public static DateOnly Today() => DateOnly.FromDateTime(
        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Europe/Sofia"));

    public static CalendarMonth BuildMonth(DateOnly month, DateOnly today, IReadOnlyList<BlockedNights> blocks)
    {
        var days = new List<CalendarDay>();
        for (var day = month; day < month.AddMonths(1); day = day.AddDays(1))
        {
            var relevant = blocks.Where(block => block.Arrival <= day && block.Departure > day).ToList();
            var status = day < today ? "Past" : relevant.Any(block => block.Kind == AvailabilityBlockKind.Reservation)
                ? "Occupied" : relevant.Count > 0 ? "Unavailable" : "Available";
            days.Add(new CalendarDay(day, status));
        }

        return new CalendarMonth(month, ((int)month.DayOfWeek + 6) % 7, days);
    }
}
