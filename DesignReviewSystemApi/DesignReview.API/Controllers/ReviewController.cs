using DesignReview.Application.Interfaces;
using DesignReview.Application.Models;
using DesignReview.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesignReview.API.Controllers
{
    /// <summary>
    /// Review status, comments, and AI summary (requirements 4.5, 4.6): comments CRUD, generate/get review summary. Admin and Reviewer access.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Reviewer")]
    public class ReviewController : BaseApiController<ReviewController>
    {
        private readonly IReviewCommentService _commentService;
        private readonly IReviewSummaryService _summaryService;
        private readonly IDocumentService _documentService;

        public ReviewController(
            ILogger<ReviewController> logger,
            IReviewCommentService commentService,
            IReviewSummaryService summaryService,
            IDocumentService documentService,
            IUserHelperService userHelper)
            : base(logger, userHelper)
        {
            _commentService = commentService;
            _summaryService = summaryService;
            _documentService = documentService;
        }

        /// <summary>
        /// Get all review comments for a document.
        /// </summary>
        [HttpGet("documents/{documentId:guid}/comments")]
        [ProducesResponseType(typeof(IReadOnlyList<ReviewCommentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<ReviewCommentDto>>> GetComments(Guid documentId, CancellationToken cancellationToken)
        {
            var doc = await _documentService.GetByIdAsync(documentId, cancellationToken);
            if (doc == null)
                return NotFound(new { message = "Document not found." });
            var comments = await _commentService.GetCommentsAsync(documentId, cancellationToken);
            return Ok(comments);
        }

        /// <summary>
        /// Get a single comment by id.
        /// </summary>
        [HttpGet("documents/{documentId:guid}/comments/{commentId:guid}")]
        [ProducesResponseType(typeof(ReviewCommentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReviewCommentDto>> GetComment(Guid documentId, Guid commentId, CancellationToken cancellationToken)
        {
            var comment = await _commentService.GetCommentByIdAsync(documentId, commentId, cancellationToken);
            if (comment == null)
                return NotFound(new { message = "Comment not found." });
            return Ok(comment);
        }

        /// <summary>
        /// Add a review comment.
        /// </summary>
        [HttpPost("documents/{documentId:guid}/comments")]
        [ProducesResponseType(typeof(ReviewCommentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReviewCommentDto>> AddComment(Guid documentId, [FromBody] AddReviewCommentDto dto, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var doc = await _documentService.GetByIdAsync(documentId, cancellationToken);
            if (doc == null)
                return NotFound(new { message = "Document not found." });
            try
            {
                var comment = await _commentService.AddCommentAsync(documentId, userId, dto, cancellationToken);
                return CreatedAtAction(nameof(GetComment), new { documentId, commentId = comment.Id }, comment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update a comment (author only).
        /// </summary>
        [HttpPut("documents/{documentId:guid}/comments/{commentId:guid}")]
        [ProducesResponseType(typeof(ReviewCommentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ReviewCommentDto>> UpdateComment(Guid documentId, Guid commentId, [FromBody] UpdateReviewCommentDto dto, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var comment = await _commentService.UpdateCommentAsync(documentId, commentId, userId, dto, cancellationToken);
            if (comment == null)
                return NotFound(new { message = "Comment not found or you are not the author." });
            return Ok(comment);
        }

        /// <summary>
        /// Delete a comment.
        /// </summary>
        [HttpDelete("documents/{documentId:guid}/comments/{commentId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteComment(Guid documentId, Guid commentId, CancellationToken cancellationToken)
        {
            var deleted = await _commentService.DeleteCommentAsync(documentId, commentId, cancellationToken);
            if (!deleted)
                return NotFound(new { message = "Comment not found." });
            return NoContent();
        }

        /// <summary>
        /// Generate AI review summary from extracted content and verification results. Store per document.
        /// </summary>
        [HttpPost("documents/{documentId:guid}/summary/generate")]
        [ProducesResponseType(typeof(ReviewSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReviewSummaryDto>> GenerateSummary(Guid documentId, CancellationToken cancellationToken)
        {
            var doc = await _documentService.GetByIdAsync(documentId, cancellationToken);
            if (doc == null)
                return NotFound(new { message = "Document not found." });
            try
            {
                var summary = await _summaryService.GenerateSummaryAsync(documentId, cancellationToken);
                return Ok(summary);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get stored AI review summary for a document.
        /// </summary>
        [HttpGet("documents/{documentId:guid}/summary")]
        [ProducesResponseType(typeof(ReviewSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReviewSummaryDto>> GetSummary(Guid documentId, CancellationToken cancellationToken)
        {
            var summary = await _summaryService.GetSummaryAsync(documentId, cancellationToken);
            if (summary == null)
                return NotFound(new { message = "No summary. Generate first (POST .../summary/generate)." });
            return Ok(summary);
        }
    }
}
