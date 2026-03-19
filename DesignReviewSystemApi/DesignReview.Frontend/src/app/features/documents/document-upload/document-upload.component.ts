import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { DocumentService } from '../../../core/services/document.service';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatSnackBarModule
  ],
  templateUrl: './document-upload.component.html',
  styleUrl: './document-upload.component.scss'
})
export class DocumentUploadComponent {
  selectedFile: File | null = null;
  uploading = false;
  uploadProgress = 0;

  constructor(
    private documentService: DocumentService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      if (file.type === 'application/pdf') {
        this.selectedFile = file;
      } else {
        this.snackBar.open('Please select a PDF file', 'Close', { duration: 3000 });
      }
    }
  }

  upload(): void {
    if (!this.selectedFile) {
      return;
    }

    this.uploading = true;
    this.uploadProgress = 0;

    this.documentService.upload(this.selectedFile).subscribe({
      next: (document) => {
        this.uploading = false;
        this.uploadProgress = 100;
        this.snackBar.open('Document uploaded successfully', 'Close', { duration: 3000 });
        this.router.navigate(['/documents', document.id]);
      },
      error: (error) => {
        this.uploading = false;
        this.snackBar.open('Upload failed. Please try again.', 'Close', { duration: 3000 });
      }
    });
  }

  cancel(): void {
    this.selectedFile = null;
    this.router.navigate(['/documents']);
  }
}
