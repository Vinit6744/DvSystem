using DesignReview.Application.Interfaces;
using DesignReview.Application.Models;
using DesignReview.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesignReview.API.Controllers
{
    /// <summary>
    /// Document upload and viewer (requirement 4.1): upload PDF, list, get detail with pages, jump to page, update status, delete, extract text. Admin and Reviewer access.
    /// </summary>
    [Authorize(Roles = "Admin,Reviewer")]
    public class DocumentsController : BaseApiController<DocumentsController>
    {
        private readonly IDocumentService _documentService;
        private readonly ITextExtractionService _textExtractionService;

        public DocumentsController(
            ILogger<DocumentsController> logger,
            IDocumentService documentService,
            ITextExtractionService textExtractionService,
            IUserHelperService userHelper)
            : base(logger, userHelper)
        {
            _documentService = documentService;
            _textExtractionService = textExtractionService;
        }

        /// <summary>
        /// Upload a PDF drawing document. Stores file and creates document metadata (Draft).
        /// </summary>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(UploadDocumentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UploadDocumentDto>> Upload(IFormFile file, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file or empty file." });
            }

            var maxBytes = _documentService.GetMaxUploadSizeBytes();
            if (maxBytes.HasValue && file.Length > maxBytes.Value)
            {
                return BadRequest(new { message = $"File size exceeds maximum allowed ({maxBytes.Value / (1024 * 1024)} MB)." });
            }
            var fileName = file.FileName ?? "document.pdf";
            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Only PDF files are allowed." });
            }
            try
            {
                await using var stream = file.OpenReadStream();
                var result = await _documentService.UploadAsync(stream, fileName, userId, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// List documents (metadata: filename, upload date, uploaded by, status). Optional filter by current user.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<DocumentMetadataDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IReadOnlyList<DocumentMetadataDto>>> GetList(
            [FromQuery] bool myDocumentsOnly = false,
            CancellationToken cancellationToken = default)
        {
            var userId = myDocumentsOnly ? GetCurrentUserId() : null;
            var list = await _documentService.GetListAsync(userId, cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Get document detail for viewer: metadata + pages (for navigation, zoom, jump to page).
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(DocumentDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var doc = await _documentService.GetByIdAsync(id, cancellationToken);
            if (doc == null)
                return NotFound();
            return Ok(doc);
        }

        /// <summary>
        /// Get the stored PDF file for viewing in the application (requirement 4.1). Use with PDF viewer controls (page navigation, zoom).
        /// </summary>
        [HttpGet("{id:guid}/file")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetFile(Guid id, CancellationToken cancellationToken)
        {
            var file = await _documentService.GetFileAsync(id, cancellationToken);
            if (file == null)
                return NotFound(new { message = "Document or file not found." });
            return File(file.Value.Stream, "application/pdf", file.Value.FileName);
        }

        /// <summary>
        /// Get a single page (for jump-to-page). Returns page number and extracted text.
        /// </summary>
        [HttpGet("{id:guid}/pages/{pageNumber:int}")]
        [ProducesResponseType(typeof(DocumentPageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentPageDto>> GetPage(Guid id, int pageNumber, CancellationToken cancellationToken)
        {
            if (pageNumber < 1)
            {
                return BadRequest(new { message = "Page number must be at least 1." });
            }
            var page = await _documentService.GetPageAsync(id, pageNumber, cancellationToken);
            if (page == null)
                return NotFound(new { message = "Page or document not found." });
            return Ok(page);
        }

        /// <summary>
        /// Extract text from PDF and store per page. Run this after upload before verification. Uses OCR fallback if needed.
        /// </summary>
        [HttpPost("{id:guid}/extract")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ExtractText(Guid id, CancellationToken cancellationToken)
        {
            var doc = await _documentService.GetByIdAsync(id, cancellationToken);
            if (doc == null)
                return NotFound(new { message = "Document not found." });
            try
            {
                await _textExtractionService.ExtractAndStoreAsync(id, cancellationToken);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update document review status (Draft, InReview, NeedsChanges, Approved).
        /// </summary>
        [HttpPut("{id:guid}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateReviewStatusDto dto, CancellationToken cancellationToken)
        {
            var updated = await _documentService.UpdateStatusAsync(id, dto.ReviewStatus, cancellationToken);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Delete a document and its stored file. Removes all related data (pages, verification results, comments, summary).
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _documentService.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return NotFound(new { message = "Document not found." });
            return NoContent();
        }
    }
}
