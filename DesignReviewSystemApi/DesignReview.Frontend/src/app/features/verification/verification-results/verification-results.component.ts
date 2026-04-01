import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatSortModule } from '@angular/material/sort';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { VerificationService } from '../../../core/services/verification.service';
import { DocumentService } from '../../../core/services/document.service';

import { VerificationRunResponse, VerificationOutcome } from '../../../core/models/verification.model';
import { DocumentMetadata } from '../../../core/models/document.model';

@Component({
  selector: 'app-verification-results',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatSelectModule,
    MatSortModule,
    MatPaginatorModule
  ],
  templateUrl: './verification-results.component.html',
  styleUrl: './verification-results.component.scss'
})
export class VerificationResultsComponent implements OnInit {

  displayedColumns: string[] = ['checkName', 'outcome', 'foundValue', 'pageNumbers', 'message'];

  dataSource = new MatTableDataSource<any>();
  verificationResults: VerificationRunResponse | null = null;
  documents: DocumentMetadata[] = [];

  loading = false;
  documentId: string = '';
  selectedDocumentId: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private verificationService: VerificationService,
    private documentService: DocumentService
  ) {}

  ngOnInit(): void {

    this.loadDocuments();
    this.route.params.subscribe(params => {
      this.documentId = params['documentId'] || '';
      this.selectedDocumentId = this.documentId;

      if (this.documentId) {
        this.loadResults();
      } else {
        // No ID in URL — just show dropdown, clear any previous results
        this.verificationResults = null;
        this.loading = false;
      }
    });
  }

 loadDocuments(): void {
    this.documentService.getList().subscribe({
      next: (docs: DocumentMetadata[]) => {
        this.documents = docs;
        if (!this.documentId && docs.length > 0) {
          this.router.navigate(['/verification', docs[0].id], { replaceUrl: true });
        }
      }
    });
  }

  onDocumentChange(documentId: string): void {
    if (documentId) {
      this.router.navigate(['/verification', documentId]);
    }
  }

  loadResults(): void {
    this.loading = true;

    this.verificationService.getResults(this.documentId).subscribe({
      next: (results: VerificationRunResponse) => {
        this.verificationResults = results;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  getOutcomeColor(outcome: VerificationOutcome): string {
    switch (outcome) {
      case VerificationOutcome.Pass:
        return 'primary';
      case VerificationOutcome.Fail:
        return 'warn';
      case VerificationOutcome.ManualReview:
        return 'accent';
      default:
        return '';
    }
  }
}