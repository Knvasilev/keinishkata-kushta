using System.Collections.Generic;

namespace KeinishkataKushta.Web.Areas.Admin.Models
{
    public class GalleryIndexViewModel
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;

        public GalleryUploadViewModel Upload { get; set; } = new();

        public List<GalleryImageListItemViewModel> Images { get; set; } = new();
    }

    public class GalleryImageListItemViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public bool IsCover { get; set; }
    }

}
