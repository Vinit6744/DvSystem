import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { FormsModule } from '@angular/forms';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { DocumentService } from '../../../core/services/document.service';
import { VerificationService } from '../../../core/services/verification.service';
import { ReviewService } from '../../../core/services/review.service';
import { ReportService } from '../../../core/services/report.service';
import { DocumentDetail, ReviewStatus } from '../../../core/models/document.model';
import { VerificationRunResponse, Issue } from '../../../core/models/verification.model';
import { ReviewSummary } from '../../../core/models/review.model';

@Component({
  selector: 'app-document-viewer',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatSnackBarModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatListModule,
    MatDividerModule
  ],
  templateUrl: './document-viewer.component.html',
  styleUrl: './document-viewer.component.scss'
})
export class DocumentViewerComponent implements OnInit {
  documentId: string = '';
  document: DocumentDetail | null = null;
  pdfUrl: SafeResourceUrl | null = null;
  currentPage = 1;
  loading = false;
  verificationResults: VerificationRunResponse | null = null;
  issues: Issue[] = [];
  loadingIssues = false;
  summary: ReviewSummary | null = null;
  loadingSummary = false;
  generatingSummary = false;
  exportingReport = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private documentService: DocumentService,
    private verificationService: VerificationService,
    private reviewService: ReviewService,
    private reportService: ReportService,
    private snackBar: MatSnackBar,
    private sanitizer: DomSanitizer
  ) {}

  get canGoPrevious(): boolean {
    if (!this.document || !this.document.pages || this.document.pages.length === 0) {
      return false;
    }
    const firstPage = this.document.pages[0].pageNumber;
    return this.currentPage > firstPage;
  }

  get canGoNext(): boolean {
    if (!this.document || !this.document.pages || this.document.pages.length === 0) {
      return false;
    }
    const lastPage = this.document.pages[this.document.pages.length - 1].pageNumber;
    return this.currentPage < lastPage;
  }

  ngOnInit(): void {
    this.documentId = this.route.snapshot.params['id'];
    this.loadDocument();
    this.loadPdf();
  }

  loadDocument(): void {
    this.loading = true;
    this.documentService.getById(this.documentId).subscribe({
      next: (doc) => {
        this.document = doc;
        if (this.document.pages && this.document.pages.length > 0) {
          this.currentPage = this.document.pages[0].pageNumber;
        }
        this.loading = false;
        this.loadVerificationResults();
        this.loadIssues();
        this.loadSummary();
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  loadPdf(): void {
    this.documentService.getFile(this.documentId).subscribe({
      next: (blob) => {
        const objectUrl = URL.createObjectURL(blob);
        this.pdfUrl = this.sanitizer.bypassSecurityTrustResourceUrl(objectUrl);
      }
    });
  }

  loadVerificationResults(): void {
    this.verificationService.getResults(this.documentId).subscribe({
      next: (results) => {
        this.verificationResults = results;
      },
      error: () => {
        // No verification results yet
      }
    });
  }

  loadIssues(): void {
    this.loadingIssues = true;
    this.verificationService.getIssues(this.documentId).subscribe({
      next: (issues) => {
        this.issues = issues;
        this.loadingIssues = false;
      },
      error: () => {
        this.loadingIssues = false;
      }
    });
  }

  loadSummary(): void {
    this.loadingSummary = true;
    this.reviewService.getSummary(this.documentId).subscribe({
      next: (summary) => {
        this.summary = summary;
        this.loadingSummary = false;
      },
      error: () => {
        this.loadingSummary = false;
      }
    });
  }

  extractText(): void {
    this.documentService.extractText(this.documentId).subscribe({
      next: () => {
        this.loadDocument();
        this.snackBar.open('Text extracted successfully.', 'Close', { duration: 3000 });
      },
      error: () => {
        this.snackBar.open('Failed to extract text.', 'Close', { duration: 3000 });
      }
    });
  }

  runVerification(): void {
    this.verificationService.runVerification(this.documentId).subscribe({
      next: (results) => {
        this.verificationResults = results;
        this.loadIssues();
        this.snackBar.open('Verification completed.', 'Close', { duration: 3000 });
      },
      error: () => {
        this.snackBar.open('Failed to run verification.', 'Close', { duration: 3000 });
      }
    });
  }

  updateStatus(status: ReviewStatus): void {
    this.documentService.updateStatus(this.documentId, { reviewStatus: status }).subscribe({
      next: () => {
        this.loadDocument();
      }
    });
  }

  jumpToPage(page: number): void {
    if (!this.document || !this.document.pages || this.document.pages.length === 0) {
      return;
    }
    const minPage = this.document.pages[0].pageNumber;
    const maxPage = this.document.pages[this.document.pages.length - 1].pageNumber;
    if (page < minPage || page > maxPage) {
      return;
    }
    this.currentPage = page;
  }

  goToPreviousPage(): void {
    this.jumpToPage(this.currentPage - 1);
  }

  goToNextPage(): void {
    this.jumpToPage(this.currentPage + 1);
  }

  onIssueClick(issue: Issue): void {
    const targetPage =
      issue.firstPageNumber ||
      (issue.pageNumbers ? parseInt(issue.pageNumbers.split(',')[0], 10) : NaN);
    if (!isNaN(targetPage)) {
      this.jumpToPage(targetPage);
    }
  }

  generateSummary(): void {
    this.generatingSummary = true;
    this.reviewService.generateSummary(this.documentId).subscribe({
      next: (summary) => {
        this.summary = summary;
        this.generatingSummary = false;
        this.snackBar.open('AI review summary generated.', 'Close', { duration: 3000 });
      },
      error: () => {
        this.generatingSummary = false;
        this.snackBar.open('Failed to generate AI summary.', 'Close', { duration: 3000 });
      }
    });
  }

  exportReport(format: 'pdf' | 'html' = 'pdf'): void {
    this.exportingReport = true;
    this.reportService.export(this.documentId, format).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        const ext = format === 'pdf' ? 'pdf' : 'html';
        link.href = url;
        link.download = `${this.document?.fileName || 'design-review-report'}.${ext}`;
        link.click();
        URL.revokeObjectURL(url);
        this.exportingReport = false;
      },
      error: () => {
        this.exportingReport = false;
        this.snackBar.open('Failed to export report.', 'Close', { duration: 3000 });
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/documents']);
  }
}
