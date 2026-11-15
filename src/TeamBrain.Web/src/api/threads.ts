import client from "./client";

export interface ThreadSummary {
  id: string;
  title: string;
  threadType: string;
  status: string;
  repoId?: string;
  adoWorkItemId?: string;
  asanaTaskId?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ThreadDetail extends ThreadSummary {
  branchName?: string;
  summary?: string;
  createdByUserId: string;
  messages: ThreadMessage[];
  participantIds: string[];
  sourceArtifacts: ThreadArtifact[];
}

export interface ThreadMessage {
  id: string;
  authorUserId: string;
  role: "user" | "assistant";
  content: string;
  createdAt: string;
}

export interface ThreadArtifact {
  id: string;
  title: string;
  artifactType: string;
  classification: string;
  sourceUrl?: string;
}

export interface ThreadsListResponse {
  items: ThreadSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const threadsApi = {
  list: (page = 1, pageSize = 20) => client.get<ThreadsListResponse>(`/threads?page=${page}&pageSize=${pageSize}`),
  create: (data: { title: string; threadType: string; repoId?: string; branchName?: string; adoWorkItemId?: string; asanaTaskId?: string }) =>
    client.post<{ id: string }>("/threads", data),
  get: (id: string) => client.get<ThreadDetail>(`/threads/${id}`),
  update: (id: string, data: Partial<{ title: string; threadType: string; status: string; repoId: string; branchName: string; adoWorkItemId: string; asanaTaskId: string; summary: string }>) =>
    client.patch(`/threads/${id}`, data),
  addMessage: (id: string, content: string) => client.post<{ messageId: string }>(`/threads/${id}/messages`, { content }),
  promoteLearning: (id: string) => client.post<{ learningRecordId: string }>(`/threads/${id}/promote-learning`),
};
