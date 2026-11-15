import { BrowserRouter, Routes, Route, NavLink } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { FluentProvider, webLightTheme, Text } from "@fluentui/react-components";
import { MsalProvider, AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from "@azure/msal-react";
import { msalInstance, loginRequest } from "./api/msalInstance";
import { Dashboard } from "./pages/Dashboard";
import { AskTeamBrain } from "./pages/AskTeamBrain";
import { ReviewQueue } from "./pages/ReviewQueue";
import { LearningRecordDetail } from "./pages/LearningRecordDetail";
import { ThreadsPage } from "./pages/ThreadsPage";
import { ThreadDetail } from "./pages/ThreadDetail";
import { SeedKnowledgePage } from "./pages/SeedKnowledgePage";
import { KnowledgePage } from "./pages/KnowledgePage";
import { PilotDashboard } from "./pages/PilotDashboard";

const queryClient = new QueryClient();

const navItems = [
  { path: "/", label: "Dashboard" },
  { path: "/ask", label: "Ask TeamBrain" },
  { path: "/learning-records", label: "Knowledge" },
  { path: "/threads", label: "Threads" },
  { path: "/review-queue", label: "Review Queue" },
  { path: "/admin/seed-knowledge", label: "Seed Knowledge" },
  { path: "/admin/pilot-metrics", label: "Pilot Telemetry" },
];

function Shell() {
  const { instance, accounts } = useMsal();

  return (
    <div style={{ display: "flex", minHeight: "100vh", fontFamily: "var(--fontFamilyBase)" }}>
      <nav style={{ width: 220, background: "#0f3d5f", color: "#fff", padding: "24px 0", flexShrink: 0 }}>
        <div style={{ padding: "0 20px 24px", borderBottom: "1px solid rgba(255,255,255,0.15)" }}>
          <Text weight="bold" size={500} style={{ color: "#fff" }}>TeamBrain</Text>
          <br />
          <Text size={200} style={{ color: "rgba(255,255,255,0.6)" }}>Engineering Memory</Text>
        </div>
        <div style={{ padding: "16px 0" }}>
          {navItems.map(item => (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.path === "/"}
              style={({ isActive }) => ({
                display: "block",
                padding: "10px 20px",
                color: isActive ? "#fff" : "rgba(255,255,255,0.7)",
                background: isActive ? "rgba(255,255,255,0.15)" : "transparent",
                textDecoration: "none",
                fontSize: 14,
              })}
            >
              {item.label}
            </NavLink>
          ))}
        </div>
        <div style={{ position: "absolute", bottom: 20, left: 0, width: 220, padding: "0 20px" }}>
          {accounts[0] && (
            <>
              <Text size={200} style={{ color: "rgba(255,255,255,0.6)", display: "block" }}>
                {accounts[0].username}
              </Text>
              <button
                onClick={() => instance.logoutRedirect()}
                style={{ background: "none", border: "none", color: "rgba(255,255,255,0.6)", cursor: "pointer", fontSize: 12, padding: 0, marginTop: 4 }}
              >
                Sign out
              </button>
            </>
          )}
        </div>
      </nav>

      <main style={{ flex: 1, background: "#f5f5f5", overflow: "auto" }}>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/ask" element={<AskTeamBrain />} />
          <Route path="/learning-records" element={<KnowledgePage />} />
          <Route path="/threads" element={<ThreadsPage />} />
          <Route path="/threads/:id" element={<ThreadDetail />} />
          <Route path="/review-queue" element={<ReviewQueue />} />
          <Route path="/learning-records/:id" element={<LearningRecordDetail />} />
          <Route path="/admin/seed-knowledge" element={<SeedKnowledgePage />} />
          <Route path="/admin/pilot-metrics" element={<PilotDashboard />} />
        </Routes>
      </main>
    </div>
  );
}

function SignInPage() {
  const { instance } = useMsal();
  return (
    <div style={{ display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", minHeight: "100vh", background: "#f5f5f5" }}>
      <Text weight="bold" size={800} style={{ marginBottom: 8 }}>TeamBrain</Text>
      <Text style={{ marginBottom: 24, color: "#666" }}>Engineering collective intelligence. Sign in to continue.</Text>
      <button
        onClick={() => instance.loginRedirect(loginRequest)}
        style={{ padding: "12px 28px", background: "#0f3d5f", color: "#fff", border: "none", borderRadius: 4, fontSize: 15, cursor: "pointer" }}
      >
        Sign in with Microsoft
      </button>
    </div>
  );
}

export default function App() {
  return (
    <MsalProvider instance={msalInstance}>
      <FluentProvider theme={webLightTheme}>
        <QueryClientProvider client={queryClient}>
          <BrowserRouter>
            <AuthenticatedTemplate>
              <Shell />
            </AuthenticatedTemplate>
            <UnauthenticatedTemplate>
              <SignInPage />
            </UnauthenticatedTemplate>
          </BrowserRouter>
        </QueryClientProvider>
      </FluentProvider>
    </MsalProvider>
  );
}
