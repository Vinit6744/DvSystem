using DesignReview.Application.Interfaces;
using DesignReview.Application.Models;
using DesignReview.BusinessLogic.DbContext;
using DesignReview.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DesignReview.BusinessLogic.Services
{
    public class ReviewCommentService : IReviewCommentService
    {
        private readonly DesignReviewDbContext _db;
        private readonly IUserHelperService _userHelper;

        public ReviewCommentService(
            DesignReviewDbContext db,
            IUserHelperService userHelper)
        {
            _db = db;
            _userHelper = userHelper;
        }

        public async Task<ReviewCommentDto> AddCommentAsync(
            Guid documentId,
            string authorId,
            AddReviewCommentDto dto,
            CancellationToken cancellationToken = default)
        {
            var document = await _db.ReviewDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

            if (document == null)
            {
                throw new InvalidOperationException($"Document {documentId} not found.");
            }

            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                throw new ArgumentException("Comment text cannot be empty.", nameof(dto));
            }

            var comment = new ReviewComment
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                AuthorId = authorId,
                Text = dto.Text.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.ReviewComments.Add(comment);
            await _db.SaveChangesAsync(cancellationToken);

            var userName = await _userHelper.GetUserNameAsync(authorId, cancellationToken);

            return new ReviewCommentDto
            {
                Id = comment.Id,
                DocumentId = comment.DocumentId,
                AuthorId = comment.AuthorId,
                AuthorUserName = userName,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<IReadOnlyList<ReviewCommentDto>> GetCommentsAsync(
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            var comments = await _db.ReviewComments
                .Where(c => c.DocumentId == documentId)
                .OrderBy(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = new List<ReviewCommentDto>(comments.Count);
            foreach (var comment in comments)
            {
                var userName = await _userHelper.GetUserNameAsync(comment.AuthorId, cancellationToken);
                dtos.Add(new ReviewCommentDto
                {
                    Id = comment.Id,
                    DocumentId = comment.DocumentId,
                    AuthorId = comment.AuthorId,
                    AuthorUserName = userName,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt
                });
            }

            return dtos;
        }

        public async Task<ReviewCommentDto?> GetCommentByIdAsync(Guid documentId, Guid commentId, CancellationToken cancellationToken = default)
        {
            var comment = await _db.ReviewComments
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.DocumentId == documentId && c.Id == commentId, cancellationToken);
            if (comment == null)
                return null;
            var userName = await _userHelper.GetUserNameAsync(comment.AuthorId, cancellationToken);
            return new ReviewCommentDto
            {
                Id = comment.Id,
                DocumentId = comment.DocumentId,
                AuthorId = comment.AuthorId,
                AuthorUserName = userName,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<ReviewCommentDto?> UpdateCommentAsync(Guid documentId, Guid commentId, string authorId, UpdateReviewCommentDto dto, CancellationToken cancellationToken = default)
        {
            var comment = await _db.ReviewComments
                .FirstOrDefaultAsync(c => c.DocumentId == documentId && c.Id == commentId, cancellationToken);
            if (comment == null)
                return null;
            if (comment.AuthorId != authorId)
                return null; // Only author can update
            if (string.IsNullOrWhiteSpace(dto?.Text))
                throw new ArgumentException("Comment text cannot be empty.", nameof(dto));
            comment.Text = dto.Text.Trim();
            await _db.SaveChangesAsync(cancellationToken);
            var userName = await _userHelper.GetUserNameAsync(comment.AuthorId, cancellationToken);
            return new ReviewCommentDto
            {
                Id = comment.Id,
                DocumentId = comment.DocumentId,
                AuthorId = comment.AuthorId,
                AuthorUserName = userName,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<bool> DeleteCommentAsync(Guid documentId, Guid commentId, CancellationToken cancellationToken = default)
        {
            var comment = await _db.ReviewComments
                .FirstOrDefaultAsync(c => c.DocumentId == documentId && c.Id == commentId, cancellationToken);
            if (comment == null)
                return false;
            _db.ReviewComments.Remove(comment);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
