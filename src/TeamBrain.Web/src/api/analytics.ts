import client from "./client";

export interface RecentFeedbackItem {
  feedbackId: string;
  queryLogId: string;
  isHelpful: boolean;
  comment?: string;
  createdAt: string;
}

export interface PilotMetrics {
  totalLearningRecords: number;
  approvedRecords: number;
  candidateRecords: number;
  legacyRecords: number;
  antiPatternRecords: number;
  totalQueriesAsked: number;
  approvedPatternMatches: number;
  patternMatchRatePercentage: number;
  totalFeedbackCount: number;
  helpfulCount: number;
  notHelpfulCount: number;
  sentimentHelpfulPercentage: number;
  recentFeedbackComments: RecentFeedbackItem[];
}

export const analyticsApi = {
  getPilotMetrics: () => client.get<PilotMetrics>("/analytics/pilot-metrics"),
};
