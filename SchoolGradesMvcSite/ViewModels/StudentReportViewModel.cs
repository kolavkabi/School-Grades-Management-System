using SchoolGradesMvcSite.Models;

namespace SchoolGradesMvcSite.ViewModels;

public class StudentReportViewModel
{
    public Student? Student { get; set; }
    public List<Grade> Grades { get; set; } = new();
    public decimal Average { get; set; }
}
