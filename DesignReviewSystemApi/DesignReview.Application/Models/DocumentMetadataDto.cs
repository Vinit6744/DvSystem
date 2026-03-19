using DesignReview.Domain.Enums;

namespace DesignReview.Application.Models
{
    /// <summary>
    /// Document metadata for list view and detail header.
    /// </summary>
    public class DocumentMetadataDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string UploadedById { get; set; } = string.Empty;
        public string? UploadedByUserName { get; set; }
        public ReviewStatus ReviewStatus { get; set; }
    }
}
