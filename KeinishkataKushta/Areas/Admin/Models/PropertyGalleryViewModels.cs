using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KeinishkataKushta.Web.Areas.Admin.Models
{
    public class PropertyGalleryIndexViewModel
    {
        public PropertyGalleryUploadViewModel Upload { get; set; } = new();
        public List<GalleryImageListItemViewModel> Images { get; set; } = new();
    }

    public class PropertyGalleryUploadViewModel
    {
        [Required]
        [MinLength(1)]
        [MaxLength(20)]
        public List<IFormFile> Files { get; set; } = new();

        [StringLength(500)]
        public string? Caption { get; set; }

        [StringLength(500)]
        public string? CaptionEn { get; set; }
    }
}
