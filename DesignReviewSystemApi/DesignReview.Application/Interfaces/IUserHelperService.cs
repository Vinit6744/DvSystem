namespace DesignReview.Application.Interfaces
{
    /// <summary>
    /// Helper service for user-related operations (e.g. get user name by ID).
    /// </summary>
    public interface IUserHelperService
    {
        /// <summary>
        /// Gets user name (UserName or Email) by user ID.
        /// </summary>
        Task<string?> GetUserNameAsync(string userId, CancellationToken cancellationToken = default);
    }
}
