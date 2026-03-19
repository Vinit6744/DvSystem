export interface ReviewComment {
  id: string;
  documentId: string;
  text: string;
  createdBy: string;
  createdByName?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface AddReviewComment {
  text: string;
}

export interface UpdateReviewComment {
  text: string;
}

export interface ReviewSummary {
  id: string;
  documentId: string;
  summaryText: string;
  generatedAt: string;
}
