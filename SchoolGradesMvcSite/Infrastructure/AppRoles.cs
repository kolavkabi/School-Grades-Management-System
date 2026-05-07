namespace SchoolGradesMvcSite.Infrastructure;

public static class AppRoles
{
    public const string Guest = "Guest";
    public const string User = "User";
    public const string Administrator = "Administrator";
    public const string Staff = User + "," + Administrator;

    public static readonly string[] ManagedRoles = [Administrator, User];
}
