namespace DesignReview.Application.Models
{
    /// <summary>
    /// Review comment for display and add.
    /// </summary>
    public class ReviewCommentDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string? AuthorUserName { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
