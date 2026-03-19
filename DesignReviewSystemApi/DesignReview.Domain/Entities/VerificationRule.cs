using DesignReview.Domain.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesignReview.Domain.Entities
{
    /// <summary>
    /// Admin-managed verification rule template. Used by the verification engine to run checks against extracted PDF content.
    /// </summary>
    public partial class VerificationRule
    {
        public Guid Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        /// <summary>Rule type: ProjectName, DrawingNumber, Revision, Date, Scale, or Custom.</summary>
        public string RuleType { get; set; } = "Custom";
        /// <summary>JSON array of regex patterns used to search extracted text.</summary>
        public string SearchPatterns { get; set; } = "[]";
        /// <summary>Optional regex to validate found value (e.g. scale format). If validation fails, outcome is ManualReview.</summary>
        public string? ValidationPattern { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<VerificationResult> VerificationResults { get; set; } = new HashSet<VerificationResult>();

        public class Configuration : EntityConfigurationBase<VerificationRule>
        {
            public Configuration(DbProvider dbProvider = DbProvider.SqlServer) : base(dbProvider) { }

            protected override void ConfigureEntity(EntityTypeBuilder<VerificationRule> entity)
            {
                entity.ToTable("VerificationRules");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.RuleName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.RuleType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.SearchPatterns)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.ValidationPattern)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.DisplayOrder)
                    .HasDefaultValue(0);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime2");

                entity.HasIndex(e => e.RuleType)
                    .HasDatabaseName("IX_VerificationRules_RuleType");
                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_VerificationRules_IsActive");
            }
        }
    }
}
