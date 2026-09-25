namespace KeinishkataKushta.Web.Areas.Admin.Models
{
    public class RoomListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsPublished { get; set; }
        public string? CoverFileName { get; set; }
    }
}
