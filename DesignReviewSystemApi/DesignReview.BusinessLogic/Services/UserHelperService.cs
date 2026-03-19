using DesignReview.Application.Interfaces;
using DesignReview.BusinessLogic.Identity;
using Microsoft.AspNetCore.Identity;

namespace DesignReview.BusinessLogic.Services
{
    public class UserHelperService : IUserHelperService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserHelperService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string?> GetUserNameAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            var user = await _userManager.FindByIdAsync(userId);
            return user?.UserName ?? user?.Email;
        }
    }
}
