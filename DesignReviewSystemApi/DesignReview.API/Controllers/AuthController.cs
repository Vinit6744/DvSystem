using DesignReview.Application.Interfaces;
using DesignReview.Application.Models;
using DesignReview.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesignReview.API.Controllers
{
    /// <summary>
    /// Auth endpoints. Login is anonymous (no JWT); Register requires Admin role.
    /// </summary>
    public class AuthController : BaseApiController<AuthController>
    {
        private readonly IAuthService _authService;

        public AuthController(
            ILogger<AuthController> logger,
            IAuthService authService,
            IUserHelperService userHelper)
            : base(logger, userHelper)
        {
            _authService = authService;
        }

        /// <summary>
        /// Login with username and password. Returns JWT with role claims. Anonymous (no JWT required).
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            if (result == null)
                return Unauthorized(new { message = "Invalid username or password." });
            return Ok(result);
        }

        /// <summary>
        /// Register a new user (Reviewer or Admin). Requires Admin role.
        /// </summary>
        [HttpPost("register")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, GetCurrentUserName(), cancellationToken);
            if (result == null)
                return BadRequest(new { message = "Registration failed. User may already exist or password does not meet requirements." });
            return Ok(result);
        }
    }
}
