import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  Card,
  Title1,
  Title3,
  Badge,
  Button,
  Text,
  Spinner,
  Textarea,
  Input,
  Select,
  Dialog,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogContent,
  DialogActions,
  Field,
} from "@fluentui/react-components";
import { threadsApi } from "../api/threads";
import { integrationsApi } from "../api/integrations";

export function ThreadDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [newMessage, setNewMessage] = useState("");
  const [isEditing, setIsEditing] = useState(false);

  const { data: response, isLoading, error } = useQuery({
    queryKey: ["thread", id],
    queryFn: () => threadsApi.get(id!),
    enabled: !!id,
  });

  const thread = response?.data;

  const [editTitle, setEditTitle] = useState("");
  const [editStatus, setEditStatus] = useState("");

  const startEdit = () => {
    if (thread) {
      setEditTitle(thread.title);
      setEditStatus(thread.status);
      setIsEditing(true);
    }
  };

  const updateMutation = useMutation({
    mutationFn: (data: { title?: string; status?: string }) => threadsApi.update(id!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["thread", id] });
      setIsEditing(false);
    },
  });

  const addMessageMutation = useMutation({
    mutationFn: (content: string) => threadsApi.addMessage(id!, content),
    onSuccess: () => {
      setNewMessage("");
      queryClient.invalidateQueries({ queryKey: ["thread", id] });
    },
  });

  const promoteMutation = useMutation({
    mutationFn: () => threadsApi.promoteLearning(id!),
    onSuccess: (res) => {
      navigate(`/review-queue/${res.data.learningRecordId}`);
    },
  });

  const handleAddMessage = (e: React.FormEvent) => {
    e.preventDefault();
    if (newMessage.trim()) {
      addMessageMutation.mutate(newMessage.trim());
    }
  };

  const handleSaveUpdate = () => {
    updateMutation.mutate({
      title: editTitle.trim() || undefined,
      status: editStatus || undefined,
    });
  };

  const [isLinkOpen, setIsLinkOpen] = useState(false);
  const [linkProvider, setLinkProvider] = useState<"github" | "ado" | "asana">("github");
  const [linkItemId, setLinkItemId] = useState("");
  const [linkUrl, setLinkUrl] = useState("");

  const linkMutation = useMutation({
    mutationFn: (data: { provider: "github" | "ado" | "asana"; workItemId: string; workItemUrl: string }) =>
      integrationsApi.linkWorkItem({
        targetId: id!,
        targetType: "thread",
        provider: data.provider,
        workItemId: data.workItemId,
        workItemUrl: data.workItemUrl,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["thread", id] });
      setIsLinkOpen(false);
      setLinkItemId("");
      setLinkUrl("");
    },
  });

  const handleLinkSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (linkItemId.trim()) {
      linkMutation.mutate({
        provider: linkProvider,
        workItemId: linkItemId.trim(),
        workItemUrl: linkUrl.trim() || `https://github.com/issues/${linkItemId.trim()}`,
      });
    }
  };

  if (isLoading) return <Spinner label="Loading thread details..." />;
  if (error || !thread) return <Card style={{ padding: 24 }}><Title3>Thread not found</Title3></Card>;

  return (
    <div style={{ maxWidth: 1000, margin: "0 auto", padding: "24px 16px" }}>
      <Button appearance="subtle" onClick={() => navigate("/threads")} style={{ marginBottom: 16 }}>
        ← Back to Threads
      </Button>

      <Card style={{ padding: 24, marginBottom: 24 }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
          <div style={{ flex: 1, marginRight: 16 }}>
            <div style={{ display: "flex", gap: 8, alignItems: "center", marginBottom: 8 }}>
              <Badge color="brand">{thread.threadType}</Badge>
              <Badge appearance="outline">{thread.status}</Badge>
              {thread.repoId && <Badge appearance="tint">Repo: {thread.repoId}</Badge>}
              {thread.branchName && <Badge appearance="tint">Branch: {thread.branchName}</Badge>}
            </div>

            {isEditing ? (
              <div style={{ display: "flex", flexDirection: "column", gap: 12, marginTop: 8 }}>
                <Input value={editTitle} onChange={e => setEditTitle(e.target.value)} placeholder="Thread Title" />
                <Select value={editStatus} onChange={e => setEditStatus(e.target.value)}>
                  <option value="active">Active</option>
                  <option value="resolved">Resolved</option>
                  <option value="archived">Archived</option>
                </Select>
                <div style={{ display: "flex", gap: 8 }}>
                  <Button appearance="primary" onClick={handleSaveUpdate} disabled={updateMutation.isPending}>Save</Button>
                  <Button appearance="secondary" onClick={() => setIsEditing(false)}>Cancel</Button>
                </div>
              </div>
            ) : (
              <div>
                <Title1>{thread.title}</Title1>
                {thread.summary && (
                  <p style={{ color: "#555", marginTop: 8 }}>{thread.summary}</p>
                )}
              </div>
            )}
          </div>

          <div style={{ display: "flex", gap: 8 }}>
            {!isEditing && (
              <Button appearance="outline" onClick={startEdit}>Edit</Button>
            )}
            <Button appearance="outline" onClick={() => setIsLinkOpen(true)}>Link Work Item</Button>
            <Button
              appearance="primary"
              onClick={() => promoteMutation.mutate()}
              disabled={promoteMutation.isPending}
            >
              {promoteMutation.isPending ? "Promoting..." : "Promote to Candidate Learning"}
            </Button>
          </div>
        </div>

        <div style={{ display: "flex", gap: 24, marginTop: 16, paddingTop: 12, borderTop: "1px solid #eee", alignItems: "center" }}>
          {thread.adoWorkItemId && (
            <Text style={{ color: "#0078d4" }}>
              <strong>ADO Work Item:</strong> #{thread.adoWorkItemId}
            </Text>
          )}
          {thread.asanaTaskId && (
            <Text style={{ color: "#e83e8c" }}>
              <strong>Asana Task:</strong> {thread.asanaTaskId}
            </Text>
          )}
          {(!thread.adoWorkItemId && !thread.asanaTaskId) && (
            <Text size={200} style={{ color: "#888" }}>
              No external issue linked yet. Click <strong>Link Work Item</strong> to attach a GitHub Issue or PR.
            </Text>
          )}
        </div>
      </Card>

      <Title3 style={{ marginBottom: 16 }}>Conversation</Title3>

      <div style={{ display: "flex", flexDirection: "column", gap: 16, marginBottom: 24 }}>
        {thread.messages.length === 0 ? (
          <Card style={{ padding: 20, textAlign: "center", color: "#666" }}>
            No messages in this thread yet. Post the first message below.
          </Card>
        ) : (
          thread.messages.map((msg) => (
            <Card
              key={msg.id}
              style={{
                padding: 16,
                backgroundColor: msg.role === "assistant" ? "#f5f7fa" : "#ffffff",
                borderLeft: msg.role === "assistant" ? "4px solid #0078d4" : "1px solid #e0e0e0",
              }}
            >
              <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 8 }}>
                <Text weight="semibold" style={{ color: msg.role === "assistant" ? "#0078d4" : "#333" }}>
                  {msg.role === "assistant" ? "TeamBrain Assistant" : "Developer"}
                </Text>
                <Text size={200} style={{ color: "#888" }}>
                  {new Date(msg.createdAt).toLocaleString()}
                </Text>
              </div>
              <div style={{ whiteSpace: "pre-wrap", lineHeight: "1.5" }}>
                <Text>{msg.content}</Text>
              </div>
            </Card>
          ))
        )}
      </div>

      <Card style={{ padding: 20 }}>
        <Title3 style={{ marginBottom: 12 }}>Add Message</Title3>
        <form onSubmit={handleAddMessage}>
          <Textarea
            value={newMessage}
            onChange={e => setNewMessage(e.target.value)}
            placeholder="Type your message or solution details..."
            rows={4}
            style={{ width: "100%", marginBottom: 12 }}
          />
          <Button
            appearance="primary"
            type="submit"
            disabled={addMessageMutation.isPending || !newMessage.trim()}
          >
            {addMessageMutation.isPending ? "Sending..." : "Send Message"}
          </Button>
        </form>
      </Card>

      <Dialog open={isLinkOpen} onOpenChange={(_, data) => setIsLinkOpen(data.open)}>
        <DialogSurface>
          <form onSubmit={handleLinkSubmit}>
            <DialogBody>
              <DialogTitle>Link External Work Item / PR</DialogTitle>
              <DialogContent style={{ display: "flex", flexDirection: "column", gap: 12, marginTop: 12 }}>
                <Field label="Integration Provider">
                  <Select value={linkProvider} onChange={e => setLinkProvider(e.target.value as any)}>
                    <option value="github">GitHub Issue / PR</option>
                    <option value="ado">Azure DevOps Work Item</option>
                    <option value="asana">Asana Task</option>
                  </Select>
                </Field>

                <Field label="Item ID / Number" required>
                  <Input value={linkItemId} onChange={e => setLinkItemId(e.target.value)} placeholder="e.g. 42 or PR-105" />
                </Field>

                <Field label="Direct URL (Optional)">
                  <Input value={linkUrl} onChange={e => setLinkUrl(e.target.value)} placeholder="e.g. https://github.com/my-org/my-repo/issues/42" />
                </Field>
              </DialogContent>
              <DialogActions style={{ marginTop: 16 }}>
                <Button appearance="secondary" onClick={() => setIsLinkOpen(false)}>Cancel</Button>
                <Button appearance="primary" type="submit" disabled={linkMutation.isPending || !linkItemId.trim()}>
                  {linkMutation.isPending ? "Linking..." : "Link Item"}
                </Button>
              </DialogActions>
            </DialogBody>
          </form>
        </DialogSurface>
      </Dialog>
    </div>
  );
}
