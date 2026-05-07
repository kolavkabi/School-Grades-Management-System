using Microsoft.AspNetCore.Mvc;
using SchoolGradesMvcSite.Controllers;
using SchoolGradesMvcSite.Tests.TestHelpers;
using SchoolGradesMvcSite.ViewModels;
using System.Text;
using Xunit;

namespace SchoolGradesMvcSite.Tests;

public class ReportsControllerTests
{
    [Fact]
    public async Task Ranking_FiltersByClassAndOrdersByAverageDescending()
    {
        await using var context = TestDbContextFactory.CreateContext();
        TestDbContextFactory.SeedAcademicData(context);
        var controller = new ReportsController(context);

        var result = await controller.Ranking("10-А");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<RankingRowViewModel>>(view.Model).ToList();
        Assert.Equal(2, model.Count);
        Assert.Equal("Бойко Андрій", model[0].StudentName);
        Assert.True(model[0].Average > model[1].Average);
        Assert.All(model, row => Assert.Equal("10-А", row.ClassName));
    }

    [Fact]
    public async Task ExportStudentReportCsv_ReturnsCsvFile()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var seeded = TestDbContextFactory.SeedAcademicData(context);
        var controller = new ReportsController(context);

        var result = await controller.ExportStudentReportCsv(seeded.StudentA.Id);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", file.ContentType);
        Assert.Contains("student-report", file.FileDownloadName);
        var csv = Encoding.UTF8.GetString(file.FileContents);
        Assert.Contains("Бойко Андрій", csv);
        Assert.Contains("Математика", csv);
    }

    [Fact]
    public async Task TopStudentInClass_ReturnsBestStudentForClass()
    {
        await using var context = TestDbContextFactory.CreateContext();
        TestDbContextFactory.SeedAcademicData(context);
        var controller = new ReportsController(context);

        var result = await controller.TopStudentInClass("10-А");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TopStudentInClassViewModel>(view.Model);
        Assert.Equal("Бойко Андрій", model.StudentName);
        Assert.Equal(11m, model.Average);
    }
}
