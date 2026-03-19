import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { ReviewService } from '../../../core/services/review.service';
import { ReviewComment, AddReviewComment } from '../../../core/models/review.model';

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
    MatListModule
  ],
  templateUrl: './review-comments.component.html',
  styleUrl: './review-comments.component.scss'
})
export class ReviewCommentsComponent implements OnInit {
  documentId: string = '';
  comments: ReviewComment[] = [];
  commentForm: FormGroup;

  constructor(
    private route: ActivatedRoute,
    private reviewService: ReviewService,
    private fb: FormBuilder
  ) {
    this.commentForm = this.fb.group({
      text: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.documentId = this.route.snapshot.params['documentId'] || '';
    if (this.documentId) {
      this.loadComments();
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
