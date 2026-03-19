using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DesignReview.Application.Interfaces;
using DesignReview.Application.Models;
using DesignReview.Application.Options;
using DesignReview.BusinessLogic.Identity;
using DesignReview.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DesignReview.BusinessLogic.Services
{
    public class AuthService : IAuthService
    {
        public const string RolesCacheKeyPrefix = "user_roles_";
        public static readonly TimeSpan RolesCacheDuration = TimeSpan.FromMinutes(10);

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IMemoryCache _cache;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JwtSettings> jwtSettings,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings;
            _cache = cache;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
                return null;

            var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isValid)
                return null;

            var roles = await GetUserRolesCachedAsync(user.Id, cancellationToken);
            if (roles == null)
                roles = Array.Empty<string>();

            var token = GenerateJwtToken(user.Id, user.UserName ?? user.Email ?? "", roles);
            var settings = _jwtSettings.Value;

            return new LoginResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes),
                UserName = user.UserName ?? user.Email ?? "",
                UserId = user.Id,
                Roles = roles
            };
        }

        public async Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto request, string? createdById = null, CancellationToken cancellationToken = default)
        {
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return null;

            var role = Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var parsed) ? parsed : UserRole.Reviewer;
            var roleName = role.ToRoleName();
            if (await _roleManager.RoleExistsAsync(roleName))
                await _userManager.AddToRoleAsync(user, roleName);

            return await LoginAsync(new LoginRequestDto { UserName = request.UserName, Password = request.Password }, cancellationToken);
        }

        public async Task<IReadOnlyList<string>?> GetUserRolesCachedAsync(string userId, CancellationToken cancellationToken = default)
        {
            var cacheKey = RolesCacheKeyPrefix + userId;
            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached) && cached != null)
                return cached;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            var roleList = roles.ToList().AsReadOnly();
            _cache.Set(cacheKey, roleList, RolesCacheDuration);
            return roleList;
        }

        private string GenerateJwtToken(string userId, string userName, IReadOnlyList<string> roles)
        {
            var settings = _jwtSettings.Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, userName),
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var token = new JwtSecurityToken(
                issuer: settings.Issuer,
                audience: settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
