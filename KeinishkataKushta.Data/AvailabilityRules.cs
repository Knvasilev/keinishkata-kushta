using System.Linq.Expressions;
using KeinishkataKushta.Data.Entities;

namespace KeinishkataKushta.Data;

public static class AvailabilityRules
{
    // Departure is exclusive: consecutive stays may share a changeover date.
    public static Expression<Func<AvailabilityBlock, bool>> Conflicts(
        DateOnly arrival, DateOnly departure, int excludingId = 0) =>
        block => block.Id != excludingId
            && block.Arrival < departure && block.Departure > arrival;
}
