namespace DesignReview.Application.Interfaces
{
    public interface ITextExtractionService
    {
        /// <summary>
        /// Extract text from document PDF and store per page. Uses OCR fallback if needed.
        /// </summary>
        Task ExtractAndStoreAsync(Guid documentId, CancellationToken cancellationToken = default);
    }
}
