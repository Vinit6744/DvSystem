namespace DesignReview.Application.Models
{
    public class VerificationRuleDto
    {
        public Guid Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string RuleType { get; set; } = "Custom";
        public string SearchPatterns { get; set; } = "[]";
        public string? ValidationPattern { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
