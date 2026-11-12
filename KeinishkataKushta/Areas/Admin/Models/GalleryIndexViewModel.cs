using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

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
        public string? CaptionEn { get; set; }
        public bool IsCover { get; set; }
    }

    public class GalleryImagePositionViewModel
    {
        public int Id { get; set; }
        public int? RoomId { get; set; }
        public int Position { get; set; }
        public int Total { get; set; }
    }

    public class GalleryImageCaptionViewModel
    {
        public int Id { get; set; }
        public int? RoomId { get; set; }

        [StringLength(500)]
        public string? Caption { get; set; }

        [StringLength(500)]
        public string? CaptionEn { get; set; }
    }

}
