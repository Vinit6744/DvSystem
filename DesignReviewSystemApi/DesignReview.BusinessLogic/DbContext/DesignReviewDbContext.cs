using DesignReview.BusinessLogic.Identity;
using DesignReview.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DesignReview.BusinessLogic.DbContext
{

    public class DesignReviewDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public DesignReviewDbContext(
            DbContextOptions<DesignReviewDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DesignReviewDbContext).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReviewDocument).Assembly);
        }

        public DbSet<ReviewDocument> ReviewDocuments => Set<ReviewDocument>();
        public DbSet<DocumentPage> DocumentPages => Set<DocumentPage>();
        public DbSet<VerificationRule> VerificationRules => Set<VerificationRule>();
        public DbSet<VerificationResult> VerificationResults => Set<VerificationResult>();
        public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();
        public DbSet<ReviewSummary> ReviewSummaries => Set<ReviewSummary>();
    }
}
