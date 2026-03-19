import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DocumentMetadata, DocumentDetail, UpdateReviewStatus } from '../models/document.model';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private readonly apiUrl = `${environment.apiUrl}/documents`;

  constructor(private http: HttpClient) {}

  upload(file: File): Observable<DocumentMetadata> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<DocumentMetadata>(`${this.apiUrl}/upload`, formData);
  }

  getList(myDocumentsOnly: boolean = false): Observable<DocumentMetadata[]> {
    const params = new HttpParams().set('myDocumentsOnly', myDocumentsOnly.toString());
    return this.http.get<DocumentMetadata[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<DocumentDetail> {
    return this.http.get<DocumentDetail>(`${this.apiUrl}/${id}`);
  }

  getFile(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/file`, { responseType: 'blob' });
  }

  getPage(id: string, pageNumber: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}/pages/${pageNumber}`);
  }

  extractText(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/extract`, {});
  }

  updateStatus(id: string, status: UpdateReviewStatus): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/status`, status);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
