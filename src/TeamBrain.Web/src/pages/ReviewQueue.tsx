import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  Button, Card, Title1, Title3, Badge, Text, Spinner,
  Dialog, DialogBody, DialogTitle, DialogContent, DialogActions,
  Textarea, Field, Select,
} from "@fluentui/react-components";
import { learningRecordsApi, LearningRecordDetail } from "../api/learningRecords";

export function ReviewQueue() {
  const qc = useQueryClient();
  const [reviewing, setReviewing] = useState<LearningRecordDetail | null>(null);
  const [decision, setDecision] = useState("Approve");
  const [comments, setComments] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["learning-records", "review-queue"],
    queryFn: () => learningRecordsApi.list({ status: "Candidate" }),
  });

  const reviewMutation = useMutation({
    mutationFn: ({ id, decision, comments }: { id: string; decision: string; comments: string }) =>
      learningRecordsApi.review(id, { decision, comments }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["learning-records"] });
      setReviewing(null);
      setComments("");
    },
  });

  const openRecord = async (id: string) => {
    const { data } = await learningRecordsApi.get(id);
    setReviewing(data);
  };

  return (
    <div style={{ maxWidth: 900, margin: "0 auto", padding: "24px 16px" }}>
      <Title1 style={{ marginBottom: 24 }}>Review Queue</Title1>

      {isLoading ? <Spinner /> : (
        <div>
          {data?.data.items.length === 0 && (
            <Text>No candidate records awaiting review.</Text>
          )}
          {data?.data.items.map(r => (
            <Card key={r.id} style={{ padding: 16, marginBottom: 12 }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
                <div>
                  <Text weight="semibold" size={400}>{r.title}</Text>
                  <br />
                  <Text size={200} style={{ color: "#666" }}>{r.category} · {r.domain}</Text>
                  <br />
                  <Text size={300} style={{ marginTop: 4, display: "block" }}>{r.summary}</Text>
                  <div style={{ marginTop: 8 }}>
                    {r.tags.map(t => <Badge key={t} style={{ marginRight: 4 }}>{t}</Badge>)}
                  </div>
                </div>
                <Button appearance="primary" onClick={() => openRecord(r.id)}>Review</Button>
              </div>
            </Card>
          ))}
        </div>
      )}

      <Dialog open={reviewing !== null} onOpenChange={(_, d) => { if (!d.open) setReviewing(null); }}>
        <DialogBody>
          <DialogTitle>{reviewing?.title}</DialogTitle>
          <DialogContent>
            <Text style={{ display: "block", marginBottom: 12 }}>{reviewing?.summary}</Text>
            <Field label="Decision" required>
              <Select value={decision} onChange={(_, d) => setDecision(d.value)}>
                <option value="Approve">Approve</option>
                <option value="Reject">Reject</option>
                <option value="RequestChanges">Request Changes</option>
                <option value="MarkLegacy">Mark Legacy</option>
                <option value="MarkDeprecated">Mark Deprecated</option>
              </Select>
            </Field>
            <Field label="Comments" style={{ marginTop: 12 }}>
              <Textarea
                value={comments}
                onChange={(_, d) => setComments(d.value)}
                placeholder="Optional review comments..."
                rows={3}
              />
            </Field>
          </DialogContent>
          <DialogActions>
            <Button appearance="secondary" onClick={() => setReviewing(null)}>Cancel</Button>
            <Button
              appearance="primary"
              onClick={() => reviewing && reviewMutation.mutate({ id: reviewing.id, decision, comments })}
              disabled={reviewMutation.isPending}
              icon={reviewMutation.isPending ? <Spinner size="tiny" /> : undefined}
            >
              Submit Review
            </Button>
          </DialogActions>
        </DialogBody>
      </Dialog>
    </div>
  );
}
