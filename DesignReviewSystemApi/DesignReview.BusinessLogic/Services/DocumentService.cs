using DesignReview.Application.Interfaces;
using DesignReview.Application.Models;
using DesignReview.Application.Options;
using DesignReview.BusinessLogic.DbContext;
using DesignReview.Domain.Entities;
using DesignReview.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DesignReview.BusinessLogic.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly DesignReviewDbContext _db;
        private readonly IUserHelperService _userHelper;
        private readonly FileStorageOptions _fileStorage;

        public DocumentService(
            DesignReviewDbContext db,
            IUserHelperService userHelper,
            IOptions<FileStorageOptions> fileStorage)
        {
            _db = db;
            _userHelper = userHelper;
            _fileStorage = fileStorage.Value;
        }

        public async Task<UploadDocumentDto> UploadAsync(Stream fileStream, string fileName, string uploadedById, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("File must be a PDF.", nameof(fileName));
            }

            var docId = Guid.NewGuid();
            var safeName = $"{docId}{Path.GetExtension(fileName)}";
            var basePath = _fileStorage.BasePath;
            Directory.CreateDirectory(basePath);
            var fullPath = Path.Combine(basePath, safeName);

            await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                await fileStream.CopyToAsync(fs, cancellationToken);

            var doc = new ReviewDocument
            {
                Id = docId,
                FileName = fileName,
                UploadedAt = DateTime.UtcNow,
                UploadedById = uploadedById,
                ReviewStatus = ReviewStatus.Draft,
                FileStoragePath = fullPath
            };
            _db.ReviewDocuments.Add(doc);
            await _db.SaveChangesAsync(cancellationToken);

            return new UploadDocumentDto
            {
                Id = doc.Id,
                FileName = doc.FileName,
                UploadedAt = doc.UploadedAt,
                UploadedById = doc.UploadedById,
                ReviewStatus = doc.ReviewStatus,
                FileStoragePath = doc.FileStoragePath
            };
        }

        public async Task<DocumentDetailDto?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            var doc = await _db.ReviewDocuments
                .Include(d => d.DocumentPages)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
            if (doc == null)
                return null;

            var userName = await _userHelper.GetUserNameAsync(doc.UploadedById, cancellationToken);
            var metadata = MapToMetadataDto(doc, userName);
            var pages = doc.DocumentPages.OrderBy(p => p.PageNumber).Select(p => new DocumentPageDto
            {
                Id = p.Id,
                DocumentId = p.DocumentId,
                PageNumber = p.PageNumber,
                ExtractedText = p.ExtractedText
            }).ToList();

            return new DocumentDetailDto
            {
                Metadata = metadata,
                Pages = pages
            };
        }

        public async Task<IReadOnlyList<DocumentMetadataDto>> GetListAsync(string? uploadedById = null, CancellationToken cancellationToken = default)
        {
            var query = _db.ReviewDocuments.AsNoTracking();
            if (!string.IsNullOrEmpty(uploadedById))
                query = query.Where(d => d.UploadedById == uploadedById);

            var list = await query.OrderByDescending(d => d.UploadedAt).ToListAsync(cancellationToken);
            var dtos = new List<DocumentMetadataDto>(list.Count);
            foreach (var doc in list)
            {
                var userName = await _userHelper.GetUserNameAsync(doc.UploadedById, cancellationToken);
                dtos.Add(MapToMetadataDto(doc, userName));
            }
            return dtos;
        }

        public async Task<DocumentPageDto?> GetPageAsync(Guid documentId, int pageNumber, CancellationToken cancellationToken = default)
        {
            var page = await _db.DocumentPages
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.DocumentId == documentId && p.PageNumber == pageNumber, cancellationToken);
            if (page == null)
                return null;

            return new DocumentPageDto
            {
                Id = page.Id,
                DocumentId = page.DocumentId,
                PageNumber = page.PageNumber,
                ExtractedText = page.ExtractedText
            };
        }

        public async Task<bool> UpdateStatusAsync(Guid documentId, ReviewStatus status, CancellationToken cancellationToken = default)
        {
            var doc = await _db.ReviewDocuments.FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
            if (doc == null)
                return false;
            doc.ReviewStatus = status;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            var doc = await _db.ReviewDocuments
                .Include(d => d.DocumentPages)
                .Include(d => d.VerificationResults)
                .Include(d => d.ReviewComments)
                .Include(d => d.ReviewSummary)
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
            if (doc == null)
                return false;

            if (!string.IsNullOrEmpty(doc.FileStoragePath) && File.Exists(doc.FileStoragePath))
            {
                try
                {
                    File.Delete(doc.FileStoragePath);
                }
                catch
                {
                    // Log but continue with DB delete
                }
            }

            _db.ReviewDocuments.Remove(doc);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<(Stream Stream, string FileName)?> GetFileAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            var doc = await _db.ReviewDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
            if (doc == null || string.IsNullOrEmpty(doc.FileStoragePath))
                return null;
            if (!File.Exists(doc.FileStoragePath))
                return null;
            var stream = new FileStream(doc.FileStoragePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (stream, doc.FileName);
        }

        public long? GetMaxUploadSizeBytes() => _fileStorage.MaxFileSizeBytes;

        private static DocumentMetadataDto MapToMetadataDto(ReviewDocument doc, string? uploadedByUserName) => new()
        {
            Id = doc.Id,
            FileName = doc.FileName,
            UploadedAt = doc.UploadedAt,
            UploadedById = doc.UploadedById,
            UploadedByUserName = uploadedByUserName,
            ReviewStatus = doc.ReviewStatus
        };
    }
}
