namespace KeinishkataKushta.Models
{
    public class RoomDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public List<RoomImageViewModel> Images { get; set; } = new();
    }

    public class RoomImageViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public bool IsCover { get; set; }
    }
}
