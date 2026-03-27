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
    MatSelectModule
  ],
  templateUrl: './verification-results.component.html',
  styleUrl: './verification-results.component.scss'
})
export class VerificationResultsComponent implements OnInit {

  displayedColumns: string[] = ['checkName', 'outcome', 'foundValue', 'pageNumbers', 'message'];

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

    // this.route.params.subscribe(params => {
    //   this.documentId = params['documentId'];

    //   if (this.documentId) {
    //     this.selectedDocumentId = this.documentId;
    //     this.loadResults();
    //   }
    // });
  }

  loadDocuments(): void {
    this.documentService.getList().subscribe({
      next: (docs: DocumentMetadata[]) => {
        this.documents = docs;
        console.log('Documents loaded:', this.documents);
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