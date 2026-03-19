namespace DesignReview.Application.Models
{
    /// <summary>
    /// Request to update a review comment.
    /// </summary>
    public class UpdateReviewCommentDto
    {
        public string Text { get; set; } = string.Empty;
    }
}