using DesignReview.Application.Models;

namespace DesignReview.Application.Interfaces
{
    public interface IReportExportService
    {
        /// <summary>
        /// Export structured review report (PDF or HTML).
        /// </summary>
        /// <param name="documentId">Document id.</param>
        /// <param name="format">"pdf" or "html".</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>File content and content type.</returns>
        Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(Guid documentId, string format, CancellationToken cancellationToken = default);
    }
}
