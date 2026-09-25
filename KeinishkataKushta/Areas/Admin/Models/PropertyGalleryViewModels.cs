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
        public IFormFile File { get; set; } = default!;

        [StringLength(500)]
        public string? Caption { get; set; }
    }
}
