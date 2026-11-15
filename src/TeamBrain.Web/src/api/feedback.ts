import client from "./client";

export interface FeedbackRequest {
  queryLogId: string;
  isHelpful: boolean;
  comment?: string;
}

export const feedbackApi = {
  submit: (data: FeedbackRequest) => client.post<{ id: string }>("/feedback", data),
};
