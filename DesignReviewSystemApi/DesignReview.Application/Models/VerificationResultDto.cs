using DesignReview.Domain.Enums;

namespace DesignReview.Application.Models
{
    /// <summary>
    /// Single verification check result: Pass / Fail / Manual Review.
    /// </summary>
    public class VerificationResultDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string CheckName { get; set; } = string.Empty;
        public VerificationOutcome Outcome { get; set; }
        public string? FoundValue { get; set; }
        public string? PageNumbers { get; set; }
        public string? Message { get; set; }
    }
}
