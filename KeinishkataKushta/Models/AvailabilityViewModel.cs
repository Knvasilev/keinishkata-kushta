using KeinishkataKushta.Data.Entities;

namespace KeinishkataKushta.Models;

public record BlockedNights(DateOnly Arrival, DateOnly Departure, AvailabilityBlockKind Kind);
public record CalendarDay(DateOnly Date, string Status);
public record CalendarMonth(DateOnly Month, int LeadingDays, IReadOnlyList<CalendarDay> Days);

public class AvailabilityViewModel
{
    public DateOnly Today { get; init; }
    public DateOnly Month { get; init; }
    public DateOnly FirstMonth { get; init; }
    public DateOnly LastMonth { get; init; }
    public List<CalendarMonth> Months { get; init; } = [];
}
