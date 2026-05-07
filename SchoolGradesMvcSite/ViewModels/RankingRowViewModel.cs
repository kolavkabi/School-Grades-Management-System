namespace SchoolGradesMvcSite.ViewModels;

public class RankingRowViewModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public decimal Average { get; set; }
    public int GradesCount { get; set; }
}
