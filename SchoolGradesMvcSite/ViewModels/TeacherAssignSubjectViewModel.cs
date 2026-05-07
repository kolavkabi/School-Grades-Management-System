using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolGradesMvcSite.ViewModels;

public class TeacherAssignSubjectViewModel
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;

    [Display(Name = "Предмет")]
    public int SubjectId { get; set; }

    public List<SelectListItem> Subjects { get; set; } = new();
}
