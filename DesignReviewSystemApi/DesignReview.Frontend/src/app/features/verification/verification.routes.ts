import { Routes } from '@angular/router';

export const verificationRoutes: Routes = [
  {
    path: '',  // ← handles /verification with no ID
    loadComponent: () => import('./verification-results/verification-results.component').then(m => m.VerificationResultsComponent)
  },
  {
    path: ':documentId',
    loadComponent: () => import('./verification-results/verification-results.component').then(m => m.VerificationResultsComponent)
  }
];
