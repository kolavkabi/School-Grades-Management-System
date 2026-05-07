namespace SchoolGradesMvcSite.ViewModels;

public class ClassStatisticsViewModel
{
    public string ClassName { get; set; } = string.Empty;
    public int StudentsCount { get; set; }
    public decimal Average { get; set; }
    public int GradesCount { get; set; }
}
