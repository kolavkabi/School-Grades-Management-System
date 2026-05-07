using Microsoft.AspNetCore.Mvc;
using SchoolGradesMvcSite.Controllers;
using SchoolGradesMvcSite.Tests.TestHelpers;
using SchoolGradesMvcSite.ViewModels;
using Xunit;

namespace SchoolGradesMvcSite.Tests;

public class TeachersControllerTests
{
    [Fact]
    public async Task AssignSubject_Post_CreatesTeacherSubjectLinkOnlyOnce()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var seeded = TestDbContextFactory.SeedAcademicData(context);
        var controller = new TeachersController(context);
        ControllerTestHelpers.AttachTempData(controller);

        var model = new TeacherAssignSubjectViewModel
        {
            TeacherId = seeded.TeacherHistory.Id,
            SubjectId = seeded.SubjectMath.Id
        };

        var firstResult = await controller.AssignSubject(model);
        var secondResult = await controller.AssignSubject(model);

        Assert.IsType<RedirectToActionResult>(firstResult);
        Assert.IsType<RedirectToActionResult>(secondResult);
        Assert.Single(context.TeacherSubjects.Where(ts => ts.TeacherId == seeded.TeacherHistory.Id && ts.SubjectId == seeded.SubjectMath.Id));
    }
}
