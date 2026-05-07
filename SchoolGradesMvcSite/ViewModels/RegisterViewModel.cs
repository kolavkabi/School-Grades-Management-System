using System.ComponentModel.DataAnnotations;

namespace SchoolGradesMvcSite.ViewModels;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "ПІБ")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Логін")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має містити щонайменше 6 символів.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Підтвердження пароля")]
    [Compare(nameof(Password), ErrorMessage = "Паролі не збігаються.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
