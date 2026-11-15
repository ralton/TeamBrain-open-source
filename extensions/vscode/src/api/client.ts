import * as vscode from 'vscode';

export interface QueryResponse {
  answerMarkdown: string;
  status: string;
  confidence: string;
  sources: Array<{ type: string; id: string; title: string; classification?: string }>;
  suggestedTests: string[];
  warnings: string[];
}

export interface LearningRecordItem {
  id: string;
  title: string;
  summary: string;
  category: string;
  status: string;
  confidence: string;
}

export async function askTeamBrainApi(question: string, repoId?: string): Promise<QueryResponse> {
  const config = vscode.workspace.getConfiguration('teambrain');
  const baseUrl = config.get<string>('apiUrl', 'https://localhost:5001/api/v1').replace(/\/$/, '');
  const token = config.get<string>('authToken', '');

  const payload = {
    question,
    context: repoId ? { repoId } : undefined
  };

  const headers: Record<string, string> = {
    'Content-Type': 'application/json'
  };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(`${baseUrl}/query`, {
    method: 'POST',
    headers,
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    throw new Error(`TeamBrain API returned error HTTP ${response.status}`);
  }

  return (await response.json()) as QueryResponse;
}

export async function submitLearningRecordApi(data: {
  title: string;
  summary: string;
  bodyMarkdown: string;
  category: string;
  tags?: string[];
}): Promise<string> {
  const config = vscode.workspace.getConfiguration('teambrain');
  const baseUrl = config.get<string>('apiUrl', 'https://localhost:5001/api/v1').replace(/\/$/, '');
  const token = config.get<string>('authToken', '');

  const headers: Record<string, string> = {
    'Content-Type': 'application/json'
  };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(`${baseUrl}/learning-records`, {
    method: 'POST',
    headers,
    body: JSON.stringify(data)
  });

  if (!response.ok) {
    throw new Error(`TeamBrain API returned error HTTP ${response.status}`);
  }

  const resJson = (await response.json()) as { id: string };
  return resJson.id;
}

export async function searchPatternsApi(searchText: string): Promise<LearningRecordItem[]> {
  const config = vscode.workspace.getConfiguration('teambrain');
  const baseUrl = config.get<string>('apiUrl', 'https://localhost:5001/api/v1').replace(/\/$/, '');
  const token = config.get<string>('authToken', '');

  const headers: Record<string, string> = {};
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(`${baseUrl}/learning-records/search?searchText=${encodeURIComponent(searchText)}`, {
    method: 'GET',
    headers
  });

  if (!response.ok) {
    return [];
  }

  const resJson = (await response.json()) as { items: LearningRecordItem[] };
  return resJson.items || [];
}
