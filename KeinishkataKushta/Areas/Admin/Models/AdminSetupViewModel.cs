using System.ComponentModel.DataAnnotations;

namespace KeinishkataKushta.Web.Areas.Admin.Models;

public class AdminSetupViewModel
{
    [Required(ErrorMessage = "Въведете потребителско име.")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Потребителското име трябва да е между 4 и 100 символа.")]
    [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Използвайте само латински букви, цифри, точка, тире или долна черта.")]
    [Display(Name = "Потребител")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Въведете парола.")]
    [MinLength(12, ErrorMessage = "Паролата трябва да е поне 12 символа.")]
    [DataType(DataType.Password)]
    [Display(Name = "Парола")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Повторете паролата.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Паролите не съвпадат.")]
    [Display(Name = "Повторете паролата")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
