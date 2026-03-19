using DesignReview.Application.Models;
using DesignReview.Domain.Enums;

namespace DesignReview.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<UploadDocumentDto> UploadAsync(Stream fileStream, string fileName, string uploadedById, CancellationToken cancellationToken = default);
        Task<DocumentDetailDto?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DocumentMetadataDto>> GetListAsync(string? uploadedById = null, CancellationToken cancellationToken = default);
        Task<DocumentPageDto?> GetPageAsync(Guid documentId, int pageNumber, CancellationToken cancellationToken = default);
        Task<bool> UpdateStatusAsync(Guid documentId, ReviewStatus status, CancellationToken cancellationToken = default);
        /// <summary>
        /// Delete a document and its file. Removes document, pages, verification results, comments, and summary.
        /// </summary>
        Task<bool> DeleteAsync(Guid documentId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get the stored PDF file stream for viewing/download. Returns null if document or file not found.
        /// Caller does not need to dispose the stream when passing to File(stream, ...) — the framework disposes it.
        /// </summary>
        Task<(Stream Stream, string FileName)?> GetFileAsync(Guid documentId, CancellationToken cancellationToken = default);
        /// <summary>Maximum allowed upload size in bytes; null means no limit (subject to server config).</summary>
        long? GetMaxUploadSizeBytes();
    }
}
