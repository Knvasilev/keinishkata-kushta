namespace KeinishkataKushta.Data.Entities;

public enum AvailabilityBlockKind
{
    Reservation = 0,
    Maintenance = 1
}

public class AvailabilityBlock
{
    public int Id { get; set; }
    // Informational only: every entry blocks the entire property.
    public string? RoomsUsed { get; set; }
    public DateOnly Arrival { get; set; }
    public DateOnly Departure { get; set; }
    public AvailabilityBlockKind Kind { get; set; }
    public string? Notes { get; set; }
    public byte[] Version { get; set; } = [];
}
