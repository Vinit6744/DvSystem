using DesignReview.Domain.Enums;

namespace DesignReview.Application.Models
{
    /// <summary>
    /// Request to update document review status.
    /// </summary>
    public class UpdateReviewStatusDto
    {
        public ReviewStatus ReviewStatus { get; set; }
    }
}
