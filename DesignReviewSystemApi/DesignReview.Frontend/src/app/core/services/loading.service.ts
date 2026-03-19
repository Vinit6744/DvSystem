import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private _activeRequests = signal(0);

  isLoading = this._activeRequests.asReadonly();

  show(): void {
    this._activeRequests.update(count => count + 1);
  }

  hide(): void {
    this._activeRequests.update(count => Math.max(0, count - 1));
  }
}

