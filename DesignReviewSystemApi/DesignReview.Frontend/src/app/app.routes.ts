import { Routes } from '@angular/router';
import { authGuard, reviewerGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.authRoutes)
  },
  {
    path: '',
    loadComponent: () => import('./layouts/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    canActivate: [authGuard, reviewerGuard],
    children: [
      {
        path: '',
        redirectTo: 'documents',
        pathMatch: 'full'
      },
      {
        path: 'documents',
        loadChildren: () => import('./features/documents/documents.routes').then(m => m.documentRoutes)
      },
      {
        path: 'verification',
        loadChildren: () => import('./features/verification/verification.routes').then(m => m.verificationRoutes)
      },
      {
        path: 'review',
        loadChildren: () => import('./features/review/review.routes').then(m => m.reviewRoutes)
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
