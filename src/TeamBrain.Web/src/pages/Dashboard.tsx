import { useQuery } from "@tanstack/react-query";
import { Card, Title1, Title3, Badge, Button, Text, Spinner } from "@fluentui/react-components";
import { useNavigate } from "react-router-dom";
import { learningRecordsApi } from "../api/learningRecords";

export function Dashboard() {
  const navigate = useNavigate();

  const { data: pending, isLoading: pendingLoading } = useQuery({
    queryKey: ["learning-records", "candidate"],
    queryFn: () => learningRecordsApi.list({ status: "Candidate", pageSize: "5" }),
  });

  const { data: recent, isLoading: recentLoading } = useQuery({
    queryKey: ["learning-records", "approved"],
    queryFn: () => learningRecordsApi.list({ status: "Approved", pageSize: "5" }),
  });

  return (
    <div style={{ maxWidth: 1200, margin: "0 auto", padding: "24px 16px" }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 24 }}>
        <Title1>Dashboard</Title1>
        <div style={{ display: "flex", gap: 12 }}>
          <Button appearance="primary" onClick={() => navigate("/ask")}>Ask TeamBrain</Button>
          <Button onClick={() => navigate("/threads/new")}>Create Thread</Button>
          <Button onClick={() => navigate("/learning-records/new")}>Submit Learning</Button>
        </div>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 24 }}>
        <Card style={{ padding: 20 }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 12 }}>
            <Title3>Pending Reviews</Title3>
            <Badge color="warning">{pending?.data.totalCount ?? 0}</Badge>
          </div>
          {pendingLoading ? <Spinner size="tiny" /> : pending?.data.items.map(r => (
            <div key={r.id} style={{ padding: "8px 0", borderBottom: "1px solid #eee", cursor: "pointer" }}
              onClick={() => navigate(`/review-queue/${r.id}`)}>
              <Text weight="semibold">{r.title}</Text>
              <br />
              <Text size={200} style={{ color: "#666" }}>{r.category} · {r.domain}</Text>
            </div>
          ))}
          <Button appearance="subtle" style={{ marginTop: 8 }} onClick={() => navigate("/review-queue")}>
            View all →
          </Button>
        </Card>

        <Card style={{ padding: 20 }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 12 }}>
            <Title3>Recently Approved</Title3>
            <Badge color="success">{recent?.data.totalCount ?? 0}</Badge>
          </div>
          {recentLoading ? <Spinner size="tiny" /> : recent?.data.items.map(r => (
            <div key={r.id} style={{ padding: "8px 0", borderBottom: "1px solid #eee", cursor: "pointer" }}
              onClick={() => navigate(`/learning-records/${r.id}`)}>
              <Text weight="semibold">{r.title}</Text>
              <br />
              <Text size={200} style={{ color: "#666" }}>{r.category} · Confidence: {r.confidence}</Text>
            </div>
          ))}
          <Button appearance="subtle" style={{ marginTop: 8 }} onClick={() => navigate("/learning-records")}>
            Browse all →
          </Button>
        </Card>
      </div>
    </div>
  );
}
