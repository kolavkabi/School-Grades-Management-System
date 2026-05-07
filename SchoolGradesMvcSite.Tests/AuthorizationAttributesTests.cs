using Microsoft.AspNetCore.Authorization;
using SchoolGradesMvcSite.Controllers;
using SchoolGradesMvcSite.Infrastructure;
using Xunit;

namespace SchoolGradesMvcSite.Tests;

public class AuthorizationAttributesTests
{
    [Fact]
    public void Subjects_Index_AllowsAnonymousGuestAccess()
    {
        var method = typeof(SubjectsController).GetMethod(nameof(SubjectsController.Index));
        var allowAnonymous = method?.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).SingleOrDefault();

        Assert.NotNull(allowAnonymous);
    }

    [Fact]
    public void Account_Register_AllowsAnonymousAccess()
    {
        var method = typeof(AccountController).GetMethod(nameof(AccountController.Register), Type.EmptyTypes);
        var allowAnonymous = method?.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).SingleOrDefault();

        Assert.NotNull(allowAnonymous);
    }

    [Fact]
    public void Teachers_Controller_RequiresAdministratorRole()
    {
        var authorize = (AuthorizeAttribute?)typeof(TeachersController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .SingleOrDefault();

        Assert.NotNull(authorize);
        Assert.Equal(AppRoles.Administrator, authorize!.Roles);
    }

    [Fact]
    public void Reports_Controller_RequiresStaffRoles()
    {
        var authorize = (AuthorizeAttribute?)typeof(ReportsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .SingleOrDefault();

        Assert.NotNull(authorize);
        Assert.Equal(AppRoles.Staff, authorize!.Roles);
    }

    [Fact]
    public void Students_ToggleStatus_RequiresAdministratorRole()
    {
        var method = typeof(StudentsController).GetMethod(nameof(StudentsController.ToggleStatus));
        var authorize = (AuthorizeAttribute?)method?
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .SingleOrDefault();

        Assert.NotNull(authorize);
        Assert.Equal(AppRoles.Administrator, authorize!.Roles);
    }
}
