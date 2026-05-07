using System.ComponentModel.DataAnnotations;

namespace SchoolGradesMvcSite.Models;

public class Student
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Ім’я")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Прізвище")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Display(Name = "Клас")]
    public string ClassName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Дата народження")]
    public DateTime DateOfBirth { get; set; }

    [Display(Name = "Активний")]
    public bool IsActive { get; set; } = true;

    public ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public string FullName => $"{LastName} {FirstName}";
}
