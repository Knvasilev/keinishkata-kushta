using System.ComponentModel.DataAnnotations;

namespace KeinishkataKushta.Web.Areas.Admin.Models;

public class AdminLoginViewModel
{
    [Required(ErrorMessage = "Въведете потребителско име.")]
    [Display(Name = "Потребител")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Въведете парола.")]
    [DataType(DataType.Password)]
    [Display(Name = "Парола")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
