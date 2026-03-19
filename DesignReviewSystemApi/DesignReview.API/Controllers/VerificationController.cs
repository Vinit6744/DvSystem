using DesignReview.Application.Interfaces;
using DesignReview.Application.Models;
using DesignReview.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesignReview.API.Controllers
{
    /// <summary>
    /// Verification checks (requirement 4.3, 4.4): run checks, get results, get issues list with page references. Admin and Reviewer access.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Reviewer")]
    public class VerificationController : BaseApiController<VerificationController>
    {
        private readonly IVerificationService _verificationService;
        private readonly IDocumentService _documentService;

        public VerificationController(
            ILogger<VerificationController> logger,
            IVerificationService verificationService,
            IDocumentService documentService,
            IUserHelperService userHelper)
            : base(logger, userHelper)
        {
            _verificationService = verificationService;
            _documentService = documentService;
        }

        /// <summary>
        /// Run verification checks against extracted content. Requires text extraction to be done first.
        /// Returns Pass/Fail/Manual Review per check with found value and page numbers.
        /// </summary>
        [HttpPost("documents/{documentId:guid}/run")]
        [ProducesResponseType(typeof(VerificationRunResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VerificationRunResponseDto>> RunVerification(Guid documentId, CancellationToken cancellationToken)
        {
            var doc = await _documentService.GetByIdAsync(documentId, cancellationToken);
            if (doc == null)
                return NotFound(new { message = "Document not found." });
            if (doc.Pages == null || doc.Pages.Count == 0)
                return BadRequest(new { message = "Extract text first (POST /api/documents/{id}/extract)." });
            try
            {
                var result = await _verificationService.RunVerificationAsync(documentId, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get verification results for a document (all checks: Pass/Fail/Manual Review, found value, page numbers).
        /// </summary>
        [HttpGet("documents/{documentId:guid}/results")]
        [ProducesResponseType(typeof(VerificationRunResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VerificationRunResponseDto>> GetResults(Guid documentId, CancellationToken cancellationToken)
        {
            var result = await _verificationService.GetVerificationResultsAsync(documentId, cancellationToken);
            if (result == null)
                return NotFound(new { message = "No verification results. Run verification first." });
            return Ok(result);
        }

        /// <summary>
        /// Get consolidated issues list with page references for navigation (Fail and Manual Review only).
        /// </summary>
        [HttpGet("documents/{documentId:guid}/issues")]
        [ProducesResponseType(typeof(IReadOnlyList<IssueDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<IssueDto>>> GetIssues(Guid documentId, CancellationToken cancellationToken)
        {
            var issues = await _verificationService.GetIssuesAsync(documentId, cancellationToken);
            return Ok(issues);
        }
    }
}
