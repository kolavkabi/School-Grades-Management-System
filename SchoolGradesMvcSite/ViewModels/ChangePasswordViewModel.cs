using System.ComponentModel.DataAnnotations;

namespace SchoolGradesMvcSite.ViewModels;

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Поточний пароль")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Новий пароль")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword))]
    [Display(Name = "Підтвердити пароль")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
