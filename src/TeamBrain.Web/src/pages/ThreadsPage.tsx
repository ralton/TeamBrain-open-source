import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
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
  Dialog,
  DialogTrigger,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogActions,
  DialogContent,
  Field,
} from "@fluentui/react-components";
import { threadsApi } from "../api/threads";

export function ThreadsPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [isOpen, setIsOpen] = useState(false);
  const [filterType, setFilterType] = useState<string>("all");

  const [newTitle, setNewTitle] = useState("");
  const [newType, setNewType] = useState("Standard");
  const [newRepo, setNewRepo] = useState("");
  const [newBranch, setNewBranch] = useState("");
  const [newAdoId, setNewAdoId] = useState("");
  const [newAsanaId, setNewAsanaId] = useState("");

  const { data: response, isLoading } = useQuery({
    queryKey: ["threads"],
    queryFn: () => threadsApi.list(1, 50),
  });

  const createMutation = useMutation({
    mutationFn: threadsApi.create,
    onSuccess: (res) => {
      queryClient.invalidateQueries({ queryKey: ["threads"] });
      setIsOpen(false);
      resetForm();
      navigate(`/threads/${res.data.id}`);
    },
  });

  const resetForm = () => {
    setNewTitle("");
    setNewType("Standard");
    setNewRepo("");
    setNewBranch("");
    setNewAdoId("");
    setNewAsanaId("");
  };

  const handleCreate = (e: React.FormEvent) => {
    e.preventDefault();
    createMutation.mutate({
      title: newTitle,
      threadType: newType,
      repoId: newRepo || undefined,
      branchName: newBranch || undefined,
      adoWorkItemId: newAdoId || undefined,
      asanaTaskId: newAsanaId || undefined,
    });
  };

  const items = response?.data.items ?? [];
  const filteredItems = filterType === "all"
    ? items
    : items.filter(t => t.threadType.toLowerCase() === filterType.toLowerCase());

  return (
    <div style={{ maxWidth: 1200, margin: "0 auto", padding: "24px 16px" }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 24 }}>
        <div>
          <Title1>Engineering Threads</Title1>
          <p style={{ color: "#666", marginTop: 4 }}>
            Collaborative knowledge capture threads for pattern exploration, debugging, and architecture decisions.
          </p>
        </div>
        <Dialog open={isOpen} onOpenChange={(_, data) => setIsOpen(data.open)}>
          <DialogTrigger disableButtonEnhancement>
            <Button appearance="primary" onClick={() => setIsOpen(true)}>Create Thread</Button>
          </DialogTrigger>
          <DialogSurface>
            <form onSubmit={handleCreate}>
              <DialogBody>
                <DialogTitle>Create New Thread</DialogTitle>
                <DialogContent style={{ display: "flex", flexDirection: "column", gap: 12, marginTop: 12 }}>
                  <Field label="Thread Title" required>
                    <Input value={newTitle} onChange={e => setNewTitle(e.target.value)} placeholder="e.g. Implement Azure Blob Upload Retry Pattern" />
                  </Field>

                  <Field label="Thread Type">
                    <Select value={newType} onChange={e => setNewType(e.target.value)}>
                      <option value="Standard">Standard</option>
                      <option value="Debug">Debug / Troubleshooting</option>
                      <option value="Architecture">Architecture Decision</option>
                      <option value="AdHoc">Ad-Hoc Discovery</option>
                    </Select>
                  </Field>

                  <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                    <Field label="Repository ID">
                      <Input value={newRepo} onChange={e => setNewRepo(e.target.value)} placeholder="e.g. TeamBrain" />
                    </Field>
                    <Field label="Branch Name">
                      <Input value={newBranch} onChange={e => setNewBranch(e.target.value)} placeholder="e.g. main" />
                    </Field>
                  </div>

                  <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                    <Field label="ADO Work Item ID">
                      <Input value={newAdoId} onChange={e => setNewAdoId(e.target.value)} placeholder="e.g. 10452" />
                    </Field>
                    <Field label="Asana Task ID">
                      <Input value={newAsanaId} onChange={e => setNewAsanaId(e.target.value)} placeholder="e.g. 12049" />
                    </Field>
                  </div>
                </DialogContent>
                <DialogActions style={{ marginTop: 16 }}>
                  <Button appearance="secondary" onClick={() => setIsOpen(false)}>Cancel</Button>
                  <Button appearance="primary" type="submit" disabled={createMutation.isPending || !newTitle.trim()}>
                    {createMutation.isPending ? "Creating..." : "Create Thread"}
                  </Button>
                </DialogActions>
              </DialogBody>
            </form>
          </DialogSurface>
        </Dialog>
      </div>

      <div style={{ display: "flex", gap: 12, marginBottom: 20 }}>
        <Select value={filterType} onChange={e => setFilterType(e.target.value)}>
          <option value="all">All Types</option>
          <option value="Standard">Standard</option>
          <option value="Debug">Debug</option>
          <option value="Architecture">Architecture</option>
          <option value="AdHoc">AdHoc</option>
        </Select>
      </div>

      {isLoading ? (
        <Spinner label="Loading threads..." />
      ) : filteredItems.length === 0 ? (
        <Card style={{ padding: 32, textAlign: "center" }}>
          <Title3>No engineering threads found</Title3>
          <Text style={{ color: "#666", display: "block", marginTop: 8 }}>
            Start a new thread to capture findings, debug steps, and architecture recommendations.
          </Text>
        </Card>
      ) : (
        <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
          {filteredItems.map((thread) => (
            <Card
              key={thread.id}
              style={{ padding: 16, cursor: "pointer" }}
              onClick={() => navigate(`/threads/${thread.id}`)}
            >
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
                <div>
                  <div style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 6 }}>
                    <Badge color={thread.status === "active" ? "brand" : "important"}>
                      {thread.threadType}
                    </Badge>
                    <Badge appearance="outline">{thread.status}</Badge>
                    {thread.repoId && (
                      <Text size={200} style={{ color: "#666" }}>Repo: {thread.repoId}</Text>
                    )}
                  </div>
                  <Text size={500} weight="semibold">{thread.title}</Text>
                </div>
                <Text size={200} style={{ color: "#888" }}>
                  Updated {new Date(thread.updatedAt || thread.createdAt).toLocaleDateString()}
                </Text>
              </div>

              {(thread.adoWorkItemId || thread.asanaTaskId) && (
                <div style={{ display: "flex", gap: 16, marginTop: 12, paddingTop: 8, borderTop: "1px solid #f0f0f0" }}>
                  {thread.adoWorkItemId && (
                    <Text size={200} style={{ color: "#0078d4" }}>ADO #{thread.adoWorkItemId}</Text>
                  )}
                  {thread.asanaTaskId && (
                    <Text size={200} style={{ color: "#e83e8c" }}>Asana Task {thread.asanaTaskId}</Text>
                  )}
                </div>
              )}
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
