using Microsoft.AspNetCore.Mvc;
using SchoolGradesMvcSite.Controllers;
using SchoolGradesMvcSite.Models;
using SchoolGradesMvcSite.Tests.TestHelpers;
using SchoolGradesMvcSite.ViewModels;
using System.Globalization;
using Xunit;

namespace SchoolGradesMvcSite.Tests;

public class GradesControllerTests
{
    [Fact]
    public async Task Create_ValidGrade_PersistsAndRedirects()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var seeded = TestDbContextFactory.SeedAcademicData(context);
        var controller = new GradesController(context);
        ControllerTestHelpers.AttachTempData(controller);

        var grade = new Grade
        {
            StudentId = seeded.StudentB.Id,
            SubjectId = seeded.SubjectHistory.Id,
            TeacherId = seeded.TeacherHistory.Id,
            Value = 11,
            Comment = "Модульна робота",
            DateAssigned = new DateTime(2026, 3, 12)
        };

        var result = await controller.Create(grade);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(GradesController.Index), redirect.ActionName);
        Assert.Contains(context.Grades, g => g.Comment == "Модульна робота" && g.Value == 11);
    }

    [Fact]
    public async Task StudentAverage_ReturnsCalculatedAverage()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var seeded = TestDbContextFactory.SeedAcademicData(context);
        var controller = new GradesController(context);

        var result = await controller.StudentAverage(seeded.StudentA.Id);

        var view = Assert.IsType<ViewResult>(result);
        var averageText = Assert.IsType<string>(view.ViewData["Average"]);
        Assert.Equal(11m, decimal.Parse(averageText, CultureInfo.CurrentCulture));
    }

    [Fact]
    public async Task EditComment_UpdatesGradeComment()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var seeded = TestDbContextFactory.SeedAcademicData(context);
        var grade = context.Grades.First(g => g.StudentId == seeded.StudentA.Id);
        var controller = new GradesController(context);
        ControllerTestHelpers.AttachTempData(controller);

        var result = await controller.EditComment(new GradeCommentViewModel
        {
            GradeId = grade.Id,
            Comment = "Оновлений коментар"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(GradesController.Details), redirect.ActionName);
        Assert.Equal("Оновлений коментар", context.Grades.Single(g => g.Id == grade.Id).Comment);
    }
}
