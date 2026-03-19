using System.ComponentModel.DataAnnotations;
using DesignReview.Domain.Enums;

namespace DesignReview.Application.Models
{
    public class RegisterRequestDto
    {
        [Required]
        [MinLength(2)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>Role name: Reviewer or Admin (from <see cref="UserRole"/>).</summary>
        public string Role { get; set; } = nameof(UserRole.Reviewer);
    }
}
