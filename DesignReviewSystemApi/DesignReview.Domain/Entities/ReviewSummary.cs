using DesignReview.Domain.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesignReview.Domain.Entities
{
    public partial class ReviewSummary
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }

        public virtual ReviewDocument Document { get; set; } = null!;

        public class Configuration : EntityConfigurationBase<ReviewSummary>
        {
            public Configuration(DbProvider dbProvider = DbProvider.SqlServer) : base(dbProvider) { }

            protected override void ConfigureEntity(EntityTypeBuilder<ReviewSummary> entity)
            {
                entity.ToTable("ReviewSummaries");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DocumentId)
                    .IsRequired();

                entity.Property(e => e.SummaryText)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.GeneratedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("(getutcdate())");

                entity.HasOne(d => d.Document)
                    .WithOne(p => p.ReviewSummary)
                    .HasForeignKey<ReviewSummary>(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ReviewSummaries_ReviewDocument");

                entity.HasIndex(e => e.DocumentId)
                    .IsUnique()
                    .HasDatabaseName("IX_ReviewSummaries_DocumentId");

                entity.HasIndex(e => e.GeneratedAt)
                    .HasDatabaseName("IX_ReviewSummaries_GeneratedAt");
            }
        }
    }
}
