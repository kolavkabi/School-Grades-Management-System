using System.ComponentModel.DataAnnotations;

namespace SchoolGradesMvcSite.ViewModels;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Логін")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запам’ятати мене")]
    public bool RememberMe { get; set; }
}
