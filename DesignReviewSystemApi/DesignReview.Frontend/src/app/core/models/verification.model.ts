export interface VerificationResult {
  id: string;
  documentId: string;
  checkName: string;
  outcome: VerificationOutcome;
  foundValue?: string;
  pageNumbers?: string;
  message?: string;
}

export interface VerificationRunResponse {
  documentId: string;
  results: VerificationResult[];
  runAt: string;
}

export interface Issue {
  id: string;
  checkName: string;
  outcome: VerificationOutcome;
  foundValue?: string;
  pageNumbers?: string;
  firstPageNumber?: number;
  message?: string;
}

export enum VerificationOutcome {
  Pass = 'Pass',
  Fail = 'Fail',
  ManualReview = 'ManualReview'
}
