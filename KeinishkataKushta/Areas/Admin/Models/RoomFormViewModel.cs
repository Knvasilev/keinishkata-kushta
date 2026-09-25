using System.ComponentModel.DataAnnotations;

namespace KeinishkataKushta.Web.Areas.Admin.Models
{
    public class RoomFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Slug { get; set; }

        [StringLength(4000)]
        public string? Description { get; set; }

        [Range(1, 100)]
        public int Capacity { get; set; }

        [Range(typeof(decimal), "0.01", "999999")]
        [Display(Name = "Price per night (EUR)")]
        public decimal PricePerNight { get; set; }

        public bool IsPublished { get; set; }
    }
}
