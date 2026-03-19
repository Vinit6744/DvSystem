import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { VerificationService } from '../../../core/services/verification.service';
import { VerificationRunResponse, VerificationOutcome } from '../../../core/models/verification.model';

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
    MatProgressSpinnerModule
  ],
  templateUrl: './verification-results.component.html',
  styleUrl: './verification-results.component.scss'
})
export class VerificationResultsComponent implements OnInit {
  displayedColumns: string[] = ['checkName', 'outcome', 'foundValue', 'pageNumbers', 'message'];
  verificationResults: VerificationRunResponse | null = null;
  loading = false;
  documentId: string = '';

  constructor(
    private route: ActivatedRoute,
    private verificationService: VerificationService
  ) {}

  ngOnInit(): void {
    this.documentId = this.route.snapshot.params['documentId'] || '';
    if (this.documentId) {
      this.loadResults();
    }
  }

  loadResults(): void {
    this.loading = true;
    this.verificationService.getResults(this.documentId).subscribe({
      next: (results) => {
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
