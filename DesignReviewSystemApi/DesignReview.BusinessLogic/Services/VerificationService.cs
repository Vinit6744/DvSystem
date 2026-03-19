using DesignReview.Application.Interfaces;
using DesignReview.Application.Models;
using DesignReview.BusinessLogic.DbContext;
using DesignReview.Domain.Entities;
using DesignReview.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DesignReview.BusinessLogic.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly DesignReviewDbContext _db;

        public VerificationService(DesignReviewDbContext db)
        {
            _db = db;
        }

        public async Task<VerificationRunResponseDto> RunVerificationAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            var document = await _db.ReviewDocuments
                .Include(d => d.DocumentPages)
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

            if (document == null)
            {
                throw new InvalidOperationException($"Document {documentId} not found.");
            }

            // Remove existing verification results
            var existingResults = await _db.VerificationResults
                .Where(v => v.DocumentId == documentId)
                .ToListAsync(cancellationToken);
            if (existingResults.Any())
            {
                _db.VerificationResults.RemoveRange(existingResults);
            }

            var allText = string.Join(" ", document.DocumentPages.OrderBy(p => p.PageNumber).Select(p => p.ExtractedText));
            var pagesList = document.DocumentPages.OrderBy(p => p.PageNumber).ToList();
            var documentIdVal = document.Id;
            var results = new List<VerificationResult>();

            // Use Admin-configured rules if any active rules exist; otherwise use built-in MVP checks
            var activeRules = await _db.VerificationRules
                .Where(r => r.IsActive)
                .OrderBy(r => r.DisplayOrder)
                .ThenBy(r => r.RuleName)
                .ToListAsync(cancellationToken);

            if (activeRules.Any())
            {
                foreach (var rule in activeRules)
                {
                    var result = RunRuleCheck(rule, pagesList, allText, documentIdVal);
                    results.Add(result);
                }
            }
            else
            {
                // Fallback: built-in MVP checks (Project Name, Drawing Number, Revision, Date, Scale)
                results.Add(CheckProjectName(pagesList, allText));
                results.Add(CheckDrawingNumber(pagesList, allText));
                results.Add(CheckRevision(pagesList, allText));
                results.Add(CheckDate(pagesList, allText));
                results.Add(CheckScale(pagesList, allText));
            }

            _db.VerificationResults.AddRange(results);
            await _db.SaveChangesAsync(cancellationToken);

            return await GetVerificationResultsAsync(documentId, cancellationToken) ?? new VerificationRunResponseDto
            {
                DocumentId = documentId,
                Results = new List<VerificationResultDto>(),
                Issues = new List<IssueDto>()
            };
        }

        public async Task<VerificationRunResponseDto?> GetVerificationResultsAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            var results = await _db.VerificationResults
                .Where(v => v.DocumentId == documentId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (!results.Any())
                return null;

            var resultDtos = results.Select(r => new VerificationResultDto
            {
                Id = r.Id,
                DocumentId = r.DocumentId,
                CheckName = r.CheckName,
                Outcome = r.Outcome,
                FoundValue = r.FoundValue,
                PageNumbers = r.PageNumbers,
                Message = r.Message
            }).ToList();

            var issues = await GetIssuesAsync(documentId, cancellationToken);

            return new VerificationRunResponseDto
            {
                DocumentId = documentId,
                Results = resultDtos,
                Issues = issues.ToList()
            };
        }

        public async Task<IReadOnlyList<IssueDto>> GetIssuesAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            var results = await _db.VerificationResults
                .Where(v => v.DocumentId == documentId && v.Outcome != VerificationOutcome.Pass)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return results.Select(r =>
            {
                var pageNumbers = ParsePageNumbers(r.PageNumbers);
                return new IssueDto
                {
                    VerificationResultId = r.Id,
                    CheckName = r.CheckName,
                    Outcome = r.Outcome,
                    Message = r.Message ?? $"{r.CheckName} check failed",
                    PageNumbers = r.PageNumbers,
                    FirstPageNumber = pageNumbers.FirstOrDefault()
                };
            }).ToList();
        }

        /// <summary>
        /// Runs a single Admin-configured rule against extracted text. Parses SearchPatterns (JSON array of regex), optionally validates with ValidationPattern.
        /// </summary>
        private VerificationResult RunRuleCheck(VerificationRule rule, List<DocumentPage> pages, string allText, Guid documentId)
        {
            string[]? patterns = null;
            try
            {
                patterns = JsonSerializer.Deserialize<string[]>(rule.SearchPatterns);
            }
            catch
            {
                // Invalid JSON: treat as single pattern or empty
            }
            if (patterns == null || patterns.Length == 0)
            {
                return new VerificationResult
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documentId,
                    VerificationRuleId = rule.Id,
                    CheckName = rule.RuleName,
                    Outcome = VerificationOutcome.Fail,
                    Message = $"{rule.RuleName}: No search patterns configured."
                };
            }

            foreach (var pattern in patterns)
            {
                if (string.IsNullOrWhiteSpace(pattern)) continue;
                var match = Regex.Match(allText, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var pageNum = FindPageContainingText(pages, match.Value);
                    var foundValue = match.Groups.Count > 1 ? match.Groups[1].Value.Trim() : match.Value.Trim();
                    var outcome = VerificationOutcome.Pass;
                    var message = $"{rule.RuleName} found: {foundValue}";
                    if (!string.IsNullOrWhiteSpace(rule.ValidationPattern))
                    {
                        if (!Regex.IsMatch(foundValue, rule.ValidationPattern))
                        {
                            outcome = VerificationOutcome.ManualReview;
                            message = $"{rule.RuleName} found but format may be incorrect: {foundValue}";
                        }
                    }
                    return new VerificationResult
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = documentId,
                        VerificationRuleId = rule.Id,
                        CheckName = rule.RuleName,
                        Outcome = outcome,
                        FoundValue = foundValue,
                        PageNumbers = pageNum.ToString(),
                        Message = message
                    };
                }
            }

            return new VerificationResult
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                VerificationRuleId = rule.Id,
                CheckName = rule.RuleName,
                Outcome = VerificationOutcome.Fail,
                Message = $"{rule.RuleName} not found"
            };
        }

        private VerificationResult CheckProjectName(List<DocumentPage> pages, string allText)
        {
            // Look for common project name patterns
            var patterns = new[]
            {
                @"project\s*name\s*:?\s*([A-Z][A-Za-z0-9\s\-_]+)",
                @"project\s*:?\s*([A-Z][A-Za-z0-9\s\-_]+)",
                @"proj\s*:?\s*([A-Z][A-Za-z0-9\s\-_]+)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(allText, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var pageNum = FindPageContainingText(pages, match.Value);
                    return new VerificationResult
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = pages.First().DocumentId,
                        CheckName = "Project Name",
                        Outcome = VerificationOutcome.Pass,
                        FoundValue = match.Groups[1].Value.Trim(),
                        PageNumbers = pageNum.ToString(),
                        Message = $"Project name found: {match.Groups[1].Value.Trim()}"
                    };
                }
            }

            return new VerificationResult
            {
                Id = Guid.NewGuid(),
                DocumentId = pages.First().DocumentId,
                CheckName = "Project Name",
                Outcome = VerificationOutcome.Fail,
                Message = "Project name not found"
            };
        }

        private VerificationResult CheckDrawingNumber(List<DocumentPage> pages, string allText)
        {
            var patterns = new[]
            {
                @"drawing\s*(?:number|no|#)?\s*:?\s*([A-Z0-9\-_/]+)",
                @"dwg\s*(?:no|#)?\s*:?\s*([A-Z0-9\-_/]+)",
                @"drawing\s*:?\s*([A-Z0-9\-_/]+)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(allText, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var pageNum = FindPageContainingText(pages, match.Value);
                    return new VerificationResult
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = pages.First().DocumentId,
                        CheckName = "Drawing Number",
                        Outcome = VerificationOutcome.Pass,
                        FoundValue = match.Groups[1].Value.Trim(),
                        PageNumbers = pageNum.ToString(),
                        Message = $"Drawing number found: {match.Groups[1].Value.Trim()}"
                    };
                }
            }

            return new VerificationResult
            {
                Id = Guid.NewGuid(),
                DocumentId = pages.First().DocumentId,
                CheckName = "Drawing Number",
                Outcome = VerificationOutcome.Fail,
                Message = "Drawing number not found"
            };
        }

        private VerificationResult CheckRevision(List<DocumentPage> pages, string allText)
        {
            var patterns = new[]
            {
                @"revision\s*(?:number|no|#)?\s*:?\s*([A-Z0-9]+)",
                @"rev\s*(?:no|#)?\s*:?\s*([A-Z0-9]+)",
                @"revision\s*:?\s*([A-Z0-9]+)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(allText, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var pageNum = FindPageContainingText(pages, match.Value);
                    return new VerificationResult
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = pages.First().DocumentId,
                        CheckName = "Revision",
                        Outcome = VerificationOutcome.Pass,
                        FoundValue = match.Groups[1].Value.Trim(),
                        PageNumbers = pageNum.ToString(),
                        Message = $"Revision found: {match.Groups[1].Value.Trim()}"
                    };
                }
            }

            return new VerificationResult
            {
                Id = Guid.NewGuid(),
                DocumentId = pages.First().DocumentId,
                CheckName = "Revision",
                Outcome = VerificationOutcome.Fail,
                Message = "Revision not found"
            };
        }

        private VerificationResult CheckDate(List<DocumentPage> pages, string allText)
        {
            // Look for date patterns
            var datePatterns = new[]
            {
                @"date\s*:?\s*(\d{1,2}[/\-]\d{1,2}[/\-]\d{2,4})",
                @"date\s*:?\s*(\d{1,2}\s+(?:jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)[a-z]*\s+\d{2,4})",
                @"dated\s*:?\s*(\d{1,2}[/\-]\d{1,2}[/\-]\d{2,4})"
            };

            foreach (var pattern in datePatterns)
            {
                var match = Regex.Match(allText, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var pageNum = FindPageContainingText(pages, match.Value);
                    return new VerificationResult
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = pages.First().DocumentId,
                        CheckName = "Date",
                        Outcome = VerificationOutcome.Pass,
                        FoundValue = match.Groups[1].Value.Trim(),
                        PageNumbers = pageNum.ToString(),
                        Message = $"Date found: {match.Groups[1].Value.Trim()}"
                    };
                }
            }

            return new VerificationResult
            {
                Id = Guid.NewGuid(),
                DocumentId = pages.First().DocumentId,
                CheckName = "Date",
                Outcome = VerificationOutcome.Fail,
                Message = "Date not found"
            };
        }

        private VerificationResult CheckScale(List<DocumentPage> pages, string allText)
        {
            // Look for scale patterns like 1:100, 1/100, scale 1:100, etc.
            var scalePatterns = new[]
            {
                @"scale\s*:?\s*(1\s*[:/]\s*\d+)",
                @"scale\s*:?\s*(\d+\s*[:/]\s*\d+)",
                @"(1\s*[:/]\s*\d+)\s*scale"
            };

            foreach (var pattern in scalePatterns)
            {
                var match = Regex.Match(allText, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var pageNum = FindPageContainingText(pages, match.Value);
                    var scaleValue = match.Groups[1].Value.Trim().Replace(" ", "");
                    
                    // Validate scale format (should be like 1:100 or 1/100)
                    if (Regex.IsMatch(scaleValue, @"^\d+[:/]\d+$"))
                    {
                        return new VerificationResult
                        {
                            Id = Guid.NewGuid(),
                            DocumentId = pages.First().DocumentId,
                            CheckName = "Scale",
                            Outcome = VerificationOutcome.Pass,
                            FoundValue = scaleValue,
                            PageNumbers = pageNum.ToString(),
                            Message = $"Scale found: {scaleValue}"
                        };
                    }
                    else
                    {
                        return new VerificationResult
                        {
                            Id = Guid.NewGuid(),
                            DocumentId = pages.First().DocumentId,
                            CheckName = "Scale",
                            Outcome = VerificationOutcome.ManualReview,
                            FoundValue = scaleValue,
                            PageNumbers = pageNum.ToString(),
                            Message = $"Scale found but format may be incorrect: {scaleValue}"
                        };
                    }
                }
            }

            return new VerificationResult
            {
                Id = Guid.NewGuid(),
                DocumentId = pages.First().DocumentId,
                CheckName = "Scale",
                Outcome = VerificationOutcome.Fail,
                Message = "Scale not found"
            };
        }

        private int FindPageContainingText(List<DocumentPage> pages, string searchText)
        {
            foreach (var page in pages.OrderBy(p => p.PageNumber))
            {
                if (page.ExtractedText.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    return page.PageNumber;
                }
            }
            return pages.FirstOrDefault()?.PageNumber ?? 1;
        }

        private List<int> ParsePageNumbers(string? pageNumbers)
        {
            if (string.IsNullOrWhiteSpace(pageNumbers))
                return new List<int>();

            var pages = new List<int>();
            var parts = pageNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                if (int.TryParse(part, out int pageNum))
                {
                    pages.Add(pageNum);
                }
            }
            return pages;
        }
    }
}
