import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { DocumentService } from '../../../core/services/document.service';
import { DocumentMetadata, ReviewStatus } from '../../../core/models/document.model';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDialogModule
  ],
  templateUrl: './document-list.component.html',
  styleUrl: './document-list.component.scss'
})
export class DocumentListComponent implements OnInit {
  documents: DocumentMetadata[] = [];
  loading = false;

  constructor(
    private documentService: DocumentService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.loading = true;
    this.documentService.getList().subscribe({
      next: (docs) => {
        this.documents = docs;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  getStatusColor(status: ReviewStatus): string {
    switch (status) {
      case ReviewStatus.Approved:
        return 'primary';
      case ReviewStatus.NeedsChanges:
        return 'warn';
      case ReviewStatus.InReview:
        return 'accent';
      default:
        return '';
    }
  }

  deleteDocument(id: string): void {
    const data: ConfirmDialogData = {
      title: 'Delete document',
      message: 'Are you sure you want to remove this document? This action cannot be undone.',
      confirmText: 'Yes',
      cancelText: 'No'
    };

    this.dialog.open(ConfirmDialogComponent, { data }).afterClosed().subscribe(result => {
      if (result) {
        this.documentService.delete(id).subscribe({
          next: () => this.loadDocuments()
        });
      }
    });
  }
}
