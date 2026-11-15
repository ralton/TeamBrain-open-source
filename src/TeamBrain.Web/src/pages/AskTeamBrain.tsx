import { useState } from "react";
import {
  Button, Input, Textarea, Badge, Card, Text, Title1, Title3,
  Spinner, MessageBar, MessageBarBody, Field,
} from "@fluentui/react-components";
import ReactMarkdown from "react-markdown";
import { queryApi, QueryResult } from "../api/query";
import { feedbackApi } from "../api/feedback";

export function AskTeamBrain() {
  const [question, setQuestion] = useState("");
  const [repoId, setRepoId] = useState("");
  const [domain, setDomain] = useState("");
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<QueryResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleAsk = async () => {
    if (!question.trim()) return;
    setLoading(true);
    setError(null);
    setResult(null);

    try {
      const response = await queryApi.ask({
        question,
        context: repoId ? { repoId } : undefined,
        domain: domain || undefined,
      });
      setResult(response.data);
    } catch {
      setError("Failed to get a response. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: 900, margin: "0 auto", padding: "24px 16px" }}>
      <Title1>Ask TeamBrain</Title1>
      <Text style={{ color: "#666", display: "block", marginBottom: 24 }}>
        Ask an engineering question to find approved patterns, standards, and gold-standard examples.
      </Text>

      <Card style={{ padding: 24, marginBottom: 24 }}>
        <Field label="Your question" required>
          <Textarea
            rows={3}
            placeholder="e.g. I need to add a dropdown that loads all Guarantors on the loan."
            value={question}
            onChange={(_, d) => setQuestion(d.value)}
            style={{ width: "100%" }}
          />
        </Field>
        <div style={{ display: "flex", gap: 16, marginTop: 16, flexWrap: "wrap" }}>
          <Field label="Repository (optional)">
            <Input placeholder="loan-platform-ui" value={repoId} onChange={(_, d) => setRepoId(d.value)} />
          </Field>
          <Field label="Domain (optional)">
            <Input placeholder="Loan Parties" value={domain} onChange={(_, d) => setDomain(d.value)} />
          </Field>
        </div>
        <Button
          appearance="primary"
          onClick={handleAsk}
          disabled={loading || !question.trim()}
          style={{ marginTop: 16 }}
          icon={loading ? <Spinner size="tiny" /> : undefined}
        >
          {loading ? "Searching..." : "Ask TeamBrain"}
        </Button>
      </Card>

      {error && (
        <MessageBar intent="error" style={{ marginBottom: 16 }}>
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}

      {result && <QueryResultPanel result={result} />}
    </div>
  );
}

function QueryResultPanel({ result }: { result: QueryResult }) {
  const statusColor = result.status === "approved_pattern_found" ? "success" : result.status === "partial_match" ? "warning" : "important";

  const [feedbackSubmitted, setFeedbackSubmitted] = useState<boolean>(false);
  const [showCommentInput, setShowCommentInput] = useState<boolean>(false);
  const [commentText, setCommentText] = useState("");
  const [selectedIsHelpful, setSelectedIsHelpful] = useState<boolean | null>(null);

  const handleFeedback = async (isHelpful: boolean) => {
    setSelectedIsHelpful(isHelpful);
    setShowCommentInput(true);
    try {
      await feedbackApi.submit({
        queryLogId: result.queryLogId,
        isHelpful,
      });
      setFeedbackSubmitted(true);
    } catch {
      // Ignore error for offline / dev
    }
  };

  const handleCommentSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedIsHelpful !== null) {
      try {
        await feedbackApi.submit({
          queryLogId: result.queryLogId,
          isHelpful: selectedIsHelpful,
          comment: commentText.trim() || undefined,
        });
      } catch {
        // Ignore error for offline / dev
      }
      setShowCommentInput(false);
    }
  };

  return (
    <div>
      <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 16 }}>
        <Badge color={statusColor} size="large">
          {result.status === "approved_pattern_found" ? "Approved Pattern Found" : result.status === "no_approved_pattern" ? "No Approved Pattern" : "Partial Match"}
        </Badge>
        <Badge color={result.confidence === "high" ? "success" : result.confidence === "medium" ? "warning" : "danger"}>
          Confidence: {result.confidence}
        </Badge>
      </div>

      <Card style={{ padding: 24, marginBottom: 16 }}>
        <ReactMarkdown>{result.answerMarkdown}</ReactMarkdown>

        <div style={{ marginTop: 20, paddingTop: 16, borderTop: "1px solid #f0f0f0", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <Text size={200} style={{ color: "#666" }}>Was this answer helpful?</Text>
          <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
            <Button
              appearance={selectedIsHelpful === true ? "primary" : "outline"}
              size="small"
              onClick={() => handleFeedback(true)}
            >
              👍 Helpful
            </Button>
            <Button
              appearance={selectedIsHelpful === false ? "primary" : "outline"}
              size="small"
              onClick={() => handleFeedback(false)}
            >
              👎 Not Helpful
            </Button>
            {feedbackSubmitted && <Badge color="success">Feedback Recorded!</Badge>}
          </div>
        </div>

        {showCommentInput && (
          <form onSubmit={handleCommentSubmit} style={{ marginTop: 12, display: "flex", gap: 8 }}>
            <Input
              placeholder="Add optional feedback comments for engineering managers/SMEs..."
              value={commentText}
              onChange={e => setCommentText(e.target.value)}
              style={{ flex: 1 }}
            />
            <Button appearance="secondary" type="submit" size="small">Save Comment</Button>
          </form>
        )}
      </Card>

      {result.warnings.length > 0 && (
        <MessageBar intent="warning" style={{ marginBottom: 16 }}>
          <MessageBarBody>
            <strong>Warnings:</strong>
            <ul style={{ margin: "4px 0 0", paddingLeft: 20 }}>
              {result.warnings.map((w, i) => <li key={i}>{w}</li>)}
            </ul>
          </MessageBarBody>
        </MessageBar>
      )}

      {result.suggestedTests.length > 0 && (
        <Card style={{ padding: 16, marginBottom: 16 }}>
          <Title3 style={{ marginBottom: 8 }}>Suggested Tests</Title3>
          <ul style={{ margin: 0, paddingLeft: 20 }}>
            {result.suggestedTests.map((t, i) => <li key={i}>{t}</li>)}
          </ul>
        </Card>
      )}

      {result.sources.length > 0 && (
        <Card style={{ padding: 16 }}>
          <Title3 style={{ marginBottom: 8 }}>Sources</Title3>
          {result.sources.map((s, i) => (
            <div key={i} style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 8 }}>
              <Badge color={s.classification === "GoldStandard" ? "warning" : "informative"}>
                {s.type === "learning_record" ? "Learning Record" : s.classification}
              </Badge>
              {s.url ? (
                <a href={s.url} style={{ color: "#0078d4" }}>{s.title}</a>
              ) : (
                <Text>{s.title}</Text>
              )}
            </div>
          ))}
        </Card>
      )}
    </div>
  );
}
