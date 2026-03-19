# Design Review System — API Requirements

APIs needed to satisfy the client requirements. Total: **15 APIs** (minimum set).

---

## 1. Documents (Document Upload & Viewer)

| # | Method | Route | Description | Requirement |
|---|--------|--------|-------------|-------------|
| 1 | POST | `api/documents/upload` | Upload PDF drawing document | 4.1 Document Upload |
| 2 | GET | `api/documents` | List documents (metadata: filename, upload date, uploaded by, status) | 4.1 Metadata |
| 3 | GET | `api/documents/{id}` | Get document detail for viewer (metadata + pages, for navigation/zoom) | 4.1 Viewer |
| 4 | GET | `api/documents/{id}/pages/{pageNumber}` | Get single page (e.g. content or for jump-to-page) | 4.1 Page navigation |

---

## 2. Text Extraction

| # | Method | Route | Description | Requirement |
|---|--------|--------|-------------|-------------|
| 5 | POST | `api/documents/{id}/extract` | Trigger page-wise text extraction (and store); OCR fallback if needed | 4.2 Page-wise Text Extraction |

---

## 3. Verification

| # | Method | Route | Description | Requirement |
|---|--------|--------|-------------|-------------|
| 6 | POST | `api/documents/{id}/verification/run` | Run verification checks; return Pass/Fail/Manual Review, found value, page(s), message | 4.3 Verification Checks |
| 7 | GET | `api/documents/{id}/verification/results` | Get verification results for document | 4.3 |
| 8 | GET | `api/documents/{id}/issues` | Get consolidated issues list with page references (for navigation) | 4.4 Issues List |

---

## 4. Review Status & Comments

| # | Method | Route | Description | Requirement |
|---|--------|--------|-------------|-------------|
| 9 | PUT | `api/documents/{id}/status` | Set final status: Draft / In Review / Needs Changes / Approved | 4.5 Review Status |
| 10 | POST | `api/documents/{id}/comments` | Add reviewer comment/note | 4.5 Comments |
| 11 | GET | `api/documents/{id}/comments` | Get all comments for document | 4.5 |

---

## 5. AI Review Summary

| # | Method | Route | Description | Requirement |
|---|--------|--------|-------------|-------------|
| 12 | POST | `api/documents/{id}/summary/generate` | Generate AI review summary (from extracted content + verification results) | 4.6 AI Review Summary |
| 13 | GET | `api/documents/{id}/summary` | Get stored AI summary for document | 4.6 |

---

## 6. Report Export

| # | Method | Route | Description | Requirement |
|---|--------|--------|-------------|-------------|
| 14 | GET | `api/reports/{documentId}/export?format=pdf` | Export report as PDF | 4.7 Review Report Export |
| 15 | GET | `api/reports/{documentId}/export?format=html` | Export report as HTML | 4.7 |

Report must include: document metadata, verification results table, issue list with page numbers, reviewer comments, AI summary (if generated), final review status.

---

## Summary

- **Total APIs: 15**
- **Controllers:** `DocumentsController`, `VerificationController` (or under documents), `ReviewController` (or under documents), `ReportsController`, plus optional `AuthController` for login/roles.

Optional for MVP: Auth endpoints (login, register, user info) if authentication is required.
