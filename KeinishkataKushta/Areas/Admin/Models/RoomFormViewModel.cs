using System.ComponentModel.DataAnnotations;

namespace KeinishkataKushta.Web.Areas.Admin.Models
{
    public class RoomFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Name (Bulgarian)")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Name (English)")]
        public string? NameEn { get; set; }

        [StringLength(200)]
        public string? Slug { get; set; }

        [StringLength(4000)]
        [Display(Name = "Description (Bulgarian)")]
        public string? Description { get; set; }

        [StringLength(4000)]
        [Display(Name = "Description (English)")]
        public string? DescriptionEn { get; set; }

        [Range(1, 100)]
        public int Capacity { get; set; }

        [Range(typeof(decimal), "0.01", "999999")]
        [Display(Name = "Price per night (EUR)")]
        public decimal PricePerNight { get; set; }

        public bool IsPublished { get; set; }
    }
}
