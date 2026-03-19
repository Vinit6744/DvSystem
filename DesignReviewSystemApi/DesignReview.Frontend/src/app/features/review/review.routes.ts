import { Routes } from '@angular/router';

export const reviewRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./review-comments/review-comments.component').then(m => m.ReviewCommentsComponent)
  }
];
