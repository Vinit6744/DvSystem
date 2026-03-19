using DesignReview.Domain.Configuration;
using DesignReview.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesignReview.Domain.Entities
{
    public partial class VerificationResult
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Guid? VerificationRuleId { get; set; }
        public string CheckName { get; set; } = string.Empty;
        public VerificationOutcome Outcome { get; set; }
        public string? FoundValue { get; set; }
        public string? PageNumbers { get; set; }
        public string? Message { get; set; }

        public virtual ReviewDocument Document { get; set; } = null!;
        public virtual VerificationRule? VerificationRule { get; set; }

        public class Configuration : EntityConfigurationBase<VerificationResult>
        {
            public Configuration(DbProvider dbProvider = DbProvider.SqlServer) : base(dbProvider) { }

            protected override void ConfigureEntity(EntityTypeBuilder<VerificationResult> entity)
            {
                entity.ToTable("VerificationResults");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DocumentId)
                    .IsRequired();

                entity.Property(e => e.VerificationRuleId);

                entity.Property(e => e.CheckName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Outcome)
                    .IsRequired();

                entity.Property(e => e.FoundValue)
                    .HasMaxLength(500);

                entity.Property(e => e.PageNumbers)
                    .HasMaxLength(100);

                entity.Property(e => e.Message)
                    .HasMaxLength(1000);

                entity.HasOne(d => d.Document)
                    .WithMany(p => p.VerificationResults)
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_VerificationResults_ReviewDocument");

                entity.HasOne(d => d.VerificationRule)
                    .WithMany(p => p.VerificationResults)
                    .HasForeignKey(d => d.VerificationRuleId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_VerificationResults_VerificationRule");

                entity.HasIndex(e => e.DocumentId)
                    .HasDatabaseName("IX_VerificationResults_DocumentId");

                entity.HasIndex(e => e.VerificationRuleId)
                    .HasDatabaseName("IX_VerificationResults_VerificationRuleId");

                entity.HasIndex(e => e.CheckName)
                    .HasDatabaseName("IX_VerificationResults_CheckName");

                entity.HasIndex(e => e.Outcome)
                    .HasDatabaseName("IX_VerificationResults_Outcome");
            }
        }
    }
}
