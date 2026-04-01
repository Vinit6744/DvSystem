import { Routes } from '@angular/router';

export const reviewRoutes: Routes = [
  {
    path: '',  // ← handles /review with no ID
    loadComponent: () => import('./review-comments/review-comments.component').then(m => m.ReviewCommentsComponent)
  },
  {
    path: ':documentId',
    loadComponent: () => import('./review-comments/review-comments.component').then(m => m.ReviewCommentsComponent)
  }
];