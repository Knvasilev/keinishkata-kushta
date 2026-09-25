using System.ComponentModel.DataAnnotations;

namespace KeinishkataKushta.Models
{
    public class ContactFormViewModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Phone { get; set; }

        [Required]
        [StringLength(4000)]
        public string Message { get; set; } = string.Empty;
    }
}
