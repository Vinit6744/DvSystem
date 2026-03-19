# OCR setup for scanned PDFs

When a PDF page yields little or no text (e.g. scanned documents), the system uses **Tesseract** OCR on a rendered image of the page.

## 1. Download tessdata

Tesseract needs language data (e.g. `eng.traineddata` for English):

- **URL:** https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata  
- Create a folder named **tessdata** next to the running executable (e.g. `DesignReview.API/bin/Debug/net8.0/tessdata/`).  
- Save the file as: `tessdata/eng.traineddata`

Or use an absolute path in appsettings:

```json
"Ocr": {
  "TessDataPath": "C:\\path\\to\\tessdata",
  "Language": "eng"
}
```

## 2. Configuration (appsettings.json)

| Setting | Default | Description |
|---------|---------|-------------|
| Ocr:Enabled | true | Turn OCR fallback on/off |
| Ocr:MinTextLengthThreshold | 20 | If extracted text length per page is below this, run OCR |
| Ocr:TessDataPath | tessdata | Path to tessdata folder (relative to app base or absolute) |
| Ocr:Language | eng | Tesseract language code |
| Ocr:RenderWidth / RenderHeight | 1700 / 2200 | PDF page render size for OCR (higher = better quality, slower) |

## 3. Platform

OCR uses **System.Drawing** and **Docnet.Core** (PDF→image). It is supported on **Windows**. On Linux/macOS, set `Ocr:Enabled` to `false` to use iText text extraction only.
