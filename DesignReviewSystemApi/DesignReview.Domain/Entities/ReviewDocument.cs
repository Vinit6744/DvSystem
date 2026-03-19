using DesignReview.Domain.Configuration;
using DesignReview.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesignReview.Domain.Entities
{
    public partial class ReviewDocument
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string UploadedById { get; set; } = string.Empty;
        public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Draft;
        public string? FileStoragePath { get; set; }

        public virtual ICollection<DocumentPage> DocumentPages { get; set; } = new HashSet<DocumentPage>();
        public virtual ICollection<VerificationResult> VerificationResults { get; set; } = new HashSet<VerificationResult>();
        public virtual ICollection<ReviewComment> ReviewComments { get; set; } = new HashSet<ReviewComment>();
        public virtual ReviewSummary? ReviewSummary { get; set; }

        public class Configuration : EntityConfigurationBase<ReviewDocument>
        {
            public Configuration(DbProvider dbProvider = DbProvider.SqlServer) : base(dbProvider) { }

            protected override void ConfigureEntity(EntityTypeBuilder<ReviewDocument> entity)
            {
                entity.ToTable("ReviewDocuments");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.UploadedById)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.UploadedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.ReviewStatus)
                    .HasDefaultValue(ReviewStatus.Draft);

                entity.Property(e => e.FileStoragePath)
                    .HasMaxLength(1000);

                entity.HasIndex(e => e.UploadedById)
                    .HasDatabaseName("IX_ReviewDocuments_UploadedById");

                entity.HasIndex(e => e.ReviewStatus)
                    .HasDatabaseName("IX_ReviewDocuments_ReviewStatus");

                entity.HasIndex(e => e.UploadedAt)
                    .HasDatabaseName("IX_ReviewDocuments_UploadedAt");
            }
        }
    }
}
