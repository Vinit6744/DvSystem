namespace DesignReview.Application.Models
{
    /// <summary>
    /// Page content for PDF viewer and extraction.
    /// </summary>
    public class DocumentPageDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public int PageNumber { get; set; }
        public string ExtractedText { get; set; } = string.Empty;
    }
}
