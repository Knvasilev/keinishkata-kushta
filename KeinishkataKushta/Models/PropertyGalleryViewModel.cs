namespace KeinishkataKushta.Models
{
    public class PropertyGalleryViewModel
    {
        public List<PropertyGalleryImageViewModel> Images { get; set; } = new();
    }

    public class PropertyGalleryImageViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public string? Caption { get; set; }
    }
}
