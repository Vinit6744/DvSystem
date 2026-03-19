namespace DesignReview.Domain.Enums
{
    /// <summary>
    /// Application user roles. Names match ASP.NET Identity role strings.
    /// </summary>
    public enum UserRole
    {
        Reviewer = 0,
        Admin = 1
    }

    public static class UserRoleExtensions
    {
        /// <summary>
        /// Returns the role name string used by ASP.NET Identity.
        /// </summary>
        public static string ToRoleName(this UserRole role) => role.ToString();
    }
}
