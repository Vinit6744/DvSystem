# Design Review System – Step-by-Step Testing Guide

## Prerequisites

1. **Backend API**
   - Open terminal in `DesignReview.API` folder.
   - Run: `dotnet run`
   - Confirm: "Now listening on: http://localhost:5033"
   - Optional: Open http://localhost:5033/swagger to verify API.

2. **Frontend**
   - Open another terminal in `DesignReview.Frontend` folder.
   - Run: `npm install` (first time only), then `npm start`
   - Browser should open http://localhost:4200 (or open it manually).

3. **Test PDF**
   - Use a PDF drawing that has a title block (e.g. Project Name, Drawing No, Revision, Date, Scale).
   - Or any PDF file for basic upload/view tests.

---

## Step 1: Login

1. Go to **http://localhost:4200**
2. You should see the **Login** page (or be redirected to `/auth/login`).
3. Enter:
   - **Username:** `admin`
   - **Password:** `Admin@123`
4. Click **Login**.
5. **Pass:** You are redirected to the **Documents** list page. No error in console.

---

## Step 2: Upload a Document

1. On the Documents page, click **Upload Document**.
2. Click **Select PDF File** and choose a PDF (e.g. a design drawing).
3. Click **Upload**.
4. **Pass:** Upload completes and you are taken to the **Document Viewer** for that file. PDF is visible in the viewer.

---

## Step 3: View PDF and Page Navigation

1. In the **PDF Viewer** tab, check:
   - PDF loads in the iframe.
   - **Previous** / **Next** page buttons work (if the PDF has multiple pages).
   - **Page** dropdown shows page numbers; changing it updates the selected page.
2. **Pass:** You can move between pages without errors.

---

## Step 4: Extract Text

1. In the same tab, click **Extract Text**.
2. Wait a few seconds (spinner may show).
3. **Pass:** A snackbar says "Text extracted successfully." (or similar). No error in console.
4. Optional: Reload the page and confirm the document still loads.

---

## Step 5: Run Verification

1. Click **Run Verification**.
2. Wait for the request to finish.
3. **Pass:** Snackbar says verification completed. No error.
4. Open the **Verification Results** tab.
5. **Pass:** You see a list of checks (e.g. Project Name, Drawing Number, Revision, Date, Scale) with outcome (Pass / Fail / Manual Review), found value, and page numbers.

---

## Step 6: Issues List and Page Navigation

1. Open the **Issues** tab.
2. **Pass:** You see a list of issues (Fail and Manual Review only). If all checks passed, the list may be empty with message "No issues detected yet."
3. If there are issues: click one issue.
4. **Pass:** The **Page** selector in the PDF Viewer tab reflects the issue’s page (you may need to switch back to PDF Viewer tab to see the change). No console errors.

---

## Step 7: Review Status and Comments

1. Go to the **Document Info** tab.
2. Change **Status** (e.g. from Draft to **In Review**).
3. **Pass:** Change is saved (no error). Reload page and confirm status is still updated.
4. To test **comments:** go to **Review** in the main menu (or the route where you add comments).
5. Add a comment, e.g. "Scale needs confirmation on Page 1."
6. **Pass:** Comment appears in the list with your name and time.

---

## Step 8: AI Review Summary

1. In the Document Viewer, open the **AI Summary** tab.
2. Click **Generate Review Summary**.
3. Wait (may take several seconds).
4. **Pass:** Summary text appears below the button, with "Generated at" time. No error.
5. If you see "Failed to generate AI summary": check that the backend has **OpenAI:ApiKey** set (e.g. in `appsettings.Development.json` or User Secrets). Without a key, the backend uses a basic non-AI summary; confirm with your backend setup.

---

## Step 9: Export Report

1. Go to the **Document Info** tab.
2. Click **Download PDF Report**.
3. **Pass:** A PDF file downloads. Open it and check it contains:
   - Document metadata (name, upload date, etc.)
   - Verification results
   - Issues (if any)
   - Comments
   - AI summary (if generated)
   - Final status
4. Click **Download HTML Report**.
5. **Pass:** An HTML file downloads and opens in the browser with similar content.

---

## Step 10: Logout and Re-login

1. Click your **username** (top right) and choose **Logout**.
2. **Pass:** You are redirected to the Login page.
3. Log in again with `admin` / `Admin@123`.
4. **Pass:** You return to the app and can open the same document; data (status, comments, summary) is still there.

---

## Quick Checklist

| Step | Action | Pass criteria |
|------|--------|----------------|
| 1 | Login | Redirect to Documents, no error |
| 2 | Upload PDF | Viewer opens, PDF visible |
| 3 | Page navigation | Prev/Next and dropdown work |
| 4 | Extract Text | Success message, no error |
| 5 | Run Verification | Results tab shows 5 checks |
| 6 | Issues tab | Issues listed; click navigates page |
| 7 | Status + Comments | Status saves; comment appears |
| 8 | Generate Summary | Summary text appears |
| 9 | Export PDF/HTML | File downloads with full content |
| 10 | Logout / Login | Session and data persist |

---

## Troubleshooting

- **Login fails / CORS:** Ensure backend runs on http://localhost:5033 and CORS allows http://localhost:4200. Restart backend after config change.
- **Upload fails:** Check file is PDF and under size limit (e.g. 50 MB). Check backend console for errors.
- **Verification "Extract text first":** Complete Step 4 before Step 5.
- **AI summary fails:** Set `OpenAI:ApiKey` in backend config; restart API.
- **Report download fails:** Ensure verification (and optionally summary) have been run so the report has content.
