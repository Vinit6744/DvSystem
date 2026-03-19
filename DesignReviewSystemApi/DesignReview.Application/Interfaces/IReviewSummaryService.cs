using DesignReview.Application.Models;

namespace DesignReview.Application.Interfaces
{
    public interface IReviewSummaryService
    {
        /// <summary>
        /// Generate AI review summary from extracted content and verification results.
        /// </summary>
        Task<ReviewSummaryDto> GenerateSummaryAsync(Guid documentId, CancellationToken cancellationToken = default);
        Task<ReviewSummaryDto?> GetSummaryAsync(Guid documentId, CancellationToken cancellationToken = default);
    }
}
