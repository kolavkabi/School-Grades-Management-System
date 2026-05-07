using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolGradesMvcSite.Data;
using SchoolGradesMvcSite.Infrastructure;
using SchoolGradesMvcSite.Models;
using SchoolGradesMvcSite.ViewModels;

namespace SchoolGradesMvcSite.Controllers;

[Authorize(Roles = AppRoles.Staff)]
public class GradesController : Controller
{
    private readonly ApplicationDbContext _context;

    public GradesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? studentId, int? subjectId, DateTime? dateFrom, DateTime? dateTo)
    {
        var query = _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .AsQueryable();

        if (studentId.HasValue) query = query.Where(g => g.StudentId == studentId.Value);
        if (subjectId.HasValue) query = query.Where(g => g.SubjectId == subjectId.Value);
        if (dateFrom.HasValue) query = query.Where(g => g.DateAssigned >= dateFrom.Value.Date);
        if (dateTo.HasValue) query = query.Where(g => g.DateAssigned <= dateTo.Value.Date);

        ViewBag.Students = new SelectList(await _context.Students.OrderBy(s => s.LastName).ToListAsync(), "Id", "FullName", studentId);
        ViewBag.Subjects = new SelectList(await _context.Subjects.OrderBy(s => s.Name).ToListAsync(), "Id", "Name", subjectId);
        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

        return View(await query.OrderByDescending(g => g.DateAssigned).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var grade = await _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .FirstOrDefaultAsync(g => g.Id == id);
        return grade is null ? NotFound() : View(grade);
    }

    public async Task<IActionResult> Create()
    {
        await FillSelectLists();
        return View(new Grade { DateAssigned = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Grade grade)
    {
        if (!ModelState.IsValid)
        {
            await FillSelectLists();
            return View(grade);
        }
        _context.Add(grade);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Оцінку додано.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var grade = await _context.Grades.FindAsync(id);
        if (grade is null) return NotFound();
        await FillSelectLists();
        return View(grade);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Grade grade)
    {
        if (id != grade.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            await FillSelectLists();
            return View(grade);
        }
        _context.Update(grade);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Оцінку оновлено.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var grade = await _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .FirstOrDefaultAsync(g => g.Id == id);
        return grade is null ? NotFound() : View(grade);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var grade = await _context.Grades.FindAsync(id);
        if (grade is null) return NotFound();
        _context.Grades.Remove(grade);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Оцінку видалено.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> StudentHistory(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student is null) return NotFound();

        ViewBag.Student = student;
        var grades = await _context.Grades
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .Where(g => g.StudentId == id)
            .OrderByDescending(g => g.DateAssigned)
            .ToListAsync();
        return View(grades);
    }

    public async Task<IActionResult> StudentAverage(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student is null) return NotFound();

        var grades = await _context.Grades.Where(g => g.StudentId == id).ToListAsync();
        ViewBag.Student = student;
        ViewBag.Average = grades.Any() ? grades.Average(g => g.Value).ToString("0.00") : "0.00";
        return View(grades);
    }

    public async Task<IActionResult> EditComment(int id)
    {
        var grade = await _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (grade is null) return NotFound();

        return View(new GradeCommentViewModel
        {
            GradeId = grade.Id,
            StudentId = grade.StudentId,
            StudentName = grade.Student?.FullName ?? string.Empty,
            SubjectName = grade.Subject?.Name ?? string.Empty,
            Comment = grade.Comment
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditComment(GradeCommentViewModel model)
    {
        var grade = await _context.Grades.FindAsync(model.GradeId);
        if (grade is null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.StudentName = (await _context.Students.FindAsync(grade.StudentId))?.FullName ?? string.Empty;
            model.SubjectName = (await _context.Subjects.FindAsync(grade.SubjectId))?.Name ?? string.Empty;
            return View(model);
        }

        grade.Comment = model.Comment?.Trim();
        await _context.SaveChangesAsync();
        TempData["Success"] = "Коментар до оцінки оновлено.";
        return RedirectToAction(nameof(Details), new { id = model.GradeId });
    }

    private async Task FillSelectLists()
    {
        ViewBag.Students = new SelectList(await _context.Students.OrderBy(s => s.LastName).ToListAsync(), "Id", "FullName");
        ViewBag.Subjects = new SelectList(await _context.Subjects.OrderBy(s => s.Name).ToListAsync(), "Id", "Name");
        ViewBag.Teachers = new SelectList(await _context.Teachers.OrderBy(t => t.LastName).ToListAsync(), "Id", "FullName");
    }
}
