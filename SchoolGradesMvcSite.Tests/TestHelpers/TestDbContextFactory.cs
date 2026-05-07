using Microsoft.EntityFrameworkCore;
using SchoolGradesMvcSite.Data;
using SchoolGradesMvcSite.Models;

namespace SchoolGradesMvcSite.Tests.TestHelpers;

internal static class TestDbContextFactory
{
    public static ApplicationDbContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static SeededData SeedAcademicData(ApplicationDbContext context)
    {
        var studentA = new Student
        {
            FirstName = "Андрій",
            LastName = "Бойко",
            ClassName = "10-А",
            DateOfBirth = new DateTime(2008, 5, 14),
            IsActive = true
        };
        var studentB = new Student
        {
            FirstName = "Марія",
            LastName = "Коваль",
            ClassName = "10-А",
            DateOfBirth = new DateTime(2008, 7, 10),
            IsActive = true
        };
        var studentC = new Student
        {
            FirstName = "Ігор",
            LastName = "Сидоренко",
            ClassName = "9-Б",
            DateOfBirth = new DateTime(2009, 1, 21),
            IsActive = true
        };

        var teacherMath = new Teacher
        {
            FirstName = "Олена",
            LastName = "Іваненко",
            Email = "math@school.test",
            IsActive = true
        };
        var teacherHistory = new Teacher
        {
            FirstName = "Петро",
            LastName = "Шевчук",
            Email = "history@school.test",
            IsActive = true
        };

        var math = new Subject
        {
            Name = "Математика",
            Description = "Алгебра та геометрія",
            WeeklyHours = 4,
            IsActive = true
        };
        var history = new Subject
        {
            Name = "Історія",
            Description = "Історія України",
            WeeklyHours = 2,
            IsActive = true
        };

        context.Students.AddRange(studentA, studentB, studentC);
        context.Teachers.AddRange(teacherMath, teacherHistory);
        context.Subjects.AddRange(math, history);
        context.SaveChanges();

        var teacherSubject = new TeacherSubject
        {
            TeacherId = teacherMath.Id,
            SubjectId = math.Id
        };

        var grades = new[]
        {
            new Grade
            {
                StudentId = studentA.Id,
                SubjectId = math.Id,
                TeacherId = teacherMath.Id,
                Value = 12,
                Comment = "Контрольна",
                DateAssigned = new DateTime(2026, 3, 1)
            },
            new Grade
            {
                StudentId = studentA.Id,
                SubjectId = history.Id,
                TeacherId = teacherHistory.Id,
                Value = 10,
                Comment = "Усна відповідь",
                DateAssigned = new DateTime(2026, 3, 3)
            },
            new Grade
            {
                StudentId = studentB.Id,
                SubjectId = math.Id,
                TeacherId = teacherMath.Id,
                Value = 9,
                Comment = "Самостійна",
                DateAssigned = new DateTime(2026, 3, 4)
            },
            new Grade
            {
                StudentId = studentC.Id,
                SubjectId = history.Id,
                TeacherId = teacherHistory.Id,
                Value = 7,
                Comment = "Тест",
                DateAssigned = new DateTime(2026, 3, 2)
            }
        };

        context.TeacherSubjects.Add(teacherSubject);
        context.Grades.AddRange(grades);
        context.SaveChanges();

        return new SeededData(studentA, studentB, studentC, teacherMath, teacherHistory, math, history);
    }
}

internal sealed record SeededData(
    Student StudentA,
    Student StudentB,
    Student StudentC,
    Teacher TeacherMath,
    Teacher TeacherHistory,
    Subject SubjectMath,
    Subject SubjectHistory);
