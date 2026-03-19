namespace DesignReview.Application.Models
{
    /// <summary>
    /// Request for report export format.
    /// </summary>
    public class ExportReportRequestDto
    {
        public const string FormatPdf = "pdf";
        public const string FormatHtml = "html";

        public string Format { get; set; } = FormatPdf;
    }
}
