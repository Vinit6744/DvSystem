using DesignReview.Domain.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesignReview.Domain.Entities
{
    public partial class ReviewComment
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public virtual ReviewDocument Document { get; set; } = null!;

        public class Configuration : EntityConfigurationBase<ReviewComment>
        {
            public Configuration(DbProvider dbProvider = DbProvider.SqlServer) : base(dbProvider) { }

            protected override void ConfigureEntity(EntityTypeBuilder<ReviewComment> entity)
            {
                entity.ToTable("ReviewComments");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DocumentId)
                    .IsRequired();

                entity.Property(e => e.AuthorId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("(getutcdate())");

                entity.HasOne(d => d.Document)
                    .WithMany(p => p.ReviewComments)
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ReviewComments_ReviewDocument");

                entity.HasIndex(e => e.DocumentId)
                    .HasDatabaseName("IX_ReviewComments_DocumentId");

                entity.HasIndex(e => e.AuthorId)
                    .HasDatabaseName("IX_ReviewComments_AuthorId");

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("IX_ReviewComments_CreatedAt");
            }
        }
    }
}
