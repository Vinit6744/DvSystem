import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private readonly apiUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  export(documentId: string, format: 'pdf' | 'html' = 'pdf'): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/documents/${documentId}/export`, {
      params: { format },
      responseType: 'blob'
    });
  }
}
