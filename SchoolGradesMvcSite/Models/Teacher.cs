using System.ComponentModel.DataAnnotations;

namespace SchoolGradesMvcSite.Models;

public class Teacher
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
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Активний")]
    public bool IsActive { get; set; } = true;

    public ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public string FullName => $"{LastName} {FirstName}";
}
