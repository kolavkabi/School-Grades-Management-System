using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolGradesMvcSite.Infrastructure;
using SchoolGradesMvcSite.Models;

namespace SchoolGradesMvcSite.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        foreach (var role in AppRoles.ManagedRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (await userManager.FindByNameAsync("admin") is null)
        {
            var admin = new AppUser
            {
                UserName = "admin",
                Email = "admin@school.local",
                FullName = "System Administrator",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(admin, "admin123");
            await userManager.AddToRoleAsync(admin, AppRoles.Administrator);
        }

        if (await userManager.FindByNameAsync("teacher") is null)
        {
            var teacherUser = new AppUser
            {
                UserName = "teacher",
                Email = "teacher@school.local",
                FullName = "Default Teacher",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(teacherUser, "teacher123");
            await userManager.AddToRoleAsync(teacherUser, AppRoles.User);
        }

        if (!await db.Subjects.AnyAsync())
        {
            db.Subjects.AddRange(
                new Subject { Name = "Математика", Description = "Алгебра та геометрія", WeeklyHours = 4, IsActive = true },
                new Subject { Name = "Історія", Description = "Історія України та світу", WeeklyHours = 2, IsActive = true },
                new Subject { Name = "Інформатика", Description = "Основи ІТ та програмування", WeeklyHours = 3, IsActive = true }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Teachers.AnyAsync())
        {
            var teacher1 = new Teacher { FirstName = "Олена", LastName = "Іваненко", Email = "olena.ivanenko@school.local", IsActive = true };
            var teacher2 = new Teacher { FirstName = "Петро", LastName = "Сидоренко", Email = "petro.sydorenko@school.local", IsActive = true };
            db.Teachers.AddRange(teacher1, teacher2);
            await db.SaveChangesAsync();

            var math = await db.Subjects.FirstAsync(s => s.Name == "Математика");
            var history = await db.Subjects.FirstAsync(s => s.Name == "Історія");
            db.TeacherSubjects.AddRange(
                new TeacherSubject { TeacherId = teacher1.Id, SubjectId = math.Id },
                new TeacherSubject { TeacherId = teacher2.Id, SubjectId = history.Id }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Students.AnyAsync())
        {
            db.Students.AddRange(
                new Student { FirstName = "Іван", LastName = "Петренко", ClassName = "10-А", DateOfBirth = new DateTime(2008, 5, 14), IsActive = true },
                new Student { FirstName = "Марія", LastName = "Коваль", ClassName = "10-А", DateOfBirth = new DateTime(2008, 8, 1), IsActive = true },
                new Student { FirstName = "Андрій", LastName = "Бойко", ClassName = "9-Б", DateOfBirth = new DateTime(2009, 2, 20), IsActive = true }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Grades.AnyAsync())
        {
            var student = await db.Students.FirstAsync();
            var subject = await db.Subjects.FirstAsync();
            var teacher = await db.Teachers.FirstAsync();
            db.Grades.Add(new Grade
            {
                StudentId = student.Id,
                SubjectId = subject.Id,
                TeacherId = teacher.Id,
                Value = 10,
                Comment = "Стартова оцінка",
                DateAssigned = DateTime.Now.Date,
            });
            await db.SaveChangesAsync();
        }
    }
}
