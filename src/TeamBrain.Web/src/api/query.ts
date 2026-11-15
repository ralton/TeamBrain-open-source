import client from "./client";

export interface QueryRequest {
  question: string;
  context?: {
    repoId?: string;
    branchName?: string;
    filePath?: string;
    language?: string;
    framework?: string;
    adoWorkItemId?: string;
  };
  domain?: string;
  includeLegacyContext?: boolean;
}

export interface SourceReference {
  type: string;
  id: string;
  title: string;
  status: string;
  classification?: string;
  url?: string;
}

export interface QueryResult {
  answerMarkdown: string;
  status: string;
  confidence: string;
  sources: SourceReference[];
  suggestedTests: string[];
  warnings: string[];
  queryLogId: string;
}

export const queryApi = {
  ask: (data: QueryRequest) => client.post<QueryResult>("/query", data),
};
