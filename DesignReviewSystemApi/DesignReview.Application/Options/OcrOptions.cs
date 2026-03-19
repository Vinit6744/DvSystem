namespace DesignReview.Application.Options
{
    /// <summary>
    /// Options for OCR fallback when PDF text extraction returns little or no text (e.g. scanned PDFs).
    /// </summary>
    public class OcrOptions
    {
        public const string SectionName = "Ocr";

        /// <summary>
        /// Enable OCR fallback when extracted text length is below threshold. Default true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// If extracted text length (per page) is below this value, run OCR on the rendered page image. Default 20.
        /// </summary>
        public int MinTextLengthThreshold { get; set; } = 20;

        /// <summary>
        /// Path to Tesseract tessdata folder (e.g. "tessdata" or "C:\tessdata"). If empty, uses default from Tesseract package or current directory.
        /// </summary>
        public string TessDataPath { get; set; } = "tessdata";

        /// <summary>
        /// Tesseract language code (e.g. "eng" for English). Default "eng".
        /// </summary>
        public string Language { get; set; } = "eng";

        /// <summary>
        /// Width to render each PDF page for OCR (higher = better quality, slower). Default 1700.
        /// </summary>
        public int RenderWidth { get; set; } = 1700;

        /// <summary>
        /// Height to render each PDF page for OCR. Default 2200 (roughly A4 at good DPI).
        /// </summary>
        public int RenderHeight { get; set; } = 2200;
    }
}
