using DesignReview.Domain.Enums;

namespace DesignReview.Application.Models
{
    /// <summary>
    /// Full document detail for viewer: metadata + pages.
    /// </summary>
    public class DocumentDetailDto
    {
        public DocumentMetadataDto Metadata { get; set; } = null!;
        public List<DocumentPageDto> Pages { get; set; } = new();
        public int TotalPages => Pages.Count;
    }
}
