using DesignReview.Application.Interfaces;
using DesignReview.Application.Models;
using DesignReview.BusinessLogic.DbContext;
using DesignReview.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.Text;

namespace DesignReview.BusinessLogic.Services
{
    public class ReviewSummaryService : IReviewSummaryService
    {
        private readonly DesignReviewDbContext _db;
        private readonly IVerificationService _verificationService;
        private readonly IConfiguration _configuration;
        private readonly OpenAIClient? _openAIClient;

        public ReviewSummaryService(
            DesignReviewDbContext db,
            IVerificationService verificationService,
            IConfiguration configuration)
        {
            _db = db;
            _verificationService = verificationService;
            _configuration = configuration;
            var apiKey = configuration["OpenAI:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                _openAIClient = new OpenAIClient(apiKey);
            }
        }

        public async Task<ReviewSummaryDto> GenerateSummaryAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            var document = await _db.ReviewDocuments
                .Include(d => d.DocumentPages)
                .Include(d => d.VerificationResults)
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

            if (document == null)
            {
                throw new InvalidOperationException($"Document {documentId} not found.");
            }

            // Get verification results
            var verificationResponse = await _verificationService.GetVerificationResultsAsync(documentId, cancellationToken);
            var issues = await _verificationService.GetIssuesAsync(documentId, cancellationToken);

            // Build summary text
            string summaryText;
            if (_openAIClient != null)
            {
                summaryText = await GenerateAISummaryAsync(document, verificationResponse, issues, cancellationToken);
            }
            else
            {
                summaryText = GenerateBasicSummary(document, verificationResponse, issues);
            }

            // Remove existing summary if any
            var existingSummary = await _db.ReviewSummaries
                .FirstOrDefaultAsync(s => s.DocumentId == documentId, cancellationToken);
            if (existingSummary != null)
            {
                _db.ReviewSummaries.Remove(existingSummary);
            }

            var summary = new ReviewSummary
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                SummaryText = summaryText,
                GeneratedAt = DateTime.UtcNow
            };

            _db.ReviewSummaries.Add(summary);
            await _db.SaveChangesAsync(cancellationToken);

            return new ReviewSummaryDto
            {
                Id = summary.Id,
                DocumentId = summary.DocumentId,
                SummaryText = summary.SummaryText,
                GeneratedAt = summary.GeneratedAt
            };
        }

        public async Task<ReviewSummaryDto?> GetSummaryAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            var summary = await _db.ReviewSummaries
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.DocumentId == documentId, cancellationToken);

            if (summary == null)
                return null;

            return new ReviewSummaryDto
            {
                Id = summary.Id,
                DocumentId = summary.DocumentId,
                SummaryText = summary.SummaryText,
                GeneratedAt = summary.GeneratedAt
            };
        }

        private async Task<string> GenerateAISummaryAsync(
            ReviewDocument document,
            VerificationRunResponseDto? verificationResponse,
            IReadOnlyList<IssueDto> issues,
            CancellationToken cancellationToken)
        {
            if (_openAIClient == null)
            {
                return GenerateBasicSummary(document, verificationResponse, issues);
            }

            try
            {
                var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";
                var chatClient = _openAIClient.GetChatClient(model);

                var extractedContent = string.Join("\n--- Page ---\n",
                    document.DocumentPages.OrderBy(p => p.PageNumber).Select(p => $"Page {p.PageNumber}:\n{p.ExtractedText}"));

                var verificationSummary = verificationResponse != null
                    ? string.Join("\n", verificationResponse.Results.Select(r =>
                        $"- {r.CheckName}: {r.Outcome}" + (r.FoundValue != null ? $" (Found: {r.FoundValue})" : "") +
                        (r.PageNumbers != null ? $" [Page(s) {r.PageNumbers}]" : "")))
                    : "No verification results.";

                var issuesSummary = issues.Any()
                    ? string.Join("\n", issues.Select(i => $"- {i.CheckName}: {i.Message} (Page {i.FirstPageNumber})"))
                    : "No issues identified.";

                var systemPrompt = "You are an expert design review assistant. Given extracted text from a design/drawing PDF and verification check results, produce a concise review summary. Include: (1) A short summary of the document review outcome; (2) Highlight any critical missing or inconsistent information; (3) Suggested next action (e.g., request revision info, confirm scale, approve). Keep the response focused and under 300 words. Use plain text, no markdown headers.";

                var userContent = new StringBuilder();
                userContent.AppendLine("Document: " + document.FileName);
                userContent.AppendLine("Current status: " + document.ReviewStatus);
                userContent.AppendLine();
                userContent.AppendLine("Verification results:");
                userContent.AppendLine(verificationSummary);
                userContent.AppendLine();
                userContent.AppendLine("Issues:");
                userContent.AppendLine(issuesSummary);
                userContent.AppendLine();
                userContent.AppendLine("Extracted text (excerpt, first 3000 chars):");
                userContent.AppendLine(extractedContent.Length > 3000 ? extractedContent[..3000] + "..." : extractedContent);

                var messages = new ChatMessage[]
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userContent.ToString())
                };

                var options = new ChatCompletionOptions();
                var completion = await chatClient.CompleteChatAsync(messages, options, cancellationToken);

                if (completion?.Value?.Content != null)
                {
                    var text = GetContentText(completion.Value.Content);
                    if (!string.IsNullOrWhiteSpace(text))
                        return text.Trim();
                }
            }
            catch
            {
                // Fall back to basic summary on any API or parsing error
            }

            return GenerateBasicSummary(document, verificationResponse, issues);
        }

        private static string? GetContentText(ChatMessageContent? content)
        {
            if (content == null) return null;
            try
            {
                foreach (var part in content)
                {
                    if (part.Kind == ChatMessageContentPartKind.Text && part.Text != null)
                        return part.Text;
                }
            }
            catch
            {
                // Content may not be enumerable in some SDK versions
            }
            return null;
        }

        private string GenerateBasicSummary(
            ReviewDocument document,
            VerificationRunResponseDto? verificationResponse,
            IReadOnlyList<IssueDto> issues)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Review Summary for {document.FileName}");
            sb.AppendLine($"Status: {document.ReviewStatus}");
            sb.AppendLine();

            if (verificationResponse != null)
            {
                var passCount = verificationResponse.Results.Count(r => r.Outcome == Domain.Enums.VerificationOutcome.Pass);
                var failCount = verificationResponse.Results.Count(r => r.Outcome == Domain.Enums.VerificationOutcome.Fail);
                var manualReviewCount = verificationResponse.Results.Count(r => r.Outcome == Domain.Enums.VerificationOutcome.ManualReview);

                sb.AppendLine($"Verification Results: {passCount} passed, {failCount} failed, {manualReviewCount} require manual review.");
            }

            if (issues.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Issues Identified:");
                foreach (var issue in issues)
                {
                    sb.AppendLine($"- {issue.CheckName}: {issue.Message} (Page {issue.FirstPageNumber})");
                }
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("No critical issues identified.");
            }

            sb.AppendLine();
            sb.AppendLine("Next Steps:");
            if (issues.Any(i => i.Outcome == Domain.Enums.VerificationOutcome.Fail))
            {
                sb.AppendLine("- Address failed verification checks before approval.");
            }
            if (issues.Any(i => i.Outcome == Domain.Enums.VerificationOutcome.ManualReview))
            {
                sb.AppendLine("- Review items marked for manual verification.");
            }
            if (!issues.Any())
            {
                sb.AppendLine("- Document appears ready for approval.");
            }

            return sb.ToString();
        }
    }
}
