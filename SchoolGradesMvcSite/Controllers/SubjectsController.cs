using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolGradesMvcSite.Data;
using SchoolGradesMvcSite.Infrastructure;
using SchoolGradesMvcSite.Models;

namespace SchoolGradesMvcSite.Controllers;

public class SubjectsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SubjectsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        return View(await _context.Subjects.OrderBy(s => s.Name).ToListAsync());
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var subject = await _context.Subjects
            .Include(s => s.TeacherSubjects)
                .ThenInclude(ts => ts.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id);
        return subject is null ? NotFound() : View(subject);
    }

    [Authorize(Roles = AppRoles.Staff)]
    public IActionResult Create() => View(new Subject { IsActive = true, WeeklyHours = 1 });

    [HttpPost]
    [Authorize(Roles = AppRoles.Staff)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Subject subject)
    {
        if (!ModelState.IsValid) return View(subject);
        _context.Add(subject);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Предмет створено.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Staff)]
    public async Task<IActionResult> Edit(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        return subject is null ? NotFound() : View(subject);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Staff)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Subject subject)
    {
        if (id != subject.Id) return NotFound();
        if (!ModelState.IsValid) return View(subject);
        _context.Update(subject);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Предмет оновлено.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Staff)]
    public async Task<IActionResult> Delete(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        return subject is null ? NotFound() : View(subject);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = AppRoles.Staff)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var subject = await _context.Subjects
            .Include(s => s.TeacherSubjects)
            .Include(s => s.Grades)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject is null) return NotFound();
        _context.TeacherSubjects.RemoveRange(subject.TeacherSubjects);
        _context.Grades.RemoveRange(subject.Grades);
        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Предмет видалено.";
        return RedirectToAction(nameof(Index));
    }
}
