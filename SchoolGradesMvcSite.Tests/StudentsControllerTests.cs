using Microsoft.AspNetCore.Mvc;
using SchoolGradesMvcSite.Controllers;
using SchoolGradesMvcSite.Models;
using SchoolGradesMvcSite.Tests.TestHelpers;
using Xunit;

namespace SchoolGradesMvcSite.Tests;

public class StudentsControllerTests
{
    [Fact]
    public async Task Create_ValidStudent_PersistsAndRedirects()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new StudentsController(context);
        ControllerTestHelpers.AttachTempData(controller);

        var student = new Student
        {
            FirstName = "Іван",
            LastName = "Петренко",
            ClassName = "11-Б",
            DateOfBirth = new DateTime(2007, 11, 5),
            IsActive = true
        };

        var result = await controller.Create(student);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(StudentsController.Index), redirect.ActionName);
        Assert.Single(context.Students);
        Assert.Equal("Петренко", context.Students.Single().LastName);
    }

    [Fact]
    public async Task Import_ValidCsv_ImportsRows()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var controller = new StudentsController(context);
        ControllerTestHelpers.AttachTempData(controller);

        var csv = "FirstName,LastName,ClassName,DateOfBirth\n" +
                  "Олег,Романюк,8-А,2010-02-12\n" +
                  "Леся,Іванчук,8-А,2010-04-09\n";
        var file = ControllerTestHelpers.CreateCsvFile(csv);

        var result = await controller.Import(file);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(StudentsController.Index), redirect.ActionName);
        Assert.Equal(2, context.Students.Count());
    }

    [Fact]
    public async Task Index_FiltersBySearchAndClass()
    {
        await using var context = TestDbContextFactory.CreateContext();
        TestDbContextFactory.SeedAcademicData(context);
        var controller = new StudentsController(context);

        var result = await controller.Index("Бой", "10-А");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Student>>(view.Model).ToList();
        Assert.Single(model);
        Assert.Equal("Бойко", model[0].LastName);
    }
}
