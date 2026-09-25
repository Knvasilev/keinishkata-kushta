namespace KeinishkataKushta.Models
{
    public class HomeIndexViewModel
    {
        public List<RoomCardViewModel> FeaturedRooms { get; set; } = new();
        public int TotalRooms { get; set; }
        public int MaxRoomCapacity { get; set; }
        public decimal StartingPricePerNight { get; set; }
    }
}
