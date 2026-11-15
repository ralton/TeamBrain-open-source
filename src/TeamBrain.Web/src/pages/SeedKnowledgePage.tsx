import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
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
  Field,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  TableCellLayout,
  Dialog,
  DialogTrigger,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogActions,
  DialogContent,
} from "@fluentui/react-components";
import { ingestionJobsApi, IngestionJobSummary, IngestionJobDetail } from "../api/ingestionJobs";

export function SeedKnowledgePage() {
  const queryClient = useQueryClient();
  const [sourceType, setSourceType] = useState("blob_container");
  const [sourceRef, setSourceRef] = useState("");
  const [selectedJobId, setSelectedJobId] = useState<string | null>(null);

  const { data: response, isLoading } = useQuery({
    queryKey: ["ingestion-jobs"],
    queryFn: () => ingestionJobsApi.list(1, 50),
    refetchInterval: 5000, // Poll every 5 seconds for live status
  });

  const { data: detailResponse } = useQuery({
    queryKey: ["ingestion-job", selectedJobId],
    queryFn: () => ingestionJobsApi.get(selectedJobId!),
    enabled: !!selectedJobId,
  });

  const submitMutation = useMutation({
    mutationFn: ingestionJobsApi.submit,
    onSuccess: () => {
      setSourceRef("");
      queryClient.invalidateQueries({ queryKey: ["ingestion-jobs"] });
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (sourceRef.trim()) {
      submitMutation.mutate({
        sourceType,
        sourceReference: sourceRef.trim(),
      });
    }
  };

  const jobs = response?.data.items ?? [];
  const selectedJobDetail: IngestionJobDetail | undefined = detailResponse?.data;

  const getStatusBadge = (status: string) => {
    switch (status.toLowerCase()) {
      case "completed":
        return <Badge color="success">Completed</Badge>;
      case "running":
        return <Badge color="brand">Running</Badge>;
      case "failed":
        return <Badge color="danger">Failed</Badge>;
      default:
        return <Badge color="warning">Pending</Badge>;
    }
  };

  return (
    <div style={{ maxWidth: 1200, margin: "0 auto", padding: "24px 16px" }}>
      <div style={{ marginBottom: 24 }}>
        <Title1>Seed Knowledge & Bulk Ingestion</Title1>
        <p style={{ color: "#666", marginTop: 4 }}>
          Ingest raw documentation, repository source files, or blob containers into TeamBrain's knowledge store and AI Search vector index.
        </p>
      </div>

      <Card style={{ padding: 24, marginBottom: 32 }}>
        <Title3 style={{ marginBottom: 16 }}>Submit New Ingestion Job</Title3>
        <form onSubmit={handleSubmit} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          <div style={{ display: "grid", gridTemplateColumns: "200px 1fr", gap: 16 }}>
            <Field label="Source Type" required>
              <Select value={sourceType} onChange={e => setSourceType(e.target.value)}>
                <option value="blob_container">Azure Blob Container</option>
                <option value="repository">Git Repository</option>
                <option value="documentation">Documentation Set</option>
              </Select>
            </Field>

            <Field label="Source Storage URI / Reference" required>
              <Input
                value={sourceRef}
                onChange={e => setSourceRef(e.target.value)}
                placeholder="e.g. https://teambraindevstorage.blob.core.windows.net/ingestion/seed-docs"
              />
            </Field>
          </div>

          <div style={{ display: "flex", justifyContent: "flex-end" }}>
            <Button
              appearance="primary"
              type="submit"
              disabled={submitMutation.isPending || !sourceRef.trim()}
            >
              {submitMutation.isPending ? "Submitting..." : "Submit Ingestion Job"}
            </Button>
          </div>
        </form>
      </Card>

      <Title3 style={{ marginBottom: 16 }}>Ingestion Job Pipeline History</Title3>

      {isLoading ? (
        <Spinner label="Loading ingestion jobs..." />
      ) : jobs.length === 0 ? (
        <Card style={{ padding: 32, textAlign: "center" }}>
          <Text style={{ color: "#666" }}>No ingestion jobs submitted yet.</Text>
        </Card>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHeaderCell>Job ID</TableHeaderCell>
              <TableHeaderCell>Source Type</TableHeaderCell>
              <TableHeaderCell>Storage Reference</TableHeaderCell>
              <TableHeaderCell>Status</TableHeaderCell>
              <TableHeaderCell>Processed</TableHeaderCell>
              <TableHeaderCell>Quarantined</TableHeaderCell>
              <TableHeaderCell>Submitted At</TableHeaderCell>
              <TableHeaderCell>Actions</TableHeaderCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            {jobs.map((job: IngestionJobSummary) => (
              <TableRow key={job.id}>
                <TableCell>
                  <TableCellLayout>
                    <Text size={200} style={{ fontFamily: "monospace" }}>{job.id.substring(0, 8)}...</Text>
                  </TableCellLayout>
                </TableCell>
                <TableCell>{job.sourceType}</TableCell>
                <TableCell style={{ maxWidth: 300, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                  {job.sourceReference}
                </TableCell>
                <TableCell>{getStatusBadge(job.status)}</TableCell>
                <TableCell>{job.artifactsProcessed}</TableCell>
                <TableCell>{job.artifactsQuarantined}</TableCell>
                <TableCell>{new Date(job.createdAt).toLocaleString()}</TableCell>
                <TableCell>
                  <Button appearance="subtle" onClick={() => setSelectedJobId(job.id)}>
                    View Details
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}

      <Dialog open={!!selectedJobId} onOpenChange={(_, data) => !data.open && setSelectedJobId(null)}>
        <DialogSurface>
          <DialogBody>
            <DialogTitle>Ingestion Job Details</DialogTitle>
            <DialogContent style={{ marginTop: 12 }}>
              {selectedJobDetail ? (
                <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
                  <div>
                    <Text weight="semibold">Job ID: </Text><Text style={{ fontFamily: "monospace" }}>{selectedJobDetail.id}</Text>
                  </div>
                  <div>
                    <Text weight="semibold">Status: </Text>{getStatusBadge(selectedJobDetail.status)}
                  </div>
                  <div>
                    <Text weight="semibold">Source Reference: </Text><Text>{selectedJobDetail.sourceReference}</Text>
                  </div>
                  <div style={{ display: "flex", gap: 24, marginTop: 8 }}>
                    <Card style={{ padding: 12, flex: 1, textAlign: "center" }}>
                      <Text size={500} weight="bold">{selectedJobDetail.artifactsProcessed}</Text>
                      <br />
                      <Text size={200} style={{ color: "#666" }}>Artifacts Processed</Text>
                    </Card>
                    <Card style={{ padding: 12, flex: 1, textAlign: "center" }}>
                      <Text size={500} weight="bold" style={{ color: selectedJobDetail.artifactsQuarantined > 0 ? "#d13438" : "inherit" }}>
                        {selectedJobDetail.artifactsQuarantined}
                      </Text>
                      <br />
                      <Text size={200} style={{ color: "#666" }}>Quarantined / Errors</Text>
                    </Card>
                  </div>

                  {selectedJobDetail.errorMessage && (
                    <div style={{ marginTop: 12, padding: 12, backgroundColor: "#fde8e8", borderRadius: 4 }}>
                      <Text weight="semibold" style={{ color: "#d13438" }}>Pipeline Error:</Text>
                      <br />
                      <Text size={200}>{selectedJobDetail.errorMessage}</Text>
                    </div>
                  )}

                  {selectedJobDetail.errors && selectedJobDetail.errors.length > 0 && (
                    <div style={{ marginTop: 12 }}>
                      <Text weight="semibold">Detailed Error Log:</Text>
                      <div style={{ maxHeight: 200, overflowY: "auto", marginTop: 8, background: "#f8f9fa", padding: 8, borderRadius: 4 }}>
                        {selectedJobDetail.errors.map(err => (
                          <div key={err.id} style={{ borderBottom: "1px solid #eee", padding: "6px 0" }}>
                            <Text size={200} weight="semibold" style={{ color: "#d13438" }}>[{err.errorType}] </Text>
                            <Text size={200}>{err.message}</Text>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              ) : (
                <Spinner label="Loading details..." />
              )}
            </DialogContent>
            <DialogActions style={{ marginTop: 16 }}>
              <Button appearance="primary" onClick={() => setSelectedJobId(null)}>Close</Button>
            </DialogActions>
          </DialogBody>
        </DialogSurface>
      </Dialog>
    </div>
  );
}
