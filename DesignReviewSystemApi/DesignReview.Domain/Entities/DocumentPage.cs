using DesignReview.Domain.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesignReview.Domain.Entities
{
    public partial class DocumentPage
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public int PageNumber { get; set; }
        public string ExtractedText { get; set; } = string.Empty;

        public virtual ReviewDocument Document { get; set; } = null!;

        public class Configuration : EntityConfigurationBase<DocumentPage>
        {
            public Configuration(DbProvider dbProvider = DbProvider.SqlServer) : base(dbProvider) { }

            protected override void ConfigureEntity(EntityTypeBuilder<DocumentPage> entity)
            {
                entity.ToTable("DocumentPages");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DocumentId)
                    .IsRequired();

                entity.Property(e => e.PageNumber)
                    .IsRequired();

                entity.Property(e => e.ExtractedText)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.HasOne(d => d.Document)
                    .WithMany(p => p.DocumentPages)
                    .HasForeignKey(d => d.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DocumentPages_ReviewDocument");

                entity.HasIndex(e => e.DocumentId)
                    .HasDatabaseName("IX_DocumentPages_DocumentId");

                entity.HasIndex(e => new { e.DocumentId, e.PageNumber })
                    .IsUnique()
                    .HasDatabaseName("IX_DocumentPages_DocumentId_PageNumber");
            }
        }
    }
}
