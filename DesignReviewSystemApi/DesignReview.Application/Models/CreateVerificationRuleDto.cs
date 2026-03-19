namespace DesignReview.Application.Models
{
    public class CreateVerificationRuleDto
    {
        public string RuleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string RuleType { get; set; } = "Custom";
        /// <summary>JSON array of regex patterns, e.g. ["pattern1", "pattern2"].</summary>
        public string SearchPatterns { get; set; } = "[]";
        public string? ValidationPattern { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}
