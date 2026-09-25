using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KeinishkataKushta.Web.Areas.Admin.Models
{
    public class GalleryUploadViewModel
    {
        public int RoomId { get; set; }

        [Required]
        public IFormFile File { get; set; } = default!;

        [StringLength(500)]
        public string? Caption { get; set; }

        public bool IsCover { get; set; }
    }
}
