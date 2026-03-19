using DesignReview.Domain.Enums;

namespace DesignReview.Application.Models
{
    /// <summary>
    /// Full review report for export: metadata, verification, issues, comments, summary, status.
    /// </summary>
    public class ReviewReportDto
    {
        public DocumentMetadataDto DocumentMetadata { get; set; } = null!;
        public List<VerificationResultDto> VerificationResults { get; set; } = new();
        public List<IssueDto> Issues { get; set; } = new();
        public List<ReviewCommentDto> ReviewComments { get; set; } = new();
        public ReviewSummaryDto? ReviewSummary { get; set; }
        public ReviewStatus FinalStatus { get; set; }
    }
}
