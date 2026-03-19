import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { VerificationRunResponse, Issue } from '../models/verification.model';

@Injectable({
  providedIn: 'root'
})
export class VerificationService {
  private readonly apiUrl = `${environment.apiUrl}/verification`;

  constructor(private http: HttpClient) {}

  runVerification(documentId: string): Observable<VerificationRunResponse> {
    return this.http.post<VerificationRunResponse>(`${this.apiUrl}/documents/${documentId}/run`, {});
  }

  getResults(documentId: string): Observable<VerificationRunResponse> {
    return this.http.get<VerificationRunResponse>(`${this.apiUrl}/documents/${documentId}/results`);
  }

  getIssues(documentId: string): Observable<Issue[]> {
    return this.http.get<Issue[]>(`${this.apiUrl}/documents/${documentId}/issues`);
  }
}
