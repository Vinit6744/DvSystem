using DesignReview.Domain.Enums;

namespace DesignReview.Application.Models
{
    /// <summary>
    /// Issue for consolidated list with page reference (for navigation).
    /// </summary>
    public class IssueDto
    {
        public Guid VerificationResultId { get; set; }
        public string CheckName { get; set; } = string.Empty;
        public VerificationOutcome Outcome { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? PageNumbers { get; set; }
        public int? FirstPageNumber { get; set; }
    }
}
