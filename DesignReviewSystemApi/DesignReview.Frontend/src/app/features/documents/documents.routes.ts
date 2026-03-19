import { Routes } from '@angular/router';

export const documentRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./document-list/document-list.component').then(m => m.DocumentListComponent)
  },
  {
    path: 'upload',
    loadComponent: () => import('./document-upload/document-upload.component').then(m => m.DocumentUploadComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./document-viewer/document-viewer.component').then(m => m.DocumentViewerComponent)
  }
];
