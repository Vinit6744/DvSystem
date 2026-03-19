using DesignReview.Domain.Entities;
using DesignReview.Domain.Enums;
using DesignReview.BusinessLogic.DbContext;
using DesignReview.BusinessLogic.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace DesignReview.BusinessLogic.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DesignReviewDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
        {
            var roleName = role.ToRoleName();
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        var adminId = await EnsureUserAsync(userManager, configuration.GetSection("SeedAdmin"), UserRole.Admin);
        var reviewerId = await EnsureUserAsync(userManager, configuration.GetSection("SeedReviewer"), UserRole.Reviewer);

        // Seed default verification rules (Admin-managed) if none exist — MVP: Project Name, Drawing Number, Revision, Date, Scale
        if (!await db.VerificationRules.AnyAsync())
        {
            var defaultRules = new[]
            {
                new VerificationRule { Id = Guid.NewGuid(), RuleName = "Project Name", Description = "Project name must be present", RuleType = "ProjectName", SearchPatterns = "[\"project\\\\s*name\\\\s*:?\\\\s*([A-Z][A-Za-z0-9\\\\s\\\\-_]+)\",\"project\\\\s*:?\\\\s*([A-Z][A-Za-z0-9\\\\s\\\\-_]+)\",\"proj\\\\s*:?\\\\s*([A-Z][A-Za-z0-9\\\\s\\\\-_]+)\"]", IsActive = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow },
                new VerificationRule { Id = Guid.NewGuid(), RuleName = "Drawing Number", Description = "Drawing number must be present", RuleType = "DrawingNumber", SearchPatterns = "[\"drawing\\\\s*(?:number|no|#)?\\\\s*:?\\\\s*([A-Z0-9\\\\-_/]+)\",\"dwg\\\\s*(?:no|#)?\\\\s*:?\\\\s*([A-Z0-9\\\\-_/]+)\"]", IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
                new VerificationRule { Id = Guid.NewGuid(), RuleName = "Revision", Description = "Revision must be present", RuleType = "Revision", SearchPatterns = "[\"revision\\\\s*(?:number|no|#)?\\\\s*:?\\\\s*([A-Z0-9]+)\",\"rev\\\\s*(?:no|#)?\\\\s*:?\\\\s*([A-Z0-9]+)\"]", IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.UtcNow },
                new VerificationRule { Id = Guid.NewGuid(), RuleName = "Date", Description = "Date must be present", RuleType = "Date", SearchPatterns = "[\"date\\\\s*:?\\\\s*(\\\\d{1,2}[/\\\\-]\\\\d{1,2}[/\\\\-]\\\\d{2,4})\",\"dated\\\\s*:?\\\\s*(\\\\d{1,2}[/\\\\-]\\\\d{1,2}[/\\\\-]\\\\d{2,4})\"]", IsActive = true, DisplayOrder = 3, CreatedAt = DateTime.UtcNow },
                new VerificationRule { Id = Guid.NewGuid(), RuleName = "Scale", Description = "Scale must be present and follow expected format", RuleType = "Scale", SearchPatterns = "[\"scale\\\\s*:?\\\\s*(1\\\\s*[:/]\\\\s*\\\\d+)\",\"scale\\\\s*:?\\\\s*(\\\\d+\\\\s*[:/]\\\\s*\\\\d+)\"]", ValidationPattern = "^\\\\d+[:/]\\\\d+$", IsActive = true, DisplayOrder = 4, CreatedAt = DateTime.UtcNow }
            };
            foreach (var r in defaultRules)
                db.VerificationRules.Add(r);
            await db.SaveChangesAsync();
        }

        // Sample document: use Reviewer so document workflow is visible to Reviewer (Admin only manages rules)
        if (reviewerId != null && !await db.ReviewDocuments.AnyAsync())
        {
            var docId = Guid.NewGuid();
            var doc = new ReviewDocument
            {
                Id = docId,
                FileName = "Sample-Design-Drawing-001.pdf",
                UploadedAt = DateTime.UtcNow.AddDays(-2),
                UploadedById = reviewerId,
                ReviewStatus = ReviewStatus.InReview,
                FileStoragePath = "/uploads/sample-drawing.pdf"
            };
            db.ReviewDocuments.Add(doc);

            for (int i = 1; i <= 3; i++)
            {
                db.DocumentPages.Add(new DocumentPage
                {
                    Id = Guid.NewGuid(),
                    DocumentId = docId,
                    PageNumber = i,
                    ExtractedText = i == 1
                        ? "PROJECT: North Building Phase 2\nDRAWING NO: DR-2024-001\nREV: B\nDATE: 2024-01-15\nSCALE: 1:100\n\nTitle block and general layout."
                        : i == 2
                            ? "Floor plan - Level 1\nAdditional details and dimensions."
                            : "Section A-A\nDetail view."
                });
            }

            var verificationChecks = new (string CheckName, VerificationOutcome Outcome, string FoundValue, string PageNumbers, string? Message)[]
            {
                ("Project Name", VerificationOutcome.Pass, "North Building Phase 2", "1", null),
                ("Drawing Number", VerificationOutcome.Pass, "DR-2024-001", "1", null),
                ("Revision", VerificationOutcome.Pass, "B", "1", null),
                ("Date", VerificationOutcome.Pass, "2024-01-15", "1", null),
                ("Scale", VerificationOutcome.Pass, "1:100", "1", null)
            };
            foreach (var row in verificationChecks)
            {
                db.VerificationResults.Add(new VerificationResult
                {
                    Id = Guid.NewGuid(),
                    DocumentId = docId,
                    CheckName = row.CheckName,
                    Outcome = row.Outcome,
                    FoundValue = row.FoundValue,
                    PageNumbers = row.PageNumbers,
                    Message = row.Message
                });
            }

            db.ReviewComments.Add(new ReviewComment
            {
                Id = Guid.NewGuid(),
                DocumentId = docId,
                AuthorId = reviewerId,
                Text = "Sample review comment: Please confirm scale on page 1 before final approval.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });

            db.ReviewSummaries.Add(new ReviewSummary
            {
                Id = Guid.NewGuid(),
                DocumentId = docId,
                SummaryText = "Sample AI summary: Document contains required fields (Project Name, Drawing Number, Revision, Date, Scale). All verification checks passed. Suggested next action: Confirm scale format with client if needed.",
                GeneratedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }

    private static async Task<string?> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfigurationSection section,
        UserRole role)
    {
        if (!section.Exists()) return null;
        var userName = section["UserName"];
        var email = section["Email"];
        var password = section["Password"];
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)) return null;

        var existing = await userManager.FindByNameAsync(userName);
        if (existing != null) return existing.Id;

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email ?? userName + "@designreview.local",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded) return null;
        var roles = await userManager.GetRolesAsync(user);
        if (roles.Count == 0)
            await userManager.AddToRoleAsync(user, role.ToRoleName());
        return user.Id;
    }
}
