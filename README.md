# TeamBrain

**The Collective Intelligence Platform for Engineering Teams.**

TeamBrain captures reusable knowledge from software delivery work, validates it through human review, stores it as structured team knowledge, and delivers approved guidance back to developers inside their existing workflow.

> **Status:** MVP 1 implementation complete — Phases 0 through 5 (Foundation, Knowledge Core, Async Ingestion Pipeline, Thread Workspace, Seed Knowledge Administration, Pluggable GitHub Integrations, Knowledge Directory, Feedback Loop, and VS Code Extension) implemented. See [what's done](#build-status) and [what's next](#roadmap).

---

## Why TeamBrain

Engineering knowledge is fragmented across individual memory, AI sessions, pull requests, work items, wikis, and Slack. When a developer solves a hard problem, that learning rarely reaches the rest of the team in a reusable, trusted form.

TeamBrain fixes this with a governed learning loop:

```
Developer work → Discovery → TeamBrain Thread → Candidate Learning
→ SME Review → Approved Knowledge → Reuse by every developer
```

It is not a generic chatbot. It is a governed engineering memory and pattern-retrieval system.

---

## Architecture

```
React Web UI (Fluent UI + MSAL)
  └─ TeamBrain API (.NET 10 / ASP.NET Core)
       ├─ Azure SQL          — system of record (EF Core)
       ├─ Azure Key Vault    — secrets
       ├─ Azure Blob Storage — raw source content
       ├─ Azure Service Bus  — async ingestion jobs
       ├─ Azure Functions    — ingestion + indexing pipeline
       ├─ Azure AI Search    — hybrid / vector retrieval
       ├─ GitHub API / REST  — repositories, issues, PRs
       └─ Azure OpenAI       — LLM answers + embeddings
```

**Auth:** Microsoft Entra ID (Azure AD) throughout — API and frontend.

**Integrations:** GitHub repository, issue, and pull request access is implemented. The API also defines pluggable code repository and work item interfaces. Azure DevOps and Asana are referenced in the domain model and UI but do not yet have concrete service implementations. The VS Code extension is implemented; see [its documentation](extensions/vscode/README.md).

---

## Repository Structure

```
/
├── src/
│   ├── TeamBrain.Domain          # Entities, enums, base classes
│   ├── TeamBrain.Application     # CQRS commands & queries (MediatR)
│   ├── TeamBrain.Infrastructure  # EF Core, Azure services, secret scanner
│   ├── TeamBrain.Api             # ASP.NET Core Web API
│   ├── TeamBrain.Ingestion       # Azure Functions ingestion pipeline
│   └── TeamBrain.Web             # React + TypeScript + Vite frontend
├── tests/
│   ├── TeamBrain.Application.Tests
│   ├── TeamBrain.Api.Tests
│   └── TeamBrain.Infrastructure.Tests
├── infra/
│   ├── bicep/                    # Azure Bicep IaC (all resources)
│   └── terraform/                # Terraform alternative (same resources)
├── docs/
│   └── PRD.md                    # Full product requirements document
└── .github/workflows/ci.yml      # CI/CD pipeline
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API |
| Architecture | Clean architecture — Domain / Application / Infrastructure / API |
| ORM | Entity Framework Core + Azure SQL |
| CQRS | MediatR |
| Validation | FluentValidation |
| Auth | Microsoft.Identity.Web (Entra ID / Azure AD) |
| AI | Azure OpenAI (gpt-4o + text-embedding-ada-002) |
| Search | Azure AI Search (hybrid + vector, 1536-dim embeddings) |
| Storage | Azure Blob Storage |
| Messaging | Azure Service Bus |
| Secrets | Azure Key Vault |
| Observability | Application Insights + Log Analytics |
| Frontend | React 18, TypeScript, Vite |
| UI | Microsoft Fluent UI v9 |
| State | TanStack Query |
| Frontend auth | MSAL React (@azure/msal-react) |
| IaC | Azure Bicep / Terraform |
| CI/CD | GitHub Actions |

---

## Getting Started

### Prerequisites

Install these tools before you begin:

| Tool | Version | Install |
|---|---|---|
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| Node.js | 20+ | https://nodejs.org |
| Azure CLI | latest | https://learn.microsoft.com/en-us/cli/azure/install-azure-cli |
| Terraform | 1.8+ | https://developer.hashicorp.com/terraform/install *(Terraform path only)* |
| EF Core CLI tools | latest | `dotnet tool install --global dotnet-ef` |

You also need:
- An Azure subscription with permission to create resources and assign RBAC roles
- An Azure Active Directory (Entra ID) tenant where you can register applications

---

### Step 0 — Entra ID app registrations

TeamBrain uses Entra ID for authentication. You need two app registrations before deploying infrastructure: one for the API and one for the frontend SPA.

#### API app registration

1. Go to **Azure Portal → Microsoft Entra ID → App registrations → New registration**
2. Name it `teambrain-api`, select **Accounts in this organizational directory only**
3. Leave redirect URI blank for now — click **Register**
4. On the Overview page, note the **Application (client) ID** — this is your `client_id`
5. Note the **Directory (tenant) ID** — this is your `tenant_id`
6. Go to **Expose an API → Add a scope**:
   - Application ID URI: accept the default `api://<client-id>`
   - Scope name: `user_impersonation`
   - Who can consent: Admins and users
   - Click **Add scope**

#### Frontend SPA app registration

1. Create another registration named `teambrain-web`
2. Under **Authentication → Add a platform → Single-page application**
3. Add redirect URIs:
   - `http://localhost:5173` (local dev)
   - `https://teambrain-dev-web.azurewebsites.net` (deployed)
4. Under **API permissions → Add a permission → My APIs → teambrain-api**
   - Select the `user_impersonation` scope
5. Note the **Application (client) ID** for the frontend — used in `VITE_ENTRA_CLIENT_ID`

---

### Step 1 — Provision Azure infrastructure

Choose **one** of the two IaC options below. Both provision identical resources.

#### Option A — Bicep

**1a. Log in and create a resource group:**
```bash
az login
az account set --subscription "<your-subscription-id>"
az group create --name teambrain-dev --location australiaeast
```

**1b. Edit the parameters file** (`infra/bicep/parameters/dev.bicepparam`), replacing the placeholder values:

```
__TENANT_ID__          → your Entra ID tenant ID (from Step 0)
__CLIENT_ID__          → your teambrain-api client ID (from Step 0)
__SQL_ADMIN_PASSWORD__ → a strong password for the SQL administrator account
```

**1c. Deploy:**
```bash
az deployment group create \
  --resource-group teambrain-dev \
  --template-file infra/bicep/main.bicep \
  --parameters infra/bicep/parameters/dev.bicepparam
```

**1d. Note the outputs** — the deployment prints `apiUrl` and `keyVaultUri`. You'll need the resource endpoints in Step 2. Run this to retrieve them:
```bash
az deployment group show \
  --resource-group teambrain-dev \
  --name main \
  --query properties.outputs
```

---

#### Option B — Terraform

**1a. Install Terraform** (if you haven't already):

```bash
# macOS (Homebrew)
brew tap hashicorp/tap && brew install hashicorp/tap/terraform

# Linux (apt)
wget -O- https://apt.releases.hashicorp.com/gpg | sudo gpg --dearmor -o /usr/share/keyrings/hashicorp-archive-keyring.gpg
echo "deb [signed-by=/usr/share/keyrings/hashicorp-archive-keyring.gpg] https://apt.releases.hashicorp.com $(lsb_release -cs) main" | sudo tee /etc/apt/sources.list.d/hashicorp.list
sudo apt update && sudo apt install terraform

# Windows (winget)
winget install --id Hashicorp.Terraform
```

Verify: `terraform -version` should show `>= 1.8.0`

**1b. Authenticate the Azure CLI** — Terraform uses your CLI session by default:
```bash
az login
az account set --subscription "<your-subscription-id>"

# Confirm you're targeting the right subscription
az account show --query "{name:name, id:id}"
```

**1c. Populate your variables file:**
```bash
cd infra/terraform
Copy `environments/dev.tfvars` to the git-ignored `terraform.tfvars` file and edit `terraform.tfvars`.
```

Open `terraform.tfvars` and replace the three placeholder values:

```hcl
tenant_id          = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"   # from Step 0
client_id          = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"   # teambrain-api client ID
sql_admin_password = "__SQL_ADMIN_PASSWORD__"                    # min 8 chars, mixed case + symbol
```

> `terraform.tfvars` is git-ignored — your credentials will not be committed.

**1d. Initialise Terraform** — downloads the AzureRM provider (~60 MB):
```bash
terraform init
```

You should see: `Terraform has been successfully initialized!`

**1e. Preview the deployment plan:**
```bash
terraform plan -var-file=terraform.tfvars
```

This prints every resource that will be created — ~28 resources. Review it to confirm the names and region match your expectations. No changes are made at this stage.

**1f. Apply the plan:**
```bash
terraform apply -var-file=terraform.tfvars
```

Type `yes` when prompted. Deployment takes approximately 10–15 minutes. The slowest resources are Azure AI Search and the OpenAI model deployments.

**1g. Read the outputs** — after apply completes, Terraform prints the key endpoints:
```bash
terraform output
```

Example output:
```
api_url                        = "https://teambrain-dev-api.azurewebsites.net"
key_vault_uri                  = "https://teambrain-dev-kv.vault.azure.net/"
openai_endpoint                = "https://teambrain-dev-openai.openai.azure.com/"
search_endpoint                = "https://teambrain-dev-search.search.windows.net"
sql_server_fqdn                = "teambrain-dev-sql.database.windows.net"
storage_blob_endpoint          = "https://teambraindevstorage.blob.core.windows.net/"
app_insights_connection_string = <sensitive>
```

To retrieve the sensitive Application Insights connection string:
```bash
terraform output app_insights_connection_string
```

**Resources provisioned by Terraform:**

| Resource | Name | Notes |
|---|---|---|
| Resource Group | `teambrain-dev-rg` | All resources scoped here |
| Log Analytics Workspace | `teambrain-dev-logs` | 90-day retention |
| Application Insights | `teambrain-dev-ai` | Linked to Log Analytics |
| Key Vault | `teambrain-dev-kv` | RBAC-enabled, soft delete 90 days |
| Storage Account | `teambraindevstorage` | LRS, no public blob access |
| Storage Containers | `artifacts`, `ingestion` | Private, for source content |
| SQL Server | `teambrain-dev-sql` | TLS 1.2, Azure services firewall rule |
| SQL Database | `teambrain` | S1 Standard tier |
| AI Search | `teambrain-dev-search` | Standard SKU, semantic search enabled |
| Azure OpenAI | `teambrain-dev-openai` | S0 SKU |
| OpenAI Deployment | `gpt-4o` | 2024-11-20, 10K TPM |
| OpenAI Deployment | `text-embedding-ada-002` | v2, 10K TPM, 1536-dim |
| Service Bus | `teambrain-dev-sb` | Standard tier |
| Service Bus Queues | `ingestion-jobs`, `indexing-jobs` | 5 max delivery, 5-min lock |
| App Service Plan | `teambrain-dev-plan` | Linux B2 |
| App Service (API) | `teambrain-dev-api` | .NET 10, system-assigned identity |
| RBAC: Key Vault Secrets User | — | API identity → Key Vault |
| RBAC: Cognitive Services OpenAI User | — | API identity → OpenAI |
| RBAC: Search Index Data Contributor | — | API identity → AI Search |
| RBAC: Storage Blob Data Contributor | — | API identity → Storage |
| RBAC: Azure Service Bus Data Owner | — | API identity → Service Bus |

**Tearing down:**
```bash
terraform destroy -var-file=terraform.tfvars
```

---

### Step 2 — Configure the API

Create `src/TeamBrain.Api/appsettings.Development.json` (this file is git-ignored):

```json
{
  "AzureAd": {
    "TenantId": "<tenant-id from Step 0>",
    "ClientId": "<teambrain-api client-id from Step 0>"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:<sql-fqdn>,1433;Database=teambrain;Authentication=Active Directory Default;Encrypt=yes;TrustServerCertificate=no;"
  },
  "AzureOpenAi": {
    "Endpoint": "<openai_endpoint from Step 1 outputs>"
  },
  "AzureAiSearch": {
    "Endpoint": "<search_endpoint from Step 1 outputs>"
  },
  "BlobStorage": {
    "AccountUri": "<storage_blob_endpoint from Step 1 outputs>"
  },
  "ApplicationInsights": {
    "ConnectionString": "<app_insights_connection_string from Step 1 outputs>"
  }
}
```

Replace the `<...>` values with the outputs from Step 1. The SQL connection string uses `Authentication=Active Directory Default` — no password needed locally, as `DefaultAzureCredential` picks up your `az login` session.

---

### Step 3 — Run database migrations

```bash
export PATH="$HOME/.dotnet:$PATH"   # if dotnet is not on your system PATH

dotnet ef migrations add Initial \
  --project src/TeamBrain.Infrastructure \
  --startup-project src/TeamBrain.Api

dotnet ef database update \
  --project src/TeamBrain.Infrastructure \
  --startup-project src/TeamBrain.Api
```

This creates all tables in the `teambrain` database. Confirm success with:
```bash
dotnet ef migrations list \
  --project src/TeamBrain.Infrastructure \
  --startup-project src/TeamBrain.Api
```

---

### Step 4 — Run the API

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet run --project src/TeamBrain.Api
```

The API starts on `https://localhost:7153` (HTTPS) and `http://localhost:5210` (HTTP), as configured in `src/TeamBrain.Api/Properties/launchSettings.json`. Open Swagger at:
```
https://localhost:7153/swagger
```

All endpoints require a Bearer token. Use the Authorize button in Swagger with a token acquired from Entra ID, or test via the frontend in Step 5.

---

### Step 5 — Run the frontend

```bash
cd src/TeamBrain.Web
cp .env.example .env.local
```

Edit `.env.local` with your Entra ID values:

```env
VITE_ENTRA_CLIENT_ID=<teambrain-web client-id from Step 0>
VITE_ENTRA_TENANT_ID=<tenant-id from Step 0>
VITE_API_BASE_URL=https://localhost:7153/api/v1
```

Then install dependencies and start the dev server:

```bash
npm install
npm run dev
```

The app opens at `http://localhost:5173`. Sign in with an account from your Entra ID tenant.

---

## Build Status

### Implemented — Phases 0 through 5

| Area | Status |
|---|---|
| Solution structure & project references | Done |
| Domain entities (LearningRecord, Thread, SourceArtifact, Review, IngestionJob, FeedbackEvent, etc.) | Done |
| Application CQRS — Learning Records (create, review, list, get, update, delete, multi-faceted search) | Done |
| Application CQRS — Threads (create, list, get, update, message, promote-to-learning) | Done |
| Application CQRS — Ingestion Jobs (submit, list, get details) | Done |
| Application CQRS — Feedback Events (`RecordFeedbackCommand` & audit logging) | Done |
| Application CQRS — AI Query (Ask TeamBrain hybrid search & generation) | Done |
| Azure Functions ingestion & search indexing pipeline (`TeamBrain.Ingestion`) | Done |
| Service Bus queue integration (`ingestion-jobs`) | Done |
| EF Core DbContext + entity configurations | Done |
| EF Core migrations | Not checked in; generate an initial migration using Step 3 |
| Azure AI Search service (hybrid retrieval + vector indexer) | Done |
| Azure OpenAI service (chat + embeddings) | Done |
| Blob Storage service | Done |
| Secret scanner (regex-based, pre-storage) | Done |
| Audit service (`AuditEvent` logger) | Done |
| Pluggable integration interfaces (`ICodeRepositoryService`, `IWorkItemService`) | Done |
| GitHub REST integration service (`GitHubService` for Repos, Issues, PRs) | Done |
| ASP.NET Core API — LearningRecords, Threads, IngestionJobs, Query, Integrations, FeedbackEvents controllers | Done |
| Entra ID JWT auth (Microsoft.Identity.Web) | Done |
| Exception handling middleware | Done |
| React frontend shell + MSAL auth | Done |
| Dashboard page | Done |
| Ask TeamBrain page + Helpful/Not Helpful sentiment controls | Done |
| Knowledge Directory & Search page (`KnowledgePage`) | Done |
| Developer pilot telemetry & metrics dashboard (`PilotDashboard`) | Done |
| Review Queue page | Done |
| Learning Record detail page | Done |
| Threads workspace & thread detail pages (`ThreadsPage`, `ThreadDetail`) | Done |
| Seed Knowledge & bulk ingestion admin page (`SeedKnowledgePage`) | Done |
| Pluggable External Work Item & GitHub PR linking modal dialog | Done |
| **VS Code Extension (`extensions/vscode`)** — Sidebar Ask panel, Right-click Submit Selection, Quick Pick pattern search, Hover advisories, independent `.vsix` packaging support, prerequisite scripts (`scripts/setup-extension-prereqs.ps1`/`.sh`), and complete documentation ([extensions/vscode/README.md](extensions/vscode/README.md)) | Done |
| Azure Bicep & Terraform IaC — all resource modules | Done |
| GitHub Actions CI/CD pipeline | Done |

### Roadmap — Future phases

| Phase | Area | Key work |
|---|---|---|
| Phase 6 | PR advisory bot | GitHub / ADO PR bot with approved pattern warnings |

---

## Core Concepts

| Concept | Description |
|---|---|
| **Learning Record** | A governed knowledge object: approved pattern, architecture decision, code pattern, testing standard, anti-pattern, etc. |
| **Thread** | A persistent workspace for one engineering problem, feature, or investigation — links to ADO, Asana, source files. |
| **Source Artifact** | Evidence: source file, PR, ADO work item, wiki page, README, Asana task. |
| **Knowledge Status** | `candidate → in_review → approved` (or `rejected / legacy / deprecated / archived`). Only `approved` records are returned as standards. |
| **Gold Standard** | A source file or PR explicitly marked as the best current reference for a pattern. |
| **Anti-Pattern** | A known approach that must not be used for new work. |

---

## Security Notes

- Azure Key Vault is provisioned for secrets. Local development configuration uses the git-ignored `appsettings.Development.json`; configure deployed secrets and connection values in the hosting environment or Key Vault before deployment.
- Secret scanner runs on every source artifact before storage or indexing.
- API endpoints use Entra ID bearer-token authentication. Fine-grained role-based authorization should be configured and verified for each deployment.
- Search results filtered by repository and team permissions.
- Quarantine flow: secret detected → content redacted → artifact flagged → admin notified → never indexed.

---

## Documentation

- [Product Requirements Document — MVP 1](docs/PRD.md)

## License

TeamBrain is distributed under the MIT License. See [LICENSE](LICENSE).

