import client from "./client";

export interface IngestionJobSummary {
  id: string;
  sourceType: string;
  sourceReference: string;
  status: string;
  artifactsProcessed: number;
  artifactsQuarantined: number;
  errorMessage?: string;
  createdByUserId: string;
  createdAt: string;
  startedAt?: string;
  completedAt?: string;
}

export interface IngestionError {
  id: string;
  errorType: string;
  message: string;
  sourceReference?: string;
  createdAt: string;
}

export interface IngestionJobDetail extends IngestionJobSummary {
  errors: IngestionError[];
}

export interface IngestionJobsListResponse {
  items: IngestionJobSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const ingestionJobsApi = {
  list: (page = 1, pageSize = 20) => client.get<IngestionJobsListResponse>(`/ingestion/jobs?page=${page}&pageSize=${pageSize}`),
  get: (id: string) => client.get<IngestionJobDetail>(`/ingestion/jobs/${id}`),
  submit: (data: { sourceType: string; sourceReference: string }) => client.post<{ id: string }>("/ingestion/jobs", data),
};
