using System.Security.Claims;
using DesignReview.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DesignReview.API.Controllers
{
    /// <summary>
    /// Base controller for all API endpoints. Applies JWT authentication by default.
    /// Override with [AllowAnonymous] on specific actions (e.g. login) where needed.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public abstract class BaseApiController<T> : ControllerBase
    {
        private readonly IUserHelperService _userHelper;

        public BaseApiController(ILogger<T> logger, IUserHelperService userHelper)
        {
            Logger = logger;
            _userHelper = userHelper;
        }

        public ILogger<T> Logger { get; }

        /// <summary>
        /// Gets the current authenticated user's ID from JWT claims.
        /// </summary>
        protected string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>
        /// Gets the current authenticated user's name from JWT claims.
        /// </summary>
        protected string? GetCurrentUserName() => User.FindFirstValue(ClaimTypes.Name);

        /// <summary>
        /// Gets user name (UserName or Email) by user ID.
        /// </summary>
        protected async Task<string?> GetUserNameAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _userHelper.GetUserNameAsync(userId, cancellationToken);
        }

        /// <summary>
        /// Gets user name (UserName or Email) for the current authenticated user.
        /// </summary>
        protected async Task<string?> GetCurrentUserNameAsync(CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return null;
            return await GetUserNameAsync(userId, cancellationToken);
        }
    }
}
