using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SchoolGradesMvcSite.Models;

namespace SchoolGradesMvcSite.Tests.TestHelpers;

internal static class IdentityMocks
{
    public static Mock<UserManager<AppUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<AppUser>>();
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new Mock<IPasswordHasher<AppUser>>();
        var userValidators = new List<IUserValidator<AppUser>>();
        var passwordValidators = new List<IPasswordValidator<AppUser>>();
        var normalizer = new Mock<ILookupNormalizer>();
        var errorDescriber = new IdentityErrorDescriber();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<AppUser>>>();

        return new Mock<UserManager<AppUser>>(
            store.Object,
            options,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            normalizer.Object,
            errorDescriber,
            services.Object,
            logger.Object);
    }

    public static Mock<SignInManager<AppUser>> CreateSignInManagerMock(UserManager<AppUser> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        contextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
        var options = Options.Create(new IdentityOptions());
        var logger = new Mock<ILogger<SignInManager<AppUser>>>();
        var schemes = new Mock<IAuthenticationSchemeProvider>();
        var userConfirmation = new Mock<IUserConfirmation<AppUser>>();

        return new Mock<SignInManager<AppUser>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            options,
            logger.Object,
            schemes.Object,
            userConfirmation.Object);
    }
}
