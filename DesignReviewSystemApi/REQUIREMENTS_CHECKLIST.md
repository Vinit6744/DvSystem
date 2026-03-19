# Design Review System – Client Requirements Checklist

## 1. Background and Objective
| Requirement | Status |
|-------------|--------|
| Upload PDFs, run verification, structured review output | ✅ Implemented |

## 2. Users and Roles
| Requirement | Status |
|-------------|--------|
| **Reviewer**: Upload PDFs, run verification, review issues, add comments, mark final status (Approved / Needs Changes / Manual Review) | ✅ Reviewer role; Documents, Verification, Review, Reports APIs require Reviewer |
| **Admin**: Manages verification rule templates only | ✅ Admin role; Rules API only; DataSeeder seeds default rules |

## 3. Documents In Scope
| Requirement | Status |
|-------------|--------|
| PDF design/drawing documents | ✅ Upload, store, view |
| Digital and scanned PDFs; OCR fallback when required | ✅ Full implementation: iText text extraction + Docnet.Core (PDF→image) + Tesseract OCR when text length &lt; threshold |

## 4. Core Functional Requirements

### 4.1 Document Upload and Viewer
| Requirement | Status |
|-------------|--------|
| Upload PDF drawing document | ✅ POST /api/documents/upload |
| Viewable inside application | ✅ GET /api/documents/{id}/file returns PDF for viewer |
| Page navigation (next/previous), jump to page, zoom | ✅ API: GetById (pages list), GetPage(id, pageNumber), GetFile(id); zoom is frontend |
| Metadata: Filename, Upload date/time, Uploaded by, Current review status | ✅ DocumentMetadataDto + list/detail APIs |

### 4.2 Page-wise Text Extraction
| Requirement | Status |
|-------------|--------|
| Extract text per page, persist | ✅ POST /api/documents/{id}/extract; DocumentPages table |
| Support verification, AI summary, report export | ✅ Used by VerificationService, ReviewSummaryService, ReportExportService |
| OCR fallback for scanned PDFs | ✅ Docnet.Core renders page to image; Tesseract OCR runs when extracted text length &lt; Ocr:MinTextLengthThreshold; config Ocr:TessDataPath, Ocr:Language |

### 4.3 Verification Checks
| Requirement | Status |
|-------------|--------|
| Run defined checks; return Pass / Fail / Manual Review, found value, page numbers, message | ✅ VerificationResult + VerificationRunResponseDto |
| MVP: Project Name, Drawing Number, Revision, Date, Scale | ✅ Default rules seeded; VerificationService runs rules or built-in checks |
| Engine extensible (additional rules without redesign) | ✅ VerificationRules table; Admin CRUD; RunRuleCheck from DB rules |

### 4.4 Issues List with Page Navigation
| Requirement | Status |
|-------------|--------|
| Consolidated issues with page references | ✅ GET /api/verification/documents/{id}/issues; IssueDto with PageNumbers, FirstPageNumber |
| Click issue → navigate to page | ✅ Frontend uses FirstPageNumber / PageNumbers from API |

### 4.5 Review Status and Comments
| Requirement | Status |
|-------------|--------|
| States: Draft, In Review, Needs Changes, Approved | ✅ ReviewStatus enum; PUT /api/documents/{id}/status |
| Reviewers add comments before final status | ✅ ReviewController: add/update/delete comments |

### 4.6 AI Review Summary
| Requirement | Status |
|-------------|--------|
| Generate after verification; input: extracted content + verification results | ✅ POST /api/review/documents/{id}/summary/generate |
| Output: concise summary, critical missing/inconsistent info, suggested next action | ✅ OpenAI prompt + basic fallback |
| Store per document; optional in export | ✅ ReviewSummaries table; included in report export |

### 4.7 Review Report Export
| Requirement | Status |
|-------------|--------|
| Export PDF or HTML | ✅ GET /api/reports/documents/{id}/export?format=pdf|html |
| Content: metadata, verification results, issues, comments, AI summary, final status | ✅ ReviewReportDto; author names resolved in export |

## 5. Non-Functional Requirements
| Requirement | Status |
|-------------|--------|
| Authentication and role-based access (Reviewer, Admin) | ✅ JWT; [Authorize(Roles = "Reviewer")] / Admin on Rules |
| Uploaded documents and extracted content stored securely | ✅ Auth required; files on server; DB for metadata/text |
| Support reasonable PDF sizes | ✅ FileStorage:MaxFileSizeBytes (e.g. 50 MB); enforced on upload |

## 6. Out of Scope (MVP)
| Item | Status |
|------|--------|
| CAD-level measurement / dimension validation | ✅ Not implemented |
| Drawing object detection / image-based understanding | ✅ Not implemented |
| Bounding-box highlighting of text coordinates | ✅ Not implemented |
| Complex regulatory/engineering compliance | ✅ Not implemented |

## 7. Acceptance Criteria
| Criterion | Status |
|-----------|--------|
| Reviewer can upload and view PDF | ✅ |
| Text extraction per page and stored | ✅ |
| At least five verification checks; Pass/Fail/Manual Review | ✅ |
| Issues with page refs; navigation to page | ✅ |
| Review status and comments can be saved | ✅ |
| AI review summary generated and stored | ✅ |
| Report export with verification, issues, status, AI summary | ✅ |

## Optional / Nice-to-Have Added
| Item | Status |
|------|--------|
| CORS for frontend (SPA on different origin) | ✅ Configured; Cors:AllowedOrigins in appsettings |
| Max upload file size config | ✅ FileStorage:MaxFileSizeBytes |
| PDF file endpoint for in-app viewer | ✅ GET /api/documents/{id}/file |
| Reviewer/author names in exported report | ✅ BuildReviewCommentsWithAuthorNamesAsync |

---

## OCR setup (scanned PDFs)

OCR runs when a page yields little or no text (e.g. scanned PDFs). It uses **Docnet.Core** (PDF→image) and **Tesseract** (OCR).

1. **Tessdata** – Tesseract needs language data. Download `eng.traineddata` and place it in a `tessdata` folder:
   - Get: https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata  
   - Put: `tessdata/eng.traineddata` next to the running app (e.g. `DesignReview.API/bin/Debug/net8.0/tessdata/`) or set `Ocr:TessDataPath` in appsettings to the full path of the folder containing `eng.traineddata`.
2. **Config** – In `appsettings.json`: `Ocr:Enabled` (default true), `Ocr:MinTextLengthThreshold` (default 20), `Ocr:Language` (default "eng"), `Ocr:RenderWidth` / `Ocr:RenderHeight` (page render size for OCR).
3. **Platform** – OCR uses System.Drawing and is supported on **Windows**. On Linux/macOS, set `Ocr:Enabled` to false to skip OCR and keep iText-only extraction.

---
*Last checked against client requirements document. OCR fully implemented with Docnet.Core + Tesseract.*
