using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolGradesMvcSite.Data;
using SchoolGradesMvcSite.Infrastructure;
using SchoolGradesMvcSite.Models;
using System.Text;

namespace SchoolGradesMvcSite.Controllers;

[Authorize(Roles = AppRoles.Staff)]
public class StudentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public StudentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? className)
    {
        var query = _context.Students.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.FirstName.Contains(search) || s.LastName.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(className))
        {
            query = query.Where(s => s.ClassName == className);
        }

        ViewBag.Classes = await _context.Students.Select(s => s.ClassName).Distinct().OrderBy(c => c).ToListAsync();
        ViewBag.Search = search;
        ViewBag.ClassName = className;
        return View(await query.OrderBy(s => s.ClassName).ThenBy(s => s.LastName).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var student = await _context.Students
            .Include(s => s.Grades)
                .ThenInclude(g => g.Subject)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student is null) return NotFound();
        return View(student);
    }

    public IActionResult Create() => View(new Student { DateOfBirth = DateTime.Today, IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Student student)
    {
        if (!ModelState.IsValid) return View(student);
        _context.Add(student);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Учня створено.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var student = await _context.Students.FindAsync(id);
        return student is null ? NotFound() : View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Student student)
    {
        if (id != student.Id) return NotFound();
        if (!ModelState.IsValid) return View(student);
        _context.Update(student);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Дані учня оновлено.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var student = await _context.Students.FindAsync(id);
        return student is null ? NotFound() : View(student);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await _context.Students.Include(s => s.Grades).FirstOrDefaultAsync(s => s.Id == id);
        if (student is null) return NotFound();
        _context.Grades.RemoveRange(student.Grades);
        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Учня видалено.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Administrator)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student is null) return NotFound();
        student.IsActive = !student.IsActive;
        await _context.SaveChangesAsync();
        TempData["Success"] = student.IsActive ? "Учня активовано." : "Учня деактивовано.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Import() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile? csvFile)
    {
        if (csvFile is null || csvFile.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Оберіть CSV файл.");
            return View();
        }

        using var reader = new StreamReader(csvFile.OpenReadStream());
        string? line;
        var imported = 0;
        var first = true;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            if (first)
            {
                first = false;
                continue;
            }

            var parts = line.Split(',');
            if (parts.Length < 4) continue;
            if (!DateTime.TryParse(parts[3], out var date)) continue;

            _context.Students.Add(new Student
            {
                FirstName = parts[0].Trim(),
                LastName = parts[1].Trim(),
                ClassName = parts[2].Trim(),
                DateOfBirth = date,
                IsActive = true
            });
            imported++;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Імпорт завершено. Додано записів: {imported}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<FileResult> ExportCsv()
    {
        var students = await _context.Students.OrderBy(s => s.ClassName).ThenBy(s => s.LastName).ToListAsync();
        var sb = new StringBuilder();
        sb.AppendLine("FirstName,LastName,ClassName,DateOfBirth,IsActive");
        foreach (var s in students)
        {
            sb.AppendLine($"{Escape(s.FirstName)},{Escape(s.LastName)},{Escape(s.ClassName)},{s.DateOfBirth:yyyy-MM-dd},{s.IsActive}");
        }
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "students.csv");
    }

    private static string Escape(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
