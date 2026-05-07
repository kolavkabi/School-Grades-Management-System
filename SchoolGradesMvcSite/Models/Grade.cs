using System.ComponentModel.DataAnnotations;

namespace SchoolGradesMvcSite.Models;

public class Grade
{
    public int Id { get; set; }

    [Range(1, 12)]
    [Display(Name = "Оцінка")]
    public int Value { get; set; }

    [StringLength(300)]
    [Display(Name = "Коментар")]
    public string? Comment { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Дата")]
    public DateTime DateAssigned { get; set; } = DateTime.Now.Date;

    [Display(Name = "Учень")]
    public int StudentId { get; set; }
    public Student? Student { get; set; }

    [Display(Name = "Предмет")]
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    [Display(Name = "Вчитель")]
    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}
