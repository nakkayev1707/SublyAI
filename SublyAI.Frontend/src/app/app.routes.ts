import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'upload', pathMatch: 'full' },
  { path: 'upload', loadComponent: () => import('./upload/upload.component').then(m => m.UploadComponent) },
  { path: 'processing/:videoId', loadComponent: () => import('./processing/processing.component').then(m => m.ProcessingComponent) },
  { path: 'result/:videoId', loadComponent: () => import('./result/result.component').then(m => m.ResultComponent) },
  { path: '**', redirectTo: 'upload' },
];
