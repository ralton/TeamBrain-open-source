import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Card, Title1, Title3, Badge, Text, Spinner, Link } from "@fluentui/react-components";
import ReactMarkdown from "react-markdown";
import { learningRecordsApi } from "../api/learningRecords";

const statusColor: Record<string, "success" | "warning" | "important" | "informative" | "danger"> = {
  Approved: "success",
  Candidate: "informative",
  InReview: "warning",
  Rejected: "danger",
  Legacy: "important",
  Deprecated: "important",
};

export function LearningRecordDetail() {
  const { id } = useParams<{ id: string }>();
  const { data, isLoading, error } = useQuery({
    queryKey: ["learning-record", id],
    queryFn: () => learningRecordsApi.get(id!),
    enabled: !!id,
  });

  if (isLoading) return <Spinner style={{ margin: "80px auto", display: "block" }} />;
  if (error || !data) return <Text>Record not found.</Text>;

  const r = data.data;

  return (
    <div style={{ maxWidth: 900, margin: "0 auto", padding: "24px 16px" }}>
      <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 8 }}>
        <Badge color={statusColor[r.status] ?? "informative"} size="large">{r.status}</Badge>
        <Badge color={r.confidence === "High" ? "success" : r.confidence === "Medium" ? "warning" : "danger"}>
          Confidence: {r.confidence}
        </Badge>
      </div>

      <Title1 style={{ marginBottom: 8 }}>{r.title}</Title1>
      <Text style={{ color: "#666", display: "block", marginBottom: 16 }}>{r.category} · {r.domain}</Text>

      <div style={{ marginBottom: 16 }}>
        {r.tags.map(t => <Badge key={t} style={{ marginRight: 4 }}>{t}</Badge>)}
      </div>

      <Card style={{ padding: 20, marginBottom: 16 }}>
        <Title3 style={{ marginBottom: 8 }}>Summary</Title3>
        <Text>{r.summary}</Text>
      </Card>

      {r.bodyMarkdown && (
        <Card style={{ padding: 20, marginBottom: 16 }}>
          <Title3 style={{ marginBottom: 8 }}>Implementation Guidance</Title3>
          <ReactMarkdown>{r.bodyMarkdown}</ReactMarkdown>
        </Card>
      )}

      {r.sources.length > 0 && (
        <Card style={{ padding: 20, marginBottom: 16 }}>
          <Title3 style={{ marginBottom: 8 }}>Sources</Title3>
          {r.sources.map(s => (
            <div key={s.id} style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 8 }}>
              <Badge color={s.classification === "GoldStandard" ? "warning" : "informative"}>
                {s.classification}
              </Badge>
              {s.sourceUrl ? <Link href={s.sourceUrl}>{s.title}</Link> : <Text>{s.title}</Text>}
              <Text size={200} style={{ color: "#888" }}>{s.artifactType}</Text>
            </div>
          ))}
        </Card>
      )}

      {r.reviewHistory.length > 0 && (
        <Card style={{ padding: 20 }}>
          <Title3 style={{ marginBottom: 8 }}>Review History</Title3>
          {r.reviewHistory.map((rev, i) => (
            <div key={i} style={{ padding: "8px 0", borderBottom: "1px solid #eee" }}>
              <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
                <Badge color={rev.decision === "Approve" ? "success" : "important"}>{rev.decision}</Badge>
                <Text size={200} style={{ color: "#666" }}>{new Date(rev.createdAt).toLocaleDateString()}</Text>
              </div>
              {rev.comments && <Text size={300} style={{ marginTop: 4, display: "block" }}>{rev.comments}</Text>}
            </div>
          ))}
        </Card>
      )}
    </div>
  );
}
