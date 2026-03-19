namespace DesignReview.Application.Models
{
    /// <summary>
    /// Response after running verification: all results + issues list.
    /// </summary>
    public class VerificationRunResponseDto
    {
        public Guid DocumentId { get; set; }
        public List<VerificationResultDto> Results { get; set; } = new();
        public List<IssueDto> Issues { get; set; } = new();
    }
}
