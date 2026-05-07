using Microsoft.AspNetCore.Mvc;
using SchoolGradesMvcSite.Controllers;
using SchoolGradesMvcSite.Models;
using SchoolGradesMvcSite.Tests.TestHelpers;
using Xunit;

namespace SchoolGradesMvcSite.Tests;

public class SubjectsControllerTests
{
    [Fact]
    public async Task Index_ReturnsOrderedSubjects_ForGuestUseCase()
    {
        await using var context = TestDbContextFactory.CreateContext();
        context.Subjects.AddRange(
            new Subject { Name = "Фізика", WeeklyHours = 2, IsActive = true },
            new Subject { Name = "Алгебра", WeeklyHours = 4, IsActive = true },
            new Subject { Name = "Біологія", WeeklyHours = 2, IsActive = true });
        await context.SaveChangesAsync();

        var controller = new SubjectsController(context);
        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Subject>>(view.Model);
        Assert.Equal(new[] { "Алгебра", "Біологія", "Фізика" }, model.Select(s => s.Name).ToArray());
    }
}
