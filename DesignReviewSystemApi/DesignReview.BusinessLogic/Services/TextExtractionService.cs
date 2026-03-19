using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using DesignReview.Application.Interfaces;
using DesignReview.Application.Options;
using DesignReview.BusinessLogic.DbContext;
using DesignReview.Domain.Entities;
using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tesseract;

namespace DesignReview.BusinessLogic.Services
{
    public class TextExtractionService : ITextExtractionService
    {
        private readonly DesignReviewDbContext _db;
        private readonly OcrOptions _ocrOptions;

        public TextExtractionService(
            DesignReviewDbContext db,
            IOptions<OcrOptions> ocrOptions)
        {
            _db = db;
            _ocrOptions = ocrOptions?.Value ?? new OcrOptions();
        }

        public async Task ExtractAndStoreAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            var document = await _db.ReviewDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

            if (document == null || string.IsNullOrEmpty(document.FileStoragePath))
                throw new InvalidOperationException($"Document {documentId} not found or file path is missing.");

            if (!File.Exists(document.FileStoragePath))
                throw new FileNotFoundException($"PDF file not found at: {document.FileStoragePath}");

            var existingPages = await _db.DocumentPages
                .Where(p => p.DocumentId == documentId)
                .ToListAsync(cancellationToken);
            if (existingPages.Any())
                _db.DocumentPages.RemoveRange(existingPages);

            var pages = new List<DocumentPage>();
            var pdfPath = document.FileStoragePath;
                var threshold = _ocrOptions.Enabled ? _ocrOptions.MinTextLengthThreshold : int.MaxValue;

                using (var reader = new PdfReader(pdfPath))
                using (var pdfDoc = new PdfDocument(reader))
                {
                    int pageCount = pdfDoc.GetNumberOfPages();

                    for (int pageNum = 1; pageNum <= pageCount; pageNum++)
                {
                    var strategy = new SimpleTextExtractionStrategy();
                    var pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pageNum), strategy);
                    pageText = pageText?.Trim() ?? string.Empty;

                    if (pageText.Length < threshold && _ocrOptions.Enabled && OperatingSystem.IsWindows())
                    {
                        var ocrText = await RunOcrOnPageAsync(pdfPath, pageNum - 1, cancellationToken);
                        if (!string.IsNullOrWhiteSpace(ocrText))
                            pageText = ocrText.Trim();
                    }

                    pages.Add(new DocumentPage
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = documentId,
                        PageNumber = pageNum,
                        ExtractedText = pageText
                    });
                }
            }

            _db.DocumentPages.AddRange(pages);
            await _db.SaveChangesAsync(cancellationToken);
        }

        [SupportedOSPlatform("windows")]
        private async Task<string> RunOcrOnPageAsync(string pdfPath, int pageIndex, CancellationToken cancellationToken)
        {
            string? tempImagePath = null;
            try
            {
                var tessDataPath = ResolveTessDataPath();
                if (string.IsNullOrEmpty(tessDataPath) || !Directory.Exists(tessDataPath))
                    return string.Empty;

                var dimensions = new PageDimensions(_ocrOptions.RenderWidth, _ocrOptions.RenderHeight);
                var docLib = DocLib.Instance;
                using var docReader = docLib.GetDocReader(pdfPath, dimensions);
                if (pageIndex >= docReader.GetPageCount())
                    return string.Empty;

                using var pageReader = docReader.GetPageReader(pageIndex);
                var rawBytes = pageReader.GetImage();
                var width = pageReader.GetPageWidth();
                var height = pageReader.GetPageHeight();
                if (rawBytes == null || rawBytes.Length == 0 || width <= 0 || height <= 0)
                    return string.Empty;

                tempImagePath = Path.Combine(Path.GetTempPath(), $"designreview_ocr_{Guid.NewGuid():N}.png");
                await Task.Run(() =>
                {
                    using var bmp = CreateBitmapFromBgra(rawBytes, width, height);
                    bmp?.Save(tempImagePath, System.Drawing.Imaging.ImageFormat.Png);
                }, cancellationToken);

                if (!File.Exists(tempImagePath))
                    return string.Empty;

                return await Task.Run(() =>
                {
                    try
                    {
                        using var engine = new TesseractEngine(tessDataPath, _ocrOptions.Language, EngineMode.Default);
                        using var pix = Pix.LoadFromFile(tempImagePath);
                        using var page = engine.Process(pix);
                        return page.GetText() ?? string.Empty;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }, cancellationToken);
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                if (tempImagePath != null && File.Exists(tempImagePath))
                {
                    try { File.Delete(tempImagePath); } catch { /* ignore */ }
                }
            }
        }

        [SupportedOSPlatform("windows")]
        private static Bitmap? CreateBitmapFromBgra(byte[] rawBytes, int width, int height)
        {
            if (rawBytes.Length < width * height * 4)
                return null;
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            try
            {
                Marshal.Copy(rawBytes, 0, bmpData.Scan0, Math.Min(rawBytes.Length, bmpData.Stride * height));
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        private string ResolveTessDataPath()
        {
            var path = _ocrOptions.TessDataPath?.Trim();
            if (string.IsNullOrEmpty(path))
                path = "tessdata";
            if (Path.IsPathRooted(path) && Directory.Exists(path))
                return path;
            var baseDir = AppContext.BaseDirectory;
            var combined = Path.Combine(baseDir, path);
            if (Directory.Exists(combined))
                return Path.GetFullPath(combined);
            if (Directory.Exists(path))
                return Path.GetFullPath(path);
            return path;
        }
    }
}
