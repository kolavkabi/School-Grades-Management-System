using System.ComponentModel.DataAnnotations;

namespace SchoolGradesMvcSite.ViewModels;

public class AdminUserViewModel
{
    public string? Id { get; set; }

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

    [Display(Name = "Роль")]
    public string Role { get; set; } = "User";

    [Display(Name = "Активний")]
    public bool IsActive { get; set; } = true;

    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }
}
