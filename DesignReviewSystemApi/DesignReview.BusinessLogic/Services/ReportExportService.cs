using DesignReview.Application.Interfaces;
using DesignReview.Application.Models;
using DesignReview.BusinessLogic.DbContext;
using DesignReview.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace DesignReview.BusinessLogic.Services
{
    public class ReportExportService : IReportExportService
    {
        private readonly DesignReviewDbContext _db;
        private readonly IUserHelperService _userHelper;

        public ReportExportService(
            DesignReviewDbContext db,
            IUserHelperService userHelper)
        {
            _db = db;
            _userHelper = userHelper;
        }

        public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(
            Guid documentId,
            string format,
            CancellationToken cancellationToken = default)
        {
            var document = await _db.ReviewDocuments
                .Include(d => d.DocumentPages)
                .Include(d => d.VerificationResults)
                .Include(d => d.ReviewComments)
                .Include(d => d.ReviewSummary)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

            if (document == null)
            {
                throw new InvalidOperationException($"Document {documentId} not found.");
            }

            var uploadedByUserName = await _userHelper.GetUserNameAsync(document.UploadedById, cancellationToken);

            var report = new ReviewReportDto
            {
                DocumentMetadata = new DocumentMetadataDto
                {
                    Id = document.Id,
                    FileName = document.FileName,
                    UploadedAt = document.UploadedAt,
                    UploadedById = document.UploadedById,
                    UploadedByUserName = uploadedByUserName,
                    ReviewStatus = document.ReviewStatus
                },
                VerificationResults = document.VerificationResults.Select(r => new VerificationResultDto
                {
                    Id = r.Id,
                    DocumentId = r.DocumentId,
                    CheckName = r.CheckName,
                    Outcome = r.Outcome,
                    FoundValue = r.FoundValue,
                    PageNumbers = r.PageNumbers,
                    Message = r.Message
                }).ToList(),
                Issues = document.VerificationResults
                    .Where(r => r.Outcome != Domain.Enums.VerificationOutcome.Pass)
                    .Select(r => new IssueDto
                    {
                        VerificationResultId = r.Id,
                        CheckName = r.CheckName,
                        Outcome = r.Outcome,
                        Message = r.Message ?? $"{r.CheckName} check failed",
                        PageNumbers = r.PageNumbers,
                        FirstPageNumber = ParsePageNumber(r.PageNumbers)
                    }).ToList(),
                ReviewComments = await BuildReviewCommentsWithAuthorNamesAsync(document.ReviewComments.ToList(), cancellationToken),
                ReviewSummary = document.ReviewSummary != null
                    ? new ReviewSummaryDto
                    {
                        Id = document.ReviewSummary.Id,
                        DocumentId = document.ReviewSummary.DocumentId,
                        SummaryText = document.ReviewSummary.SummaryText,
                        GeneratedAt = document.ReviewSummary.GeneratedAt
                    }
                    : null,
                FinalStatus = document.ReviewStatus
            };

            format = format.ToLowerInvariant();
            if (format == "pdf")
            {
                return await ExportPdfAsync(report, document.FileName);
            }
            else if (format == "html")
            {
                return await ExportHtmlAsync(report, document.FileName);
            }
            else
            {
                throw new ArgumentException($"Unsupported format: {format}. Supported formats: pdf, html");
            }
        }

        private async Task<(byte[] Content, string ContentType, string FileName)> ExportPdfAsync(
            ReviewReportDto report,
            string originalFileName)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Design Review Report")
                        .FontSize(16)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .Column(column =>
                        {
                            column.Spacing(10);

                            // Document Metadata
                            column.Item().PaddingBottom(10).Column(col =>
                            {
                                col.Item().Text("Document Information").FontSize(14).Bold();
                                col.Item().Text($"File Name: {report.DocumentMetadata.FileName}");
                                col.Item().Text($"Uploaded By: {report.DocumentMetadata.UploadedByUserName ?? report.DocumentMetadata.UploadedById}");
                                col.Item().Text($"Upload Date: {report.DocumentMetadata.UploadedAt:yyyy-MM-dd HH:mm:ss}");
                                col.Item().Text($"Status: {report.DocumentMetadata.ReviewStatus}");
                            });

                            // Verification Results
                            column.Item().PaddingTop(10).Column(col =>
                            {
                                col.Item().Text("Verification Results").FontSize(14).Bold();
                                foreach (var result in report.VerificationResults)
                                {
                                    col.Item().PaddingLeft(10).Text($"{result.CheckName}: {result.Outcome}")
                                        .FontSize(9);
                                    if (!string.IsNullOrEmpty(result.FoundValue))
                                    {
                                        col.Item().PaddingLeft(20).Text($"Found: {result.FoundValue}").FontSize(8);
                                    }
                                    if (!string.IsNullOrEmpty(result.PageNumbers))
                                    {
                                        col.Item().PaddingLeft(20).Text($"Page(s): {result.PageNumbers}").FontSize(8);
                                    }
                                }
                            });

                            // Issues
                            if (report.Issues.Any())
                            {
                                column.Item().PaddingTop(10).Column(col =>
                                {
                                    col.Item().Text("Issues").FontSize(14).Bold();
                                    foreach (var issue in report.Issues)
                                    {
                                        col.Item().PaddingLeft(10).Text($"{issue.CheckName}: {issue.Message}")
                                            .FontSize(9);
                                        if (issue.FirstPageNumber.HasValue)
                                        {
                                            col.Item().PaddingLeft(20).Text($"Page: {issue.FirstPageNumber}").FontSize(8);
                                        }
                                    }
                                });
                            }

                            // Comments
                            if (report.ReviewComments.Any())
                            {
                                column.Item().PaddingTop(10).Column(col =>
                                {
                                    col.Item().Text("Review Comments").FontSize(14).Bold();
                                    foreach (var comment in report.ReviewComments)
                                    {
                                        col.Item().PaddingLeft(10).Text($"{comment.CreatedAt:yyyy-MM-dd}: {comment.Text}")
                                            .FontSize(9);
                                    }
                                });
                            }

                            // Summary
                            if (report.ReviewSummary != null)
                            {
                                column.Item().PaddingTop(10).Column(col =>
                                {
                                    col.Item().Text("AI Review Summary").FontSize(14).Bold();
                                    col.Item().PaddingLeft(10).Text(report.ReviewSummary.SummaryText)
                                        .FontSize(9);
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                        .FontSize(8);
                });
            }).GeneratePdf();

            var fileName = $"{Path.GetFileNameWithoutExtension(originalFileName)}_ReviewReport.pdf";
            return (pdfBytes, "application/pdf", fileName);
        }

        private async Task<(byte[] Content, string ContentType, string FileName)> ExportHtmlAsync(
            ReviewReportDto report,
            string originalFileName)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<title>Design Review Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1 { color: #333; }");
            html.AppendLine("h2 { color: #666; margin-top: 20px; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin: 10px 0; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f2f2f2; }");
            html.AppendLine(".pass { color: green; }");
            html.AppendLine(".fail { color: red; }");
            html.AppendLine(".manual { color: orange; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            html.AppendLine("<h1>Design Review Report</h1>");

            // Document Metadata
            html.AppendLine("<h2>Document Information</h2>");
            html.AppendLine("<table>");
            html.AppendLine($"<tr><th>File Name</th><td>{report.DocumentMetadata.FileName}</td></tr>");
            html.AppendLine($"<tr><th>Uploaded By</th><td>{report.DocumentMetadata.UploadedByUserName ?? report.DocumentMetadata.UploadedById}</td></tr>");
            html.AppendLine($"<tr><th>Upload Date</th><td>{report.DocumentMetadata.UploadedAt:yyyy-MM-dd HH:mm:ss}</td></tr>");
            html.AppendLine($"<tr><th>Status</th><td>{report.DocumentMetadata.ReviewStatus}</td></tr>");
            html.AppendLine("</table>");

            // Verification Results
            html.AppendLine("<h2>Verification Results</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Check Name</th><th>Outcome</th><th>Found Value</th><th>Page(s)</th><th>Message</th></tr>");
            foreach (var result in report.VerificationResults)
            {
                var outcomeClass = result.Outcome.ToString().ToLower();
                html.AppendLine($"<tr>");
                html.AppendLine($"<td>{result.CheckName}</td>");
                html.AppendLine($"<td class='{outcomeClass}'>{result.Outcome}</td>");
                html.AppendLine($"<td>{result.FoundValue ?? "-"}</td>");
                html.AppendLine($"<td>{result.PageNumbers ?? "-"}</td>");
                html.AppendLine($"<td>{result.Message ?? "-"}</td>");
                html.AppendLine("</tr>");
            }
            html.AppendLine("</table>");

            // Issues
            if (report.Issues.Any())
            {
                html.AppendLine("<h2>Issues</h2>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Check Name</th><th>Outcome</th><th>Message</th><th>Page(s)</th></tr>");
                foreach (var issue in report.Issues)
                {
                    var outcomeClass = issue.Outcome.ToString().ToLower();
                    html.AppendLine($"<tr>");
                    html.AppendLine($"<td>{issue.CheckName}</td>");
                    html.AppendLine($"<td class='{outcomeClass}'>{issue.Outcome}</td>");
                    html.AppendLine($"<td>{issue.Message}</td>");
                    html.AppendLine($"<td>{issue.PageNumbers ?? "-"}</td>");
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</table>");
            }

            // Comments
            if (report.ReviewComments.Any())
            {
                html.AppendLine("<h2>Review Comments</h2>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Date</th><th>Author</th><th>Comment</th></tr>");
                foreach (var comment in report.ReviewComments)
                {
                    html.AppendLine($"<tr>");
                    html.AppendLine($"<td>{comment.CreatedAt:yyyy-MM-dd HH:mm:ss}</td>");
                    html.AppendLine($"<td>{comment.AuthorUserName ?? comment.AuthorId}</td>");
                    html.AppendLine($"<td>{comment.Text}</td>");
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</table>");
            }

            // Summary
            if (report.ReviewSummary != null)
            {
                html.AppendLine("<h2>AI Review Summary</h2>");
                html.AppendLine($"<p>{report.ReviewSummary.SummaryText.Replace("\n", "<br>")}</p>");
                html.AppendLine($"<p><small>Generated on: {report.ReviewSummary.GeneratedAt:yyyy-MM-dd HH:mm:ss}</small></p>");
            }

            html.AppendLine($"<hr>");
            html.AppendLine($"<p><small>Report generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</small></p>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            var htmlBytes = Encoding.UTF8.GetBytes(html.ToString());
            var fileName = $"{Path.GetFileNameWithoutExtension(originalFileName)}_ReviewReport.html";
            return await Task.FromResult((htmlBytes, "text/html", fileName));
        }

        private async Task<List<ReviewCommentDto>> BuildReviewCommentsWithAuthorNamesAsync(
            List<ReviewComment> comments,
            CancellationToken cancellationToken)
        {
            var dtos = new List<ReviewCommentDto>(comments.Count);
            foreach (var c in comments)
            {
                var authorName = await _userHelper.GetUserNameAsync(c.AuthorId, cancellationToken);
                dtos.Add(new ReviewCommentDto
                {
                    Id = c.Id,
                    DocumentId = c.DocumentId,
                    AuthorId = c.AuthorId,
                    AuthorUserName = authorName,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                });
            }
            return dtos;
        }

        private int? ParsePageNumber(string? pageNumbers)
        {
            if (string.IsNullOrWhiteSpace(pageNumbers))
                return null;

            var parts = pageNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 0 && int.TryParse(parts[0], out int pageNum))
            {
                return pageNum;
            }
            return null;
        }
    }
}
