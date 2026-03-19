# Design Review API – Endpoints Summary

All endpoints (except `POST /api/auth/login`) require **JWT** in header: `Authorization: Bearer <token>`.

---

## Auth (`/api/auth`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Login (anonymous). Body: `LoginRequestDto` → returns `LoginResponseDto` (JWT + user info). |
| POST | `/api/auth/register` | Register user (Admin only). Body: `RegisterRequestDto` → returns `LoginResponseDto`. |

---

## Documents (`/api/documents`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/documents/upload` | Upload PDF. Form: `file` (IFormFile) → returns `UploadDocumentDto` (201). |
| GET | `/api/documents` | List documents. Query: `myDocumentsOnly` (bool, optional) → returns `DocumentMetadataDto[]`. |
| GET | `/api/documents/{id}` | Get document detail (metadata + pages) for viewer. |
| GET | `/api/documents/{id}/pages/{pageNumber}` | Get single page (jump-to-page). |
| POST | `/api/documents/{id}/extract` | Extract text from PDF and store per page (run before verification). 204. |
| PUT | `/api/documents/{id}/status` | Update review status. Body: `UpdateReviewStatusDto` (ReviewStatus). 204. |
| DELETE | `/api/documents/{id}` | Delete document and file. 204. |

---

## Verification (`/api/verification`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/verification/documents/{documentId}/run` | Run verification checks. Returns `VerificationRunResponseDto`. |
| GET | `/api/verification/documents/{documentId}/results` | Get verification results. Returns `VerificationRunResponseDto`. |
| GET | `/api/verification/documents/{documentId}/issues` | Get issues list (Fail/Manual Review) with page refs. Returns `IssueDto[]`. |

---

## Review – Comments & Summary (`/api/review`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/review/documents/{documentId}/comments` | Get all comments. Returns `ReviewCommentDto[]`. |
| GET | `/api/review/documents/{documentId}/comments/{commentId}` | Get one comment. |
| POST | `/api/review/documents/{documentId}/comments` | Add comment. Body: `AddReviewCommentDto` (Text). Returns `ReviewCommentDto` (201). |
| PUT | `/api/review/documents/{documentId}/comments/{commentId}` | Update comment (author only). Body: `UpdateReviewCommentDto`. |
| DELETE | `/api/review/documents/{documentId}/comments/{commentId}` | Delete comment. 204. |
| POST | `/api/review/documents/{documentId}/summary/generate` | Generate AI review summary. Returns `ReviewSummaryDto`. |
| GET | `/api/review/documents/{documentId}/summary` | Get stored summary. Returns `ReviewSummaryDto`. |

---

## Reports (`/api/reports`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/reports/documents/{documentId}/export?format=pdf` | Export report. Query: `format` = `pdf` or `html`. Returns file. |

---

## Typical workflow

1. **Login** → `POST /api/auth/login`
2. **Upload PDF** → `POST /api/documents/upload`
3. **Extract text** → `POST /api/documents/{id}/extract`
4. **Run verification** → `POST /api/verification/documents/{id}/run`
5. **Get issues** → `GET /api/verification/documents/{id}/issues`
6. **Add comments** → `POST /api/review/documents/{id}/comments`
7. **Generate summary** → `POST /api/review/documents/{id}/summary/generate`
8. **Update status** → `PUT /api/documents/{id}/status` (Draft / InReview / NeedsChanges / Approved)
9. **Export report** → `GET /api/reports/documents/{id}/export?format=pdf`
