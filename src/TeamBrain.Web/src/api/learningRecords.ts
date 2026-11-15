import client from "./client";

export interface LearningRecordSummary {
  id: string;
  title: string;
  summary: string;
  category: string;
  domain?: string;
  status: string;
  confidence: string;
  tags: string[];
  approvedAt?: string;
  reviewedAt?: string;
  updatedAt: string;
}

export interface LearningRecordDetail extends LearningRecordSummary {
  bodyMarkdown?: string;
  version: number;
  ownerTeamId?: string;
  ownerUserId?: string;
  createdByUserId: string;
  approvedByUserId?: string;
  expiresAt?: string;
  createdAt: string;
  sources: SourceSummary[];
  reviewHistory: ReviewSummary[];
}

export interface SourceSummary {
  id: string;
  title: string;
  artifactType: string;
  classification: string;
  sourceUrl?: string;
}

export interface ReviewSummary {
  reviewerId: string;
  decision: string;
  comments?: string;
  createdAt: string;
}

export interface CreateLearningRecordRequest {
  title: string;
  summary: string;
  bodyMarkdown?: string;
  category: string;
  domain?: string;
  confidence: string;
  tags?: string[];
  ownerTeamId?: string;
  ownerUserId?: string;
  threadId?: string;
}

export interface ReviewRequest {
  decision: string;
  comments?: string;
  updatedTitle?: string;
  updatedSummary?: string;
  updatedBodyMarkdown?: string;
  updatedConfidence?: string;
  expiresAt?: string;
}

export interface SearchKnowledgeRecordItem {
  id: string;
  title: string;
  summary: string;
  bodyMarkdown?: string;
  category: string;
  domain?: string;
  status: string;
  confidence: string;
  version: number;
  isAutoPromoted: boolean;
  tags: string[];
  externalWorkItemId?: string;
  externalWorkItemProvider?: string;
  externalWorkItemUrl?: string;
  createdAt: string;
  updatedAt: string;
  score: number;
}

export interface SearchKnowledgeRecordsResponse {
  items: SearchKnowledgeRecordItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const learningRecordsApi = {
  list: (params?: Record<string, string>) =>
    client.get<{ items: LearningRecordSummary[]; totalCount: number }>("/learning-records", { params }),
  search: (params?: Record<string, string>) =>
    client.get<SearchKnowledgeRecordsResponse>("/learning-records/search", { params }),
  get: (id: string) => client.get<LearningRecordDetail>(`/learning-records/${id}`),
  create: (data: CreateLearningRecordRequest) => client.post<{ id: string }>("/learning-records", data),
  review: (id: string, data: ReviewRequest) => client.post(`/learning-records/${id}/review`, data),
  delete: (id: string) => client.delete(`/learning-records/${id}`),
};
