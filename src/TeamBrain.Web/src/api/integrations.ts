import client from "./client";

export interface LinkWorkItemRequest {
  targetId: string;
  targetType: "thread" | "learning_record";
  provider: "github" | "ado" | "asana";
  workItemId: string;
  workItemUrl: string;
}

export interface GitHubPullRequestResult {
  number: number;
  title: string;
  state: string;
  author: string;
  htmlUrl: string;
  headBranch: string;
  baseBranch: string;
  createdAt: string;
  mergedAt?: string;
}

export interface WorkItemDetailsResult {
  id: string;
  provider: string;
  title: string;
  status: string;
  assignee?: string;
  htmlUrl: string;
  createdAt: string;
}

export const integrationsApi = {
  linkWorkItem: (data: LinkWorkItemRequest) => client.post<{ success: boolean }>("/integrations/link-workitem", data),
  getGitHubPullRequest: (owner: string, repo: string, prNumber: number) =>
    client.get<GitHubPullRequestResult>(`/integrations/github/prs/${owner}/${repo}/${prNumber}`),
  getWorkItem: (provider: string, ownerOrProject: string, repoOrBoard: string, itemId: string) =>
    client.get<WorkItemDetailsResult>(`/integrations/workitems/${provider}/${ownerOrProject}/${repoOrBoard}/${itemId}`),
};
