using DesignReview.Application.Models;

namespace DesignReview.Application.Interfaces
{
    public interface IVerificationRuleService
    {
        Task<IReadOnlyList<VerificationRuleDto>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
        Task<VerificationRuleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<VerificationRuleDto> CreateAsync(CreateVerificationRuleDto dto, CancellationToken cancellationToken = default);
        Task<VerificationRuleDto?> UpdateAsync(Guid id, UpdateVerificationRuleDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
