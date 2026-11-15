import { useQuery } from "@tanstack/react-query";
import {
  Card,
  Title1,
  Title3,
  Badge,
  Text,
  Spinner,
} from "@fluentui/react-components";
import { analyticsApi, type PilotMetrics } from "../api/analytics";

export function PilotDashboard() {
  const { data: response, isLoading } = useQuery({
    queryKey: ["pilot-metrics"],
    queryFn: () => analyticsApi.getPilotMetrics(),
    refetchInterval: 10000,
  });

  const metrics: PilotMetrics | undefined = response?.data;

  if (isLoading) return <Spinner label="Loading pilot analytics & metrics..." />;

  return (
    <div style={{ maxWidth: 1200, margin: "0 auto", padding: "24px 16px" }}>
      <div style={{ marginBottom: 24 }}>
        <Title1>Developer Pilot & Telemetry Dashboard</Title1>
        <p style={{ color: "#666", marginTop: 4 }}>
          Live operational metrics tracking knowledge adoption, approved pattern retrieval rates, and developer feedback sentiment during pilot rollout.
        </p>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: 16, marginBottom: 32 }}>
        <Card style={{ padding: 20, textAlign: "center" }}>
          <Text size={700} weight="bold">{metrics?.totalLearningRecords ?? 0}</Text>
          <Text size={200} style={{ color: "#666", display: "block", marginTop: 4 }}>Total Knowledge Base Records</Text>
        </Card>

        <Card style={{ padding: 20, textAlign: "center" }}>
          <Text size={700} weight="bold" style={{ color: "#107c41" }}>{metrics?.approvedRecords ?? 0}</Text>
          <Text size={200} style={{ color: "#666", display: "block", marginTop: 4 }}>Approved Team Standards</Text>
        </Card>

        <Card style={{ padding: 20, textAlign: "center" }}>
          <Text size={700} weight="bold" style={{ color: "#0078d4" }}>{metrics?.patternMatchRatePercentage ?? 0}%</Text>
          <Text size={200} style={{ color: "#666", display: "block", marginTop: 4 }}>Pattern Match Success Rate</Text>
        </Card>

        <Card style={{ padding: 20, textAlign: "center" }}>
          <Text size={700} weight="bold" style={{ color: (metrics?.sentimentHelpfulPercentage ?? 0) >= 80 ? "#107c41" : "#d97706" }}>
            {metrics?.sentimentHelpfulPercentage ?? 0}%
          </Text>
          <Text size={200} style={{ color: "#666", display: "block", marginTop: 4 }}>Developer Sentiment (Helpful 👍)</Text>
        </Card>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 24 }}>
        <Card style={{ padding: 24 }}>
          <Title3 style={{ marginBottom: 16 }}>Knowledge Distribution</Title3>
          <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <Badge color="success">Approved Standards</Badge>
              <Text weight="bold">{metrics?.approvedRecords ?? 0}</Text>
            </div>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <Badge color="warning">Candidate Learnings</Badge>
              <Text weight="bold">{metrics?.candidateRecords ?? 0}</Text>
            </div>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <Badge appearance="outline" style={{ color: "#d97706" }}>Legacy Warnings</Badge>
              <Text weight="bold">{metrics?.legacyRecords ?? 0}</Text>
            </div>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <Badge color="danger">Anti-Patterns</Badge>
              <Text weight="bold">{metrics?.antiPatternRecords ?? 0}</Text>
            </div>
          </div>
        </Card>

        <Card style={{ padding: 24 }}>
          <Title3 style={{ marginBottom: 16 }}>Query Telemetry</Title3>
          <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <Text style={{ color: "#666" }}>Total AI Questions Asked:</Text>
              <Text weight="bold">{metrics?.totalQueriesAsked ?? 0}</Text>
            </div>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <Text style={{ color: "#666" }}>Approved Pattern Match Count:</Text>
              <Text weight="bold">{metrics?.approvedPatternMatches ?? 0}</Text>
            </div>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <Text style={{ color: "#666" }}>Total Feedback Submissions:</Text>
              <Text weight="bold">{metrics?.totalFeedbackCount ?? 0}</Text>
            </div>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <Text style={{ color: "#666" }}>Helpful 👍 vs Not Helpful 👎:</Text>
              <Text weight="bold" style={{ color: "#107c41" }}>{metrics?.helpfulCount ?? 0} 👍</Text> / <Text weight="bold" style={{ color: "#d13438" }}>{metrics?.notHelpfulCount ?? 0} 👎</Text>
            </div>
          </div>
        </Card>
      </div>

      <Card style={{ padding: 24, marginTop: 24 }}>
        <Title3 style={{ marginBottom: 16 }}>Recent Developer Feedback Comments</Title3>
        {metrics?.recentFeedbackComments.length === 0 ? (
          <Text style={{ color: "#666" }}>No written feedback comments submitted yet.</Text>
        ) : (
          <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
            {metrics?.recentFeedbackComments.map((fb) => (
              <div key={fb.feedbackId} style={{ padding: 12, borderBottom: "1px solid #eee", backgroundColor: fb.isHelpful ? "#f0fdf4" : "#fef2f2", borderRadius: 4 }}>
                <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 6 }}>
                  <Badge color={fb.isHelpful ? "success" : "danger"}>
                    {fb.isHelpful ? "👍 Helpful" : "👎 Not Helpful"}
                  </Badge>
                  <Text size={200} style={{ color: "#888" }}>{new Date(fb.createdAt).toLocaleString()}</Text>
                </div>
                <Text size={300}>{fb.comment}</Text>
              </div>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
}
