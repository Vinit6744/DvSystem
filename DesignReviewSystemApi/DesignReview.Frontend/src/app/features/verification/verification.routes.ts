import { Routes } from '@angular/router';

export const verificationRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./verification-results/verification-results.component').then(m => m.VerificationResultsComponent)
  }
];
