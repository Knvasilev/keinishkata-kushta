using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KeinishkataKushta.Web.Areas.Admin.Models
{
    public class GalleryUploadViewModel
    {
        public int RoomId { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(20)]
        public List<IFormFile> Files { get; set; } = new();

        [StringLength(500)]
        public string? Caption { get; set; }

        [StringLength(500)]
        public string? CaptionEn { get; set; }

        public bool IsCover { get; set; }
    }
}
