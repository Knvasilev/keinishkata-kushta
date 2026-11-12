namespace KeinishkataKushta.Data.Entities;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public string Slug { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? DescriptionEn { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsPublished { get; set; }

    public ICollection<GalleryImage> GalleryImages { get; set; } = new List<GalleryImage>();
}
