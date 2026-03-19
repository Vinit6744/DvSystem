export interface DocumentMetadata {
  id: string;
  fileName: string;
  uploadedAt: string;
  uploadedBy: string;
  reviewStatus: ReviewStatus;
}

export interface DocumentDetail extends DocumentMetadata {
  pages: DocumentPage[];
}

export interface DocumentPage {
  pageNumber: number;
  extractedText: string;
}

export enum ReviewStatus {
  Draft = 'Draft',
  InReview = 'InReview',
  NeedsChanges = 'NeedsChanges',
  Approved = 'Approved'
}

export interface UpdateReviewStatus {
  reviewStatus: ReviewStatus;
}
