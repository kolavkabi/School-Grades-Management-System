using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using Moq;
using SchoolGradesMvcSite.Models;
using System.Security.Claims;

namespace SchoolGradesMvcSite.Tests.TestHelpers;

internal static class ControllerTestHelpers
{
    public static void AttachTempData(Controller controller)
    {
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    public static void AttachAuthenticatedUser(Controller controller, AppUser user, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id ?? "test-user-id"),
            new(ClaimTypes.Name, user.UserName ?? "testuser")
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = controller.ControllerContext.HttpContext ?? new DefaultHttpContext();
        httpContext.User = principal;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData ??= new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    public static IFormFile CreateCsvFile(string content, string fileName = "import.csv")
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "csvFile", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };
    }
}
