using DesignReview.Domain.Enums;

namespace DesignReview.Application.Models
{
    /// <summary>
    /// Response after uploading a PDF document.
    /// </summary>
    public class UploadDocumentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string UploadedById { get; set; } = string.Empty;
        public ReviewStatus ReviewStatus { get; set; }
        public string? FileStoragePath { get; set; }
    }
}
