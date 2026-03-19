using DesignReview.Application.Models;

namespace DesignReview.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
        Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto request, string? createdById = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<string>?> GetUserRolesCachedAsync(string userId, CancellationToken cancellationToken = default);
    }
}
