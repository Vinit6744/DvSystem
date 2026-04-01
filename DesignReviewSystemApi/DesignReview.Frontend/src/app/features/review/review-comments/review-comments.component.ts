import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';  // ← add Router
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSelectModule } from '@angular/material/select';  // ← add
import { ReviewService } from '../../../core/services/review.service';
import { DocumentService } from '../../../core/services/document.service';  // ← add
import { ReviewComment, AddReviewComment } from '../../../core/models/review.model';
import { DocumentMetadata } from '../../../core/models/document.model';  // ← add

@Component({
  selector: 'app-review-comments',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatSelectModule 
  ],
  templateUrl: './review-comments.component.html',
  styleUrl: './review-comments.component.scss'
})
export class ReviewCommentsComponent implements OnInit {
  documentId: string = '';
  selectedDocumentId: string = '';  
  documents: DocumentMetadata[] = [];
  comments: ReviewComment[] = [];
  commentForm: FormGroup;

  constructor(
    private route: ActivatedRoute,
    private router: Router, 
    private reviewService: ReviewService,
    private documentService: DocumentService, 
    private fb: FormBuilder
  ) {
    this.commentForm = this.fb.group({
      text: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadDocuments();

    this.route.params.subscribe(params => {
      this.documentId = params['documentId'] || '';
      this.selectedDocumentId = this.documentId;

      if (this.documentId) {
        this.loadComments();
      } else {
        // No ID in URL — show dropdown, clear previous comments
        this.comments = [];
      }
    });
  }

  loadDocuments(): void {
    this.documentService.getList().subscribe({
      next: (docs: DocumentMetadata[]) => {
        this.documents = docs;
        if (!this.documentId && docs.length > 0) {
          this.router.navigate(['/review', docs[0].id], { replaceUrl: true });
        }
      }
    });
  }

  onDocumentChange(documentId: string): void {
    if (documentId) {
      this.router.navigate(['/review', documentId]);
    }
  }

  loadComments(): void {
    this.reviewService.getComments(this.documentId).subscribe({
      next: (comments) => {
        this.comments = comments;
      }
    });
  }

  addComment(): void {
    if (this.commentForm.valid) {
      const comment: AddReviewComment = {
        text: this.commentForm.value.text
      };
      this.reviewService.addComment(this.documentId, comment).subscribe({
        next: () => {
          this.commentForm.reset();
          this.loadComments();
        }
      });
    }
  }
}