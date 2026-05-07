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

public class AccountControllerTests
{
    [Fact]
    public void Login_Get_ReturnsViewAndPreservesReturnUrl()
    {
        var controller = CreateController();

        var result = controller.Login("/Grades");

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<LoginViewModel>(view.Model);
        Assert.Equal("/Grades", controller.ViewBag.ReturnUrl);
    }

    [Fact]
    public async Task Login_Post_InvalidModel_ReturnsSameView()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("UserName", "Required");

        var result = await controller.Login(new LoginViewModel { Password = "123456" });

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<LoginViewModel>(view.Model);
    }

    [Fact]
    public async Task Login_Post_InactiveUser_ReturnsViewWithError()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        var signInManager = IdentityMocks.CreateSignInManagerMock(userManager.Object);
        var user = new AppUser { UserName = "teacher", IsActive = false };
        userManager.Setup(m => m.FindByNameAsync("teacher")).ReturnsAsync(user);
        var controller = new AccountController(signInManager.Object, userManager.Object);

        var result = await controller.Login(new LoginViewModel
        {
            UserName = "teacher",
            Password = "teacher123",
            RememberMe = false
        });

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.IsType<LoginViewModel>(view.Model);
        signInManager.Verify(m => m.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_RedirectsHome()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        var signInManager = IdentityMocks.CreateSignInManagerMock(userManager.Object);
        var user = new AppUser { UserName = "teacher", IsActive = true };
        userManager.Setup(m => m.FindByNameAsync("teacher")).ReturnsAsync(user);
        signInManager
            .Setup(m => m.PasswordSignInAsync("teacher", "teacher123", true, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var controller = new AccountController(signInManager.Object, userManager.Object);

        var result = await controller.Login(new LoginViewModel
        {
            UserName = "teacher",
            Password = "teacher123",
            RememberMe = true
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public async Task Login_Post_WithLocalReturnUrl_RedirectsToThatUrl()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        var signInManager = IdentityMocks.CreateSignInManagerMock(userManager.Object);
        var user = new AppUser { UserName = "teacher", IsActive = true };
        userManager.Setup(m => m.FindByNameAsync("teacher")).ReturnsAsync(user);
        signInManager
            .Setup(m => m.PasswordSignInAsync("teacher", "teacher123", false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(x => x.IsLocalUrl("/Reports/Index")).Returns(true);

        var controller = new AccountController(signInManager.Object, userManager.Object)
        {
            Url = urlHelper.Object
        };

        var result = await controller.Login(new LoginViewModel
        {
            UserName = "teacher",
            Password = "teacher123",
            RememberMe = false
        }, "/Reports/Index");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Reports/Index", redirect.Url);
    }

    [Fact]
    public void Register_Get_ReturnsEmptyViewModel()
    {
        var controller = CreateController();

        var result = controller.Register();

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<RegisterViewModel>(view.Model);
    }

    [Fact]
    public async Task Register_Post_InvalidModel_ReturnsSameView()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Email", "Required");

        var result = await controller.Register(new RegisterViewModel());

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<RegisterViewModel>(view.Model);
    }

    [Fact]
    public async Task Register_Post_IdentityCreationFailure_ReturnsViewWithErrors()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        var signInManager = IdentityMocks.CreateSignInManagerMock(userManager.Object);
        userManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), "secret1"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Duplicate user" }));

        var controller = new AccountController(signInManager.Object, userManager.Object);

        var result = await controller.Register(new RegisterViewModel
        {
            FullName = "Test User",
            UserName = "teacher2",
            Email = "teacher2@school.test",
            Password = "secret1",
            ConfirmPassword = "secret1"
        });

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.IsType<RegisterViewModel>(view.Model);
        signInManager.Verify(m => m.SignInAsync(It.IsAny<AppUser>(), It.IsAny<bool>(), null), Times.Never);
    }

    [Fact]
    public async Task Register_Post_ValidModel_CreatesUserAssignsRoleAndSignsIn()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        var signInManager = IdentityMocks.CreateSignInManagerMock(userManager.Object);
        userManager
            .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), "secret1"))
            .ReturnsAsync(IdentityResult.Success);
        userManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), AppRoles.User))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AccountController(signInManager.Object, userManager.Object);
        ControllerTestHelpers.AttachTempData(controller);

        var result = await controller.Register(new RegisterViewModel
        {
            FullName = "Test User",
            UserName = "teacher2",
            Email = "teacher2@school.test",
            Password = "secret1",
            ConfirmPassword = "secret1"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
        userManager.Verify(m => m.CreateAsync(It.Is<AppUser>(u => u.UserName == "teacher2" && u.IsActive), "secret1"), Times.Once);
        userManager.Verify(m => m.AddToRoleAsync(It.Is<AppUser>(u => u.UserName == "teacher2"), AppRoles.User), Times.Once);
        signInManager.Verify(m => m.SignInAsync(It.Is<AppUser>(u => u.UserName == "teacher2"), false, null), Times.Once);
    }

    [Fact]
    public async Task Logout_Post_SignsOutAndRedirectsHome()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        var signInManager = IdentityMocks.CreateSignInManagerMock(userManager.Object);
        var controller = new AccountController(signInManager.Object, userManager.Object);

        var result = await controller.Logout();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
        signInManager.Verify(m => m.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_InvalidModel_ReturnsView()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("NewPassword", "Required");

        var result = await controller.ChangePassword(new ChangePasswordViewModel());

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task ChangePassword_UserMissing_RedirectsToLogin()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        var signInManager = IdentityMocks.CreateSignInManagerMock(userManager.Object);
        userManager.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync((AppUser?)null);
        var controller = new AccountController(signInManager.Object, userManager.Object);

        var result = await controller.ChangePassword(new ChangePasswordViewModel
        {
            CurrentPassword = "old123",
            NewPassword = "new123",
            ConfirmPassword = "new123"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AccountController.Login), redirect.ActionName);
    }

    [Fact]
    public async Task ChangePassword_Failure_ReturnsViewWithErrors()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        var signInManager = IdentityMocks.CreateSignInManagerMock(userManager.Object);
        var user = new AppUser { Id = "u1", UserName = "teacher", IsActive = true };
        userManager.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
        userManager
            .Setup(m => m.ChangePasswordAsync(user, "old123", "new123"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Wrong password" }));
        var controller = new AccountController(signInManager.Object, userManager.Object);

        var result = await controller.ChangePassword(new ChangePasswordViewModel
        {
            CurrentPassword = "old123",
            NewPassword = "new123",
            ConfirmPassword = "new123"
        });

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.IsType<ChangePasswordViewModel>(view.Model);
    }

    [Fact]
    public async Task ChangePassword_Success_RedirectsHome()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        var signInManager = IdentityMocks.CreateSignInManagerMock(userManager.Object);
        var user = new AppUser { Id = "u1", UserName = "teacher", IsActive = true };
        userManager.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
        userManager
            .Setup(m => m.ChangePasswordAsync(user, "old123", "new123"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AccountController(signInManager.Object, userManager.Object);
        ControllerTestHelpers.AttachTempData(controller);
        ControllerTestHelpers.AttachAuthenticatedUser(controller, user, AppRoles.User);

        var result = await controller.ChangePassword(new ChangePasswordViewModel
        {
            CurrentPassword = "old123",
            NewPassword = "new123",
            ConfirmPassword = "new123"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    private static AccountController CreateController()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        var signInManager = IdentityMocks.CreateSignInManagerMock(userManager.Object);
        return new AccountController(signInManager.Object, userManager.Object);
    }
}
