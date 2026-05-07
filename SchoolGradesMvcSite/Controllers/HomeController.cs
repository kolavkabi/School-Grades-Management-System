using Microsoft.AspNetCore.Mvc;
using SchoolGradesMvcSite.Data;
using Microsoft.EntityFrameworkCore;

namespace SchoolGradesMvcSite.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.StudentsCount = await _context.Students.CountAsync();
        ViewBag.TeachersCount = await _context.Teachers.CountAsync();
        ViewBag.SubjectsCount = await _context.Subjects.CountAsync();
        ViewBag.GradesCount = await _context.Grades.CountAsync();
        return View();
    }

    public IActionResult About() => View();

    public IActionResult AccessDenied() => View();

    public IActionResult Error() => View();
}
