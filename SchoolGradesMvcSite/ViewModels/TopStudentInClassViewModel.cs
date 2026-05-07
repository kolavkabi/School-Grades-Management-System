namespace SchoolGradesMvcSite.ViewModels;

public class TopStudentInClassViewModel
{
    public string ClassName { get; set; } = string.Empty;
    public int? StudentId { get; set; }
    public string? StudentName { get; set; }
    public decimal Average { get; set; }
    public int GradesCount { get; set; }
    public bool HasData => StudentId.HasValue;
}
