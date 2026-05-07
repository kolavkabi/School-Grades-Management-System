using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolGradesMvcSite.Data;
using SchoolGradesMvcSite.Infrastructure;
using SchoolGradesMvcSite.Models;
using SchoolGradesMvcSite.ViewModels;

namespace SchoolGradesMvcSite.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public class AdminController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminController(UserManager<AppUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewBag.UsersCount = await _userManager.Users.CountAsync();
        ViewBag.StudentsCount = await _context.Students.CountAsync();
        ViewBag.TeachersCount = await _context.Teachers.CountAsync();
        ViewBag.SubjectsCount = await _context.Subjects.CountAsync();
        ViewBag.GradesCount = await _context.Grades.CountAsync();
        return View();
    }

    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
        var items = new List<UserListItemViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new UserListItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "-",
                IsActive = user.IsActive
            });
        }

        return View(items);
    }

    [HttpGet]
    public IActionResult CreateUser() => View(new AdminUserViewModel { IsActive = true, Role = AppRoles.User });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(AdminUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new AppUser { EmailConfirmed = true };
        MapUserFields(model, user);

        var result = await _userManager.CreateAsync(user, model.Password ?? "password123");
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            AddIdentityErrors(roleResult);
            await _userManager.DeleteAsync(user);
            return View(model);
        }

        TempData["Success"] = "Користувача створено.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? AppRoles.User;
        return View(new AdminUserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            Role = role
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(AdminUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByIdAsync(model.Id!);
        if (user is null) return NotFound();

        MapUserFields(model, user);

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            AddIdentityErrors(updateResult);
            return View(model);
        }

        var rolesResult = await ReplaceUserRoleAsync(user, model.Role);
        if (!rolesResult.Succeeded)
        {
            AddIdentityErrors(rolesResult);
            return View(model);
        }

        TempData["Success"] = "Користувача оновлено.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        TempData["Success"] = user.IsActive ? "Користувача активовано." : "Користувача деактивовано.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if ((user.UserName ?? string.Empty).Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Головного адміністратора не можна видалити.";
            return RedirectToAction(nameof(Users));
        }

        await _userManager.DeleteAsync(user);
        TempData["Success"] = "Користувача видалено.";
        return RedirectToAction(nameof(Users));
    }

    private static void MapUserFields(AdminUserViewModel model, AppUser user)
    {
        user.FullName = model.FullName;
        user.UserName = model.UserName;
        user.Email = model.Email;
        user.IsActive = model.IsActive;
    }

    private async Task<IdentityResult> ReplaceUserRoleAsync(AppUser user, string role)
    {
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return removeResult;
        }

        return await _userManager.AddToRoleAsync(user, role);
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }
}
