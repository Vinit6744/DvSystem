using DesignReview.Application.Models;

namespace DesignReview.Application.Interfaces
{
    public interface IReviewCommentService
    {
        Task<ReviewCommentDto> AddCommentAsync(Guid documentId, string authorId, AddReviewCommentDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReviewCommentDto>> GetCommentsAsync(Guid documentId, CancellationToken cancellationToken = default);
        Task<ReviewCommentDto?> GetCommentByIdAsync(Guid documentId, Guid commentId, CancellationToken cancellationToken = default);
        Task<ReviewCommentDto?> UpdateCommentAsync(Guid documentId, Guid commentId, string authorId, UpdateReviewCommentDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteCommentAsync(Guid documentId, Guid commentId, CancellationToken cancellationToken = default);
    }
}
