namespace DesignReview.Application.Models
{
    /// <summary>
    /// Request to add a review comment.
    /// </summary>
    public class AddReviewCommentDto
    {
        public string Text { get; set; } = string.Empty;
    }
}
