import { Component } from '@angular/core';

@Component({
  selector: 'app-empty-review',
  standalone: true,
  template: `
    <div class="empty-review-container">
      <h1>Review Comments</h1>
      <p>Please select a document to view or add review comments.</p>
    </div>
  `,
  styles: [`
    .empty-review-container {
      padding: 2rem;
      text-align: center;
    }
  `]
})
export class EmptyReviewComponent {}
