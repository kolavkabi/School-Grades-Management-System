using System.ComponentModel.DataAnnotations;

namespace SchoolGradesMvcSite.Models;

public class Subject
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Назва")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    [Display(Name = "Опис")]
    public string? Description { get; set; }

    [Range(1, 40)]
    [Display(Name = "Годин на тиждень")]
    public int WeeklyHours { get; set; }

    [Display(Name = "Активний")]
    public bool IsActive { get; set; } = true;

    public ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
