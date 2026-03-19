namespace DesignReview.Application.Models
{
    /// <summary>
    /// AI-generated review summary.
    /// </summary>
    public class ReviewSummaryDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }
}
