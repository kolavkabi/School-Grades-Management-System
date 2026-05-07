using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolGradesMvcSite.Data;
using SchoolGradesMvcSite.Infrastructure;
using SchoolGradesMvcSite.ViewModels;
using System.Text;

namespace SchoolGradesMvcSite.Controllers;

[Authorize(Roles = AppRoles.Staff)]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Students = new SelectList(await _context.Students.OrderBy(s => s.LastName).ToListAsync(), "Id", "FullName");
        ViewBag.Classes = await _context.Students.Select(s => s.ClassName).Distinct().OrderBy(c => c).ToListAsync();
        return View();
    }

    public async Task<IActionResult> StudentReport(int studentId)
    {
        var student = await _context.Students.FindAsync(studentId);
        if (student is null) return NotFound();

        var grades = await _context.Grades
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .Where(g => g.StudentId == studentId)
            .OrderByDescending(g => g.DateAssigned)
            .ToListAsync();

        var vm = new StudentReportViewModel
        {
            Student = student,
            Grades = grades,
            Average = grades.Any() ? Convert.ToDecimal(grades.Average(g => g.Value)) : 0
        };
        return View(vm);
    }


    public async Task<IActionResult> ExportStudentReportCsv(int studentId)
    {
        var student = await _context.Students.FindAsync(studentId);
        if (student is null)
        {
            return NotFound();
        }

        var grades = await _context.Grades
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .Where(g => g.StudentId == studentId)
            .OrderByDescending(g => g.DateAssigned)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Student,Class,Subject,Teacher,Value,Date,Comment");
        foreach (var g in grades)
        {
            sb.AppendLine($"{Esc(student.FullName)},{Esc(student.ClassName)},{Esc(g.Subject?.Name ?? string.Empty)},{Esc(g.Teacher?.FullName ?? string.Empty)},{g.Value},{g.DateAssigned:yyyy-MM-dd},{Esc(g.Comment ?? string.Empty)}");
        }

        var safeLastName = student.LastName.Replace(' ', '_');
        var fileName = $"student-report-{safeLastName}-{student.Id}.csv";
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
    }

    public async Task<IActionResult> TopStudentInClass(string className)
    {
        ViewBag.Classes = await _context.Students.Select(s => s.ClassName).Distinct().OrderBy(c => c).ToListAsync();
        ViewBag.ClassName = className;

        if (string.IsNullOrWhiteSpace(className))
        {
            return View(new TopStudentInClassViewModel());
        }

        var gradeRows = await _context.Grades
            .Include(g => g.Student)
            .Where(g => g.Student != null && g.Student.ClassName == className)
            .Select(g => new
            {
                g.StudentId,
                FirstName = g.Student!.FirstName,
                LastName = g.Student.LastName,
                g.Value
            })
            .ToListAsync();

        var top = gradeRows
            .GroupBy(g => new { g.StudentId, g.FirstName, g.LastName })
            .Select(g => new TopStudentInClassViewModel
            {
                ClassName = className,
                StudentId = g.Key.StudentId,
                StudentName = $"{g.Key.LastName} {g.Key.FirstName}",
                Average = Math.Round((decimal)g.Average(x => x.Value), 2),
                GradesCount = g.Count()
            })
            .OrderByDescending(x => x.Average)
            .ThenBy(x => x.StudentName)
            .FirstOrDefault();

        return View(top ?? new TopStudentInClassViewModel { ClassName = className });
    }

    public async Task<IActionResult> Ranking(string? className)
    {
        var gradeRows = await _context.Grades
            .Include(g => g.Student)
            .Where(g => g.Student != null && (string.IsNullOrWhiteSpace(className) || g.Student.ClassName == className))
            .Select(g => new
            {
                g.StudentId,
                FirstName = g.Student!.FirstName,
                LastName = g.Student.LastName,
                ClassName = g.Student.ClassName,
                g.Value
            })
            .ToListAsync();

        var ranking = gradeRows
            .GroupBy(g => new { g.StudentId, g.FirstName, g.LastName, g.ClassName })
            .Select(g => new RankingRowViewModel
            {
                StudentId = g.Key.StudentId,
                StudentName = g.Key.LastName + " " + g.Key.FirstName,
                ClassName = g.Key.ClassName,
                Average = Math.Round((decimal)g.Average(x => x.Value), 2),
                GradesCount = g.Count()
            })
            .OrderByDescending(x => x.Average)
            .ThenBy(x => x.StudentName)
            .ToList();

        ViewBag.Classes = await _context.Students.Select(s => s.ClassName).Distinct().OrderBy(c => c).ToListAsync();
        ViewBag.ClassName = className;
        return View(ranking);
    }

    public async Task<IActionResult> ClassStatistics(string? className)
    {
        var classQuery = _context.Students.AsQueryable();
        if (!string.IsNullOrWhiteSpace(className))
            classQuery = classQuery.Where(s => s.ClassName == className);

        var classes = await classQuery
            .Select(s => s.ClassName)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var result = new List<ClassStatisticsViewModel>();
        foreach (var cls in classes)
        {
            var studentIds = await _context.Students.Where(s => s.ClassName == cls).Select(s => s.Id).ToListAsync();
            var grades = await _context.Grades.Where(g => studentIds.Contains(g.StudentId)).ToListAsync();
            result.Add(new ClassStatisticsViewModel
            {
                ClassName = cls,
                StudentsCount = studentIds.Count,
                GradesCount = grades.Count,
                Average = grades.Any() ? Convert.ToDecimal(grades.Average(g => g.Value)) : 0
            });
        }

        ViewBag.Classes = await _context.Students.Select(s => s.ClassName).Distinct().OrderBy(c => c).ToListAsync();
        ViewBag.ClassName = className;
        return View(result);
    }

    public async Task<FileResult> ExportGradesCsv()
    {
        var grades = await _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .OrderByDescending(g => g.DateAssigned)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Student,Class,Subject,Teacher,Value,Date,Comment");
        foreach (var g in grades)
        {
            sb.AppendLine($"{Esc(g.Student!.FullName)},{Esc(g.Student.ClassName)},{Esc(g.Subject!.Name)},{Esc(g.Teacher!.FullName)},{g.Value},{g.DateAssigned:yyyy-MM-dd},{Esc(g.Comment ?? string.Empty)}");
        }
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "grades-report.csv");
    }

    private static string Esc(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
