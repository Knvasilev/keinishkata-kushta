namespace KeinishkataKushta.Data.Entities;

public class GalleryImage
{
    public int Id { get; set; }
    public string FileName { get; set; } = default!;
    public string? Caption { get; set; }
    public bool IsCover { get; set; }

    public int? RoomId { get; set; }
    public Room? Room { get; set; }
}
