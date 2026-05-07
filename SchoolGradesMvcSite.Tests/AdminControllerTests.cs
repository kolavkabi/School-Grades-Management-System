using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SchoolGradesMvcSite.Controllers;
using SchoolGradesMvcSite.Infrastructure;
using SchoolGradesMvcSite.Models;
using SchoolGradesMvcSite.Tests.TestHelpers;
using SchoolGradesMvcSite.ViewModels;
using Xunit;

namespace SchoolGradesMvcSite.Tests;

public class AdminControllerTests
{
    [Fact]
    public async Task CreateUser_ValidModel_CreatesUserAndAssignsRole()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), "teacher123"))
            .ReturnsAsync(IdentityResult.Success);
        userManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), AppRoles.User))
            .ReturnsAsync(IdentityResult.Success);

        await using var context = TestDbContextFactory.CreateContext();
        var controller = new AdminController(userManager.Object, context);
        ControllerTestHelpers.AttachTempData(controller);

        var model = new AdminUserViewModel
        {
            FullName = "Test Teacher",
            UserName = "teacher2",
            Email = "teacher2@school.test",
            Role = AppRoles.User,
            Password = "teacher123",
            IsActive = true
        };

        var result = await controller.CreateUser(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.Users), redirect.ActionName);
        userManager.Verify(m => m.CreateAsync(It.Is<AppUser>(u => u.UserName == "teacher2" && u.IsActive), "teacher123"), Times.Once);
        userManager.Verify(m => m.AddToRoleAsync(It.Is<AppUser>(u => u.UserName == "teacher2"), AppRoles.User), Times.Once);
    }

    [Fact]
    public async Task CreateUser_IdentityError_ReturnsViewWithErrors()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), "teacher123"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Duplicate email" }));

        await using var context = TestDbContextFactory.CreateContext();
        var controller = new AdminController(userManager.Object, context);

        var model = new AdminUserViewModel
        {
            FullName = "Test Teacher",
            UserName = "teacher2",
            Email = "teacher2@school.test",
            Role = AppRoles.User,
            Password = "teacher123",
            IsActive = true
        };

        var result = await controller.CreateUser(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Same(model, view.Model);
        userManager.Verify(m => m.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ToggleUser_FlipsUserActivity()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        var user = new AppUser { Id = "u2", UserName = "worker", IsActive = true };
        userManager.Setup(m => m.FindByIdAsync("u2")).ReturnsAsync(user);
        userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        await using var context = TestDbContextFactory.CreateContext();
        var controller = new AdminController(userManager.Object, context);
        ControllerTestHelpers.AttachTempData(controller);

        var result = await controller.ToggleUser("u2");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.False(user.IsActive);
        userManager.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_MainAdmin_IsProtected()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        var user = new AppUser { Id = "admin-id", UserName = "admin", IsActive = true };
        userManager.Setup(m => m.FindByIdAsync("admin-id")).ReturnsAsync(user);

        await using var context = TestDbContextFactory.CreateContext();
        var controller = new AdminController(userManager.Object, context);
        ControllerTestHelpers.AttachTempData(controller);

        var result = await controller.DeleteUser("admin-id");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.Users), redirect.ActionName);
        Assert.Equal("Головного адміністратора не можна видалити.", controller.TempData["Error"]);
        userManager.Verify(m => m.DeleteAsync(It.IsAny<AppUser>()), Times.Never);
    }
}
