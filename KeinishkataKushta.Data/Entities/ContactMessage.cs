namespace KeinishkataKushta.Data.Entities;

public class ContactMessage
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public string Message { get; set; } = default!;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public bool IsProcessed { get; set; }
}
