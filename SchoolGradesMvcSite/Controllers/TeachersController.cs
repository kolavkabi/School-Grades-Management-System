using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolGradesMvcSite.Data;
using SchoolGradesMvcSite.Infrastructure;
using SchoolGradesMvcSite.Models;
using SchoolGradesMvcSite.ViewModels;

namespace SchoolGradesMvcSite.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public class TeachersController : Controller
{
    private readonly ApplicationDbContext _context;

    public TeachersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var teachers = await _context.Teachers
            .Include(t => t.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
            .OrderBy(t => t.LastName)
            .ToListAsync();
        return View(teachers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var teacher = await _context.Teachers
            .Include(t => t.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
            .Include(t => t.Grades)
                .ThenInclude(g => g.Subject)
            .FirstOrDefaultAsync(t => t.Id == id);
        return teacher is null ? NotFound() : View(teacher);
    }

    public IActionResult Create() => View(new Teacher { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Teacher teacher)
    {
        if (!ModelState.IsValid) return View(teacher);
        _context.Add(teacher);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Вчителя створено.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        return teacher is null ? NotFound() : View(teacher);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Teacher teacher)
    {
        if (id != teacher.Id) return NotFound();
        if (!ModelState.IsValid) return View(teacher);
        _context.Update(teacher);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Дані вчителя оновлено.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        return teacher is null ? NotFound() : View(teacher);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var teacher = await _context.Teachers
            .Include(t => t.TeacherSubjects)
            .Include(t => t.Grades)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (teacher is null) return NotFound();
        _context.TeacherSubjects.RemoveRange(teacher.TeacherSubjects);
        _context.Grades.RemoveRange(teacher.Grades);
        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Вчителя видалено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> AssignSubject(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher is null) return NotFound();

        var assignedIds = await _context.TeacherSubjects.Where(ts => ts.TeacherId == id).Select(ts => ts.SubjectId).ToListAsync();
        var subjects = await _context.Subjects
            .Where(s => !assignedIds.Contains(s.Id))
            .OrderBy(s => s.Name)
            .ToListAsync();

        var vm = new TeacherAssignSubjectViewModel
        {
            TeacherId = teacher.Id,
            TeacherName = teacher.FullName,
            Subjects = subjects.Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignSubject(TeacherAssignSubjectViewModel model)
    {
        if (!_context.TeacherSubjects.Any(ts => ts.TeacherId == model.TeacherId && ts.SubjectId == model.SubjectId))
        {
            _context.TeacherSubjects.Add(new TeacherSubject { TeacherId = model.TeacherId, SubjectId = model.SubjectId });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Предмет призначено вчителю.";
        }
        return RedirectToAction(nameof(Details), new { id = model.TeacherId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveSubject(int teacherId, int subjectId)
    {
        var link = await _context.TeacherSubjects.FindAsync(teacherId, subjectId);
        if (link is not null)
        {
            _context.TeacherSubjects.Remove(link);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Предмет відкріплено від вчителя.";
        }
        return RedirectToAction(nameof(Details), new { id = teacherId });
    }
}
