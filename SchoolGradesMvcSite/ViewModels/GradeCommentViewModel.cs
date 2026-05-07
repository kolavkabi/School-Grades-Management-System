using System.ComponentModel.DataAnnotations;

namespace SchoolGradesMvcSite.ViewModels;

public class GradeCommentViewModel
{
    public int GradeId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;

    [StringLength(300)]
    [Display(Name = "Коментар вчителя")]
    public string? Comment { get; set; }
}
