import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ReviewComment, AddReviewComment, UpdateReviewComment, ReviewSummary } from '../models/review.model';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private readonly apiUrl = `${environment.apiUrl}/review`;

  constructor(private http: HttpClient) {}

  getComments(documentId: string): Observable<ReviewComment[]> {
    if (!documentId?.trim()) {
      throw new Error('Document ID is required');
    }
    return this.http.get<ReviewComment[]>(`${this.apiUrl}/documents/${documentId}/comments`);
  }

  getComment(documentId: string, commentId: string): Observable<ReviewComment> {
    if (!documentId?.trim() || !commentId?.trim()) {
      throw new Error('Document ID and Comment ID are required');
    }
    return this.http.get<ReviewComment>(`${this.apiUrl}/documents/${documentId}/comments/${commentId}`);
  }

  addComment(documentId: string, comment: AddReviewComment): Observable<ReviewComment> {
    if (!documentId?.trim()) {
      throw new Error('Document ID is required');
    }
    return this.http.post<ReviewComment>(`${this.apiUrl}/documents/${documentId}/comments`, comment);
  }

  updateComment(documentId: string, commentId: string, comment: UpdateReviewComment): Observable<ReviewComment> {
    if (!documentId?.trim() || !commentId?.trim()) {
      throw new Error('Document ID and Comment ID are required');
    }
    return this.http.put<ReviewComment>(`${this.apiUrl}/documents/${documentId}/comments/${commentId}`, comment);
  }

  deleteComment(documentId: string, commentId: string): Observable<void> {
    if (!documentId?.trim() || !commentId?.trim()) {
      throw new Error('Document ID and Comment ID are required');
    }
    return this.http.delete<void>(`${this.apiUrl}/documents/${documentId}/comments/${commentId}`);
  }

  generateSummary(documentId: string): Observable<ReviewSummary> {
    return this.http.post<ReviewSummary>(`${this.apiUrl}/documents/${documentId}/summary/generate`, {});
  }

  getSummary(documentId: string): Observable<ReviewSummary> {
    return this.http.get<ReviewSummary>(`${this.apiUrl}/documents/${documentId}/summary`);
  }
}
