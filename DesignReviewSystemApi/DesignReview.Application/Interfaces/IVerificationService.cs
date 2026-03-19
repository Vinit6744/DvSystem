using DesignReview.Application.Models;

namespace DesignReview.Application.Interfaces
{
    public interface IVerificationService
    {
        /// <summary>
        /// Run verification checks against extracted content and store results.
        /// </summary>
        Task<VerificationRunResponseDto> RunVerificationAsync(Guid documentId, CancellationToken cancellationToken = default);
        Task<VerificationRunResponseDto?> GetVerificationResultsAsync(Guid documentId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<IssueDto>> GetIssuesAsync(Guid documentId, CancellationToken cancellationToken = default);
    }
}
