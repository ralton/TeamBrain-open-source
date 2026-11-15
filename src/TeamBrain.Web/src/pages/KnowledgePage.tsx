import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import {
  Card,
  Title1,
  Title3,
  Badge,
  Button,
  Text,
  Spinner,
  Input,
  Select,
} from "@fluentui/react-components";
import { learningRecordsApi, SearchKnowledgeRecordItem } from "../api/learningRecords";

export function KnowledgePage() {
  const navigate = useNavigate();

  const [searchText, setSearchText] = useState("");
  const [category, setCategory] = useState("all");
  const [status, setStatus] = useState("all");
  const [confidence, setConfidence] = useState("all");

  const queryParams: Record<string, string> = {
    page: "1",
    pageSize: "50",
  };

  if (searchText.trim()) queryParams.searchText = searchText.trim();
  if (category !== "all") queryParams.category = category;
  if (status !== "all") queryParams.status = status;
  if (confidence !== "all") queryParams.minimumConfidence = confidence;

  const { data: response, isLoading } = useQuery({
    queryKey: ["knowledge-search", queryParams],
    queryFn: () => learningRecordsApi.search(queryParams),
  });

  const items = response?.data.items ?? [];
  const totalCount = response?.data.totalCount ?? 0;

  const getStatusBadge = (item: SearchKnowledgeRecordItem) => {
    const statusLower = item.status.toLowerCase();
    const categoryLower = item.category.toLowerCase();

    if (categoryLower.includes("anti") || statusLower.includes("anti")) {
      return <Badge color="danger">Anti-Pattern</Badge>;
    }

    switch (statusLower) {
      case "approved":
        return <Badge color="success">Approved Standard</Badge>;
      case "candidate":
        return <Badge color="warning">Candidate</Badge>;
      case "legacy":
        return <Badge appearance="outline" style={{ color: "#d97706", borderColor: "#f59e0b" }}>Legacy Warning</Badge>;
      case "deprecated":
        return <Badge appearance="tint">Deprecated</Badge>;
      default:
        return <Badge appearance="outline">{item.status}</Badge>;
    }
  };

  return (
    <div style={{ maxWidth: 1200, margin: "0 auto", padding: "24px 16px" }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 24 }}>
        <div>
          <Title1>Knowledge Directory</Title1>
          <p style={{ color: "#666", marginTop: 4 }}>
            Governed team memory — search approved patterns, canonical code examples, legacy warnings, and anti-patterns.
          </p>
        </div>
        <Button appearance="primary" onClick={() => navigate("/learning-records/new")}>
          Submit Learning
        </Button>
      </div>

      <Card style={{ padding: 20, marginBottom: 24 }}>
        <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          <Input
            value={searchText}
            onChange={e => setSearchText(e.target.value)}
            placeholder="Search team standards, architecture patterns, code examples..."
            style={{ width: "100%" }}
          />

          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 16 }}>
            <div>
              <Text size={200} weight="semibold" style={{ display: "block", marginBottom: 4 }}>Category</Text>
              <Select value={category} onChange={e => setCategory(e.target.value)} style={{ width: "100%" }}>
                <option value="all">All Categories</option>
                <option value="Service Pattern">Service Pattern</option>
                <option value="Database">Database</option>
                <option value="Security">Security</option>
                <option value="UI">UI / Frontend</option>
                <option value="Architecture">Architecture</option>
                <option value="DevOps">DevOps & Ingestion</option>
              </Select>
            </div>

            <div>
              <Text size={200} weight="semibold" style={{ display: "block", marginBottom: 4 }}>Knowledge Status</Text>
              <Select value={status} onChange={e => setStatus(e.target.value)} style={{ width: "100%" }}>
                <option value="all">All Statuses</option>
                <option value="Approved">Approved Standard</option>
                <option value="Candidate">Candidate</option>
                <option value="Legacy">Legacy Warning</option>
                <option value="AntiPattern">Anti-Pattern</option>
                <option value="Deprecated">Deprecated</option>
              </Select>
            </div>

            <div>
              <Text size={200} weight="semibold" style={{ display: "block", marginBottom: 4 }}>Confidence Level</Text>
              <Select value={confidence} onChange={e => setConfidence(e.target.value)} style={{ width: "100%" }}>
                <option value="all">All Confidence Levels</option>
                <option value="High">High Confidence</option>
                <option value="Medium">Medium Confidence</option>
                <option value="Low">Low Confidence</option>
              </Select>
            </div>
          </div>
        </div>
      </Card>

      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 16 }}>
        <Text size={300} style={{ color: "#666" }}>
          Showing <strong>{items.length}</strong> of <strong>{totalCount}</strong> knowledge records
        </Text>
      </div>

      {isLoading ? (
        <Spinner label="Searching knowledge base..." />
      ) : items.length === 0 ? (
        <Card style={{ padding: 32, textAlign: "center" }}>
          <Title3>No knowledge records found</Title3>
          <Text style={{ color: "#666", display: "block", marginTop: 8 }}>
            Try adjusting your search query or clear active filters.
          </Text>
        </Card>
      ) : (
        <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          {items.map((item) => (
            <Card
              key={item.id}
              style={{ padding: 20, cursor: "pointer" }}
              onClick={() => navigate(`/learning-records/${item.id}`)}
            >
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
                <div>
                  <div style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 8 }}>
                    {getStatusBadge(item)}
                    <Badge appearance="tint">{item.category}</Badge>
                    {item.domain && <Badge appearance="outline">Domain: {item.domain}</Badge>}
                    {item.isAutoPromoted && <Badge color="brand">Auto-Promoted</Badge>}
                    <Badge appearance="outline">v{item.version}</Badge>
                  </div>

                  <Text size={500} weight="semibold">{item.title}</Text>
                  <p style={{ color: "#555", marginTop: 6, lineHeight: "1.4" }}>{item.summary}</p>
                </div>

                <Text size={200} style={{ color: "#888", whiteSpace: "nowrap" }}>
                  Updated {new Date(item.updatedAt).toLocaleDateString()}
                </Text>
              </div>

              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: 12, paddingTop: 12, borderTop: "1px solid #f0f0f0" }}>
                <div style={{ display: "flex", gap: 6, flexWrap: "wrap" }}>
                  {item.tags.map((tag, idx) => (
                    <span key={idx} style={{ background: "#f0f4f8", padding: "2px 8px", borderRadius: 4, fontSize: 12, color: "#475569" }}>
                      #{tag}
                    </span>
                  ))}
                </div>

                {item.externalWorkItemId && (
                  <Text size={200} style={{ color: item.externalWorkItemProvider === "github" ? "#24292f" : "#0078d4" }}>
                    Linked {item.externalWorkItemProvider?.toUpperCase()} #{item.externalWorkItemId}
                  </Text>
                )}
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
