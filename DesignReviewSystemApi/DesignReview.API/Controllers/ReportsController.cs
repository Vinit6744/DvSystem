using DesignReview.Application.Interfaces;
using DesignReview.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesignReview.API.Controllers
{
    /// <summary>
    /// Review report export (requirement 4.7): PDF or HTML with metadata, verification results, issues, comments, AI summary, status. Admin and Reviewer access.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Reviewer")]
    public class ReportsController : BaseApiController<ReportsController>
    {
        private readonly IReportExportService _reportExportService;
        private readonly IDocumentService _documentService;

        public ReportsController(
            ILogger<ReportsController> logger,
            IReportExportService reportExportService,
            IDocumentService documentService,
            IUserHelperService userHelper)
            : base(logger, userHelper)
        {
            _reportExportService = reportExportService;
            _documentService = documentService;
        }

        /// <summary>
        /// Export structured review report (PDF or HTML). Format: pdf or html.
        /// </summary>
        [HttpGet("documents/{documentId:guid}/export")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Export(Guid documentId, [FromQuery] string format = "pdf", CancellationToken cancellationToken = default)
        {
            var doc = await _documentService.GetByIdAsync(documentId, cancellationToken);
            if (doc == null)
                return NotFound(new { message = "Document not found." });
            if (string.IsNullOrWhiteSpace(format))
                format = "pdf";
            try
            {
                var (content, contentType, fileName) = await _reportExportService.ExportAsync(documentId, format, cancellationToken);
                return File(content, contentType, fileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
