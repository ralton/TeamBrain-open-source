# TeamBrain PRD — MVP 1

## Product Requirements Document for AI-Assisted Implementation

| Field   | Value                        |
| ------- | ---------------------------- |
| Version | 1.0                          |
| Status  | Draft                        |
| Date    | 2026-06-21                   |
| Owner   | TeamBrain Contributors    |

---

## 1. Product Name

TeamBrain

Working description:

TeamBrain is a governed engineering learning platform that captures reusable knowledge from software delivery work, validates it through human review, stores it as structured team knowledge, and delivers approved guidance back to developers inside their existing workflow.

Primary integrations:

* VS Code
* Azure DevOps (ADO)
* Asana
* Azure
* Git repositories hosted in ADO initially

## 2. Problem Statement

Engineering knowledge is currently fragmented across:

* Individual developer memory
* VS Code AI sessions
* Azure DevOps pull requests
* ADO work items
* Asana tasks
* Wiki pages
* README files
* Slack/Teams conversations
* Senior developer experience
* Legacy code patterns

When a developer discovers the correct way to solve a technical problem, that learning is often not captured, reviewed, or made reusable by the rest of the team.

This creates repeated work, inconsistent implementations, slower onboarding, poor reuse of approved patterns, and code reviews that repeatedly enforce the same standards.

Example:

A developer needs to add a dropdown containing all Guarantors on a loan. This has already been implemented in the codebase using an approved service-layer pattern, shared mapper, and standard UI component. Without TeamBrain, the developer may not find the correct example, may copy legacy code, or may create a new inconsistent implementation.

TeamBrain should allow the developer to ask:

"I need to add a dropdown that loads all Guarantors on the loan."

TeamBrain should respond with:

* Approved team pattern
* Relevant business/domain context
* Gold-standard source files
* Legacy examples to avoid
* Implementation guidance
* Suggested tests
* Source links and confidence
* Optional generated patch or implementation plan

## 3. Product Vision

TeamBrain turns individual engineering discoveries into reusable organizational intelligence.

The core learning loop is:

Developer work
→ Discovery
→ TeamBrain thread
→ Candidate learning
→ SME review
→ Approved knowledge
→ Reuse by every developer
→ Better future work

TeamBrain is not a generic chatbot and is not a replacement for source control, ADO, or documentation.

It is a governed engineering memory and pattern-retrieval system.

## 4. Primary Users

### Developer

Needs fast answers while coding.

Primary actions:

* Search approved patterns
* Ask implementation questions
* Find canonical examples
* Submit new learning
* Create a persistent engineering thread
* View recommended tests
* View legacy warnings
* Generate an implementation plan or patch

### Senior Developer / Subject Matter Expert

Needs lightweight governance.

Primary actions:

* Review candidate learnings
* Approve, reject, revise, or mark as legacy
* Identify gold-standard files
* Mark anti-patterns
* Set ownership and expiration dates
* Review evidence from source code, PRs, and ADO work items

### Engineering Manager / Technical Program Manager

Needs organizational visibility.

Primary actions:

* Track knowledge gaps
* Monitor adoption
* Review learning throughput
* Identify recurring code review feedback
* Measure onboarding and reuse
* View high-risk or stale standards

### Platform Administrator

Needs secure operations.

Primary actions:

* Configure integrations
* Manage Entra ID roles
* Manage model routing
* Configure repositories and source access
* Configure retention policies
* Monitor ingestion and indexing jobs

## 5. MVP Goal

Build a usable MVP that enables the team to:

1. Create persistent TeamBrain engineering threads.
2. Submit and review candidate learning records.
3. Store approved standards as structured knowledge.
4. Search approved standards and gold-standard code examples.
5. Link knowledge to ADO work items, pull requests, source files, and Asana tasks.
6. Seed the system from existing documentation and manually curated code patterns.
7. Provide an initial web UI and API before building the VS Code extension.
8. Maintain strict separation between approved knowledge, unreviewed candidates, legacy references, and anti-patterns.

## 6. Non-Goals for MVP

Do not build these in MVP 1:

* Automatic code changes directly in production branches
* Fully autonomous agents
* Real-time multiplayer code editing
* Replacement for Azure DevOps
* Replacement for Asana
* Full codebase semantic understanding across every repository
* Automatic approval of standards
* Full enterprise-wide document ingestion
* Automatic agent-created pull requests without human approval
* Fine-tuning custom models
* Public-facing customer access

## 7. Core Product Principles

### 7.1 Approved knowledge is different from similar code

TeamBrain must distinguish:

* Approved pattern
* Candidate pattern
* Gold-standard example
* Legacy reference
* Deprecated pattern
* Anti-pattern
* Unknown / unverified code

The system must never present random code as an approved standard.

### 7.2 Human governance is required

AI can suggest and summarize knowledge.

Humans approve standards.

### 7.3 Every answer must be traceable

Every TeamBrain answer must include:

* Confidence level
* Knowledge status
* Source records
* Source files
* Related pull requests, work items, or documentation when available
* Date last reviewed

### 7.4 TeamBrain should be workflow-native

The long-term goal is delivery in:

* VS Code
* ADO PRs
* ADO work items
* Asana
* TeamBrain web UI

MVP delivery can begin with the TeamBrain web UI and API.

### 7.5 TeamBrain must not spread secrets

Never ingest or expose:

* Passwords
* API keys
* Connection strings
* Private certificates
* Tokens
* .env secrets
* Azure Key Vault secrets
* local.settings.json secret values
* Production database credentials

## 8. Core Concepts

### 8.1 TeamBrain Thread

A persistent workspace for one engineering problem, feature, investigation, decision, incident, or pattern discovery.

Examples:

* Add Guarantor dropdown to Loan Details screen
* Investigate failed Azure Function deployment
* Review approved approach for loan-party API access
* Research correct validation pattern for borrower data
* Document production incident and root cause

Allowed `thread_type` values:

```text
feature
investigation
decision
incident
pattern_discovery
onboarding
architecture_review
```

Thread fields:

```json
{
  "id": "thread-uuid",
  "title": "Add Guarantor dropdown to Loan Details",
  "thread_type": "feature",
  "status": "active",
  "repo_id": "loan-platform-ui",
  "branch_name": "feature/guarantor-dropdown",
  "ado_work_item_id": "ADO-18422",
  "asana_task_id": "asana-task-id",
  "created_by_user_id": "user-id",
  "participants": ["user-id"],
  "summary": "Developer needs to add a Guarantor dropdown using the approved pattern.",
  "findings": [],
  "source_artifacts": [],
  "recommended_learning_record_ids": [],
  "created_at": "ISO-8601",
  "updated_at": "ISO-8601"
}
```

### 8.2 Learning Record

A structured knowledge object representing a reusable standard, pattern, decision, anti-pattern, troubleshooting solution, or domain rule.

Example categories:

* Engineering Standard
* Architecture Decision
* Code Pattern
* UI Pattern
* API Pattern
* Testing Standard
* Security Rule
* Azure / DevOps Procedure
* Troubleshooting Fix
* Domain Knowledge
* Incident Lesson
* Anti-Pattern
* Legacy Reference

Learning Record fields:

```json
{
  "id": "learning-uuid",
  "title": "Approved Pattern for Loading Guarantors into Loan Dropdowns",
  "summary": "Use the approved loan-party service and shared dropdown mapper when loading guarantors into loan-related UI dropdowns.",
  "body_markdown": "Detailed explanation and implementation guidance.",
  "category": "UI Pattern",
  "domain": "Loan Parties",
  "systems": ["Loan Platform UI", "Loan Party Service"],
  "languages": ["TypeScript", "React"],
  "status": "approved",
  "confidence": "high",
  "owner_team": "Loan Platform",
  "owner_user_id": "user-id",
  "approved_by_user_id": "user-id",
  "created_by_user_id": "user-id",
  "reviewed_at": "ISO-8601",
  "approved_at": "ISO-8601",
  "expires_at": "ISO-8601",
  "version": 1,
  "source_artifact_ids": [],
  "gold_example_ids": [],
  "legacy_example_ids": [],
  "anti_pattern_ids": [],
  "tags": ["guarantor", "loan", "dropdown", "react", "service-layer"],
  "applies_to": ["loan-ui", "participant-selection"],
  "created_at": "ISO-8601",
  "updated_at": "ISO-8601"
}
```

### 8.3 Source Artifact

A source item that provides evidence for a learning record or thread.

Supported MVP source types:

* Source code file
* ADO pull request
* ADO work item
* ADO Wiki page
* Markdown file
* README
* Manual note
* Asana task
* Architecture decision record

```json
{
  "id": "artifact-uuid",
  "artifact_type": "source_code_file",
  "title": "LoanContactsPanel.tsx",
  "url": "https://dev.azure.com/...",
  "repo_id": "loan-platform-ui",
  "file_path": "/src/features/loans/LoanContactsPanel.tsx",
  "branch_name": "main",
  "commit_sha": "abc123",
  "content_hash": "sha256",
  "classification": "gold_standard",
  "content_storage_uri": "blob://...",
  "created_at": "ISO-8601",
  "updated_at": "ISO-8601"
}
```

### 8.4 Knowledge Status

Allowed statuses:

```text
candidate
in_review
approved
rejected
legacy
deprecated
archived
```

Rules:

* Only `approved` records are returned as recommended team standards.
* `legacy` records can be shown only as warnings or historical reference.
* `deprecated` records should be shown only when users are working in legacy code or ask for migration guidance.
* `candidate` records are visible only to contributors, reviewers, and administrators.
* `rejected` records remain stored for audit history but are never recommended.

### 8.5 Gold Standard Example

A source file, PR, or implementation that has been explicitly marked as the best current reference for a given pattern.

### 8.6 Anti-Pattern

A known approach that should not be used for new work.

Example:

"Do not call loan-party APIs directly from React components when an approved service-layer method exists."

## 9. User Stories

### US-001: Search approved pattern

As a developer, I want to search for an approved implementation pattern so I can build new functionality using team standards.

Acceptance criteria:

* User can enter a natural-language question.
* System searches approved Learning Records first.
* Response includes approved pattern, confidence, sources, and related examples.
* Response distinguishes approved knowledge from similar but unapproved code.
* Response includes a "last reviewed" date.

### US-002: Ask within a TeamBrain thread

As a developer, I want a persistent thread for a feature or investigation so that context is preserved across days and between team members.

Acceptance criteria:

* User can create a thread.
* User can associate a thread with repo, branch, ADO work item, and Asana task.
* Thread stores user prompts, assistant summaries, findings, sources, and linked records.
* User can reopen the thread later.
* User can invite or add teammates.
* User can mark a thread as ready for learning promotion.

### US-003: Submit candidate learning

As a developer, I want to submit a reusable discovery so that the team can benefit from it.

Acceptance criteria:

* User can create a candidate from manual entry or existing thread.
* System creates a candidate Learning Record.
* System requires category, summary, source evidence, and domain.
* System can AI-generate a draft title, summary, tags, and implementation notes.
* Candidate is routed to an assigned reviewer or team owner.

### US-004: Review candidate learning

As an SME, I want to review a candidate record so that only correct patterns become approved standards.

Acceptance criteria:

* Reviewer can approve, reject, request changes, mark as legacy, or mark as anti-pattern.
* Reviewer can edit summary, body, category, tags, owner, confidence, and expiration date.
* Reviewer can select gold-standard and legacy examples.
* All review decisions are audited.
* Approval changes status to `approved`.

### US-005: Search code examples

As a developer, I want to find gold-standard code examples related to a pattern so I can implement it consistently.

Acceptance criteria:

* Search can filter by repo, language, domain, category, and status.
* Results include source file, path, snippet, branch, commit, and classification.
* Gold-standard examples rank above normal examples.
* Legacy examples are visually labeled.
* Source links open the relevant ADO file or pull request.

### US-006: Seed existing knowledge

As a TeamBrain administrator, I want to ingest existing approved docs and manually curated patterns so that the platform is useful on day one.

Acceptance criteria:

* Admin can ingest Markdown, README, ADO Wiki content, and manually uploaded source records.
* Admin can label imported material as approved, candidate, legacy, or deprecated.
* Import pipeline strips or quarantines secrets.
* Imported content is chunked and indexed for retrieval.
* Imported content remains linked to its source artifact.

### US-007: Create ADO review task / Asana task

As a reviewer, I want candidate learning records to create or link to an Asana review task so that governance happens in an existing work-management workflow.

Acceptance criteria:

* New candidate can create an Asana task through configured integration.
* Asana task includes candidate title, summary, sources, owner, and review status.
* TeamBrain stores returned Asana task ID.
* Updating decision in TeamBrain updates Asana status when integration is enabled.
* Integration failure does not prevent the record from being saved.

### US-008: Answer the Guarantor dropdown question

As a new developer, I want TeamBrain to guide me to the approved Guarantor dropdown pattern.

Example prompt:

"I need to add a dropdown that loads all Guarantors on the loan."

Expected response format:

```text
Approved pattern found: Yes
Confidence: High
Status: Approved

Use:
- Approved loan-party service
- Shared dropdown option mapper
- Standard dropdown component

Gold examples:
- File path and link
- File path and link

Avoid:
- Legacy direct API call pattern
- Manual mapping when shared mapper exists

Suggested tests:
- Loads guarantors for a loan
- Handles zero guarantors
- Handles service error
- Persists selected guarantor

Sources:
- Learning Record
- PR
- ADO Work Item
- Source files
```

## 10. Functional Requirements

### 10.1 Authentication and authorization

Use Microsoft Entra ID / Azure AD authentication.

Roles:

```text
reader
contributor
reviewer
admin
service_account
```

Permissions:

| Action                    | Reader | Contributor | Reviewer | Admin |
| ------------------------- | -----: | ----------: | -------: | ----: |
| Search approved knowledge |    Yes |         Yes |      Yes |   Yes |
| View gold examples        |    Yes |         Yes |      Yes |   Yes |
| Create thread             |    Yes |         Yes |      Yes |   Yes |
| Add findings to thread    |     No |         Yes |      Yes |   Yes |
| Create candidate learning |     No |         Yes |      Yes |   Yes |
| Approve learning          |     No |          No |      Yes |   Yes |
| Mark legacy/deprecated    |     No |          No |      Yes |   Yes |
| Configure integrations    |     No |          No |       No |   Yes |
| View audit logs           |     No |          No |  Own decisions only |   Yes |

Note: Reviewers can view audit events for records they have reviewed or that are assigned to them. Admins have unrestricted access to all audit logs.

### 10.2 Knowledge query service

The query service must:

1. Search approved Learning Records first.
2. Search gold-standard artifacts second.
3. Search candidate or legacy records only if allowed and clearly labeled.
4. Use hybrid retrieval:

   * Keyword search
   * Metadata filtering
   * Semantic/vector search
5. Return answer sources.
6. Return confidence.
7. Return no result rather than inventing an approved standard.

Required query filters:

```text
repo_id
language
framework
domain
category
status
team
system
date_range
```

### 10.3 AI answer generation

AI answer generation must:

* Use only retrieved source content.
* Cite source Learning Records and artifacts.
* Explicitly identify whether a pattern is:

  * Approved
  * Candidate
  * Legacy
  * Deprecated
  * Unknown
* Avoid making unsupported technical claims.
* Do not expose secrets.
* Use a concise engineering response format.
* Clearly state uncertainty when no approved pattern exists.

Required answer payload:

```json
{
  "answer_markdown": "string",
  "status": "approved_pattern_found",
  "confidence": "high",
  "learning_record_ids": ["uuid"],
  "source_artifact_ids": ["uuid"],
  "recommended_actions": [],
  "warnings": [],
  "suggested_tests": [],
  "created_at": "ISO-8601"
}
```

### 10.4 Thread-to-learning promotion

A TeamBrain thread can be promoted into a candidate Learning Record.

Workflow:

```text
Thread completed
→ user clicks "Promote to Learning"
→ system creates candidate Learning Record
→ AI drafts structured learning content
→ contributor edits if needed
→ reviewer receives task
→ review decision
→ approved record becomes active knowledge
```

### 10.5 Knowledge review workflow

Candidate review states:

```text
candidate
in_review
approved
rejected
legacy
deprecated
```

Review actions:

```text
approve
reject
request_changes
mark_legacy
mark_deprecated
archive
```

Each action must create an audit event.

### 10.6 Initial ingestion

MVP ingestion source priority:

1. Curated Markdown and README files
2. ADO Wiki pages
3. Manually selected source code files
4. Manually linked pull requests
5. Manually linked ADO work items
6. Asana tasks related to standards

Do not start with full automatic indexing of every repo file.

MVP seeding must prioritize curated quality over broad coverage.

### 10.7 Secret scanning

Before storage or indexing, content must be scanned for:

* Azure storage keys
* API keys
* Bearer tokens
* JWTs
* Connection strings
* Password assignments
* Private keys
* Certificate blocks
* GitHub / ADO PAT tokens
* OpenAI or Azure OpenAI keys

Behavior:

```text
Potential secret found
→ redact before indexing
→ mark source artifact as quarantined
→ notify admin
→ do not expose source content in query output
```

## 11. Technical Architecture

### 11.1 Recommended stack

Frontend:

* React
* TypeScript
* Vite or Next.js
* Microsoft Fluent UI or equivalent enterprise UI library

Backend:

* .NET 10 Web API preferred
* Alternative: Azure Functions isolated worker for event-driven endpoints
* TypeScript or Python acceptable only if team standards favor them

Azure infrastructure:

* Azure App Service or Azure Container Apps for web API
* Azure Functions for ingestion/event processing
* Azure SQL Database as system of record
* Azure Blob Storage for raw source artifacts and ingestion files
* Azure AI Search for hybrid retrieval and vector search
* Azure OpenAI / Microsoft Foundry models for extraction, summarization, embeddings, and answer generation
* Azure Key Vault for secrets
* Azure Service Bus for asynchronous processing
* Application Insights + Log Analytics for telemetry
* Entra ID for authentication and authorization

Integrations:

* Azure DevOps REST API
* Azure DevOps service hooks / webhooks
* Asana API
* Asana webhooks
* Git API where needed

### 11.2 Architectural pattern

```text
React Web UI
→ TeamBrain API
→ Azure SQL (system of record)
→ Azure Key Vault (secrets)
→ Blob Storage (source content)
→ Azure Service Bus (async jobs)
→ Azure Functions (ingestion, indexing, webhooks)
→ Azure AI Search (hybrid/vector retrieval)
→ Azure OpenAI / Foundry (LLM and embeddings)
→ Azure DevOps / Asana integrations
```

### 11.3 System of record rules

Azure SQL is the system of record for:

* Users
* Roles
* Threads
* Learning Records
* Reviews
* Audit events
* Source metadata
* Integration references
* Knowledge statuses
* Ownership
* Tags
* Version history

Blob Storage is the system of record for:

* Raw source content
* Imported files
* Sanitized source snapshots
* Large document content
* Ingestion payload archives

Azure AI Search is not the system of record.

Azure AI Search is the retrieval index.

### 11.4 Search index design

Create index: `teambrain-knowledge-v1`

Required fields:

```text
id                   (string, key)
record_type          (string, filterable)
learning_record_id   (string, filterable)
thread_id            (string, filterable)
artifact_id          (string, filterable)
title                (string, searchable)
content              (string, searchable)
summary              (string, searchable)
category             (string, filterable, facetable)
domain               (string, filterable, facetable)
systems              (collection, filterable)
languages            (collection, filterable, facetable)
repo_id              (string, filterable)
file_path            (string, searchable)
status               (string, filterable)
classification       (string, filterable)
confidence           (string, filterable)
tags                 (collection, filterable, facetable)
owner_team           (string, filterable)
approved_at          (datetime, filterable, sortable)
reviewed_at          (datetime, filterable, sortable)
expires_at           (datetime, filterable)
source_url           (string)
embedding            (vector, 1536 dimensions, cosine similarity)
```

Note: The `embedding` field is a vector field targeting Azure AI Search vector search. Dimension count must match the embedding model in use (1536 for `text-embedding-ada-002`; update if switching to a 3072-dimension model such as `text-embedding-3-large`).

Retrieval priority:

```text
approved_learning_record
gold_standard_artifact
approved_documentation
candidate_learning_record
legacy_artifact
deprecated_artifact
```

Default search must exclude `legacy`, `deprecated`, and `candidate` unless explicitly requested or necessary to provide warning context.

### 11.5 Model routing

Use a configurable model router.

Task types:

| Task                          | Model capability                              |
| ----------------------------- | --------------------------------------------- |
| Tagging / classification      | Fast, low-cost                                |
| Summarization                 | Fast, low-cost                                |
| Candidate learning extraction | Medium reasoning                              |
| Architecture analysis         | High reasoning                                |
| Code pattern comparison       | High reasoning                                |
| Long document analysis        | Long-context                                  |
| Embeddings                    | Embedding model                               |
| Secret detection supplement   | Deterministic scanner + optional model review |

Store model metadata for each AI-generated output:

```json
{
  "model_provider": "azure_openai",
  "model_name": "configured-model-name",
  "prompt_version": "v1",
  "generated_at": "ISO-8601"
}
```

## 12. Database Schema

Use Azure SQL with Entity Framework Core migrations.

### 12.1 Tables

```text
Users
Roles
UserRoles
Teams
Repositories
Threads
ThreadMessages
ThreadParticipants
LearningRecords
LearningRecordVersions
LearningRecordSources
SourceArtifacts
SourceArtifactChunks
Reviews
ReviewComments
Tags
LearningRecordTags
AuditEvents
IntegrationConnections
AsanaLinks
AdoLinks
IngestionJobs
IngestionErrors
QueryLogs
FeedbackEvents
```

### 12.2 Essential table definitions

#### LearningRecords

```text
Id uniqueidentifier primary key
Title nvarchar(500) not null
Summary nvarchar(max) not null
BodyMarkdown nvarchar(max) null
Category nvarchar(100) not null
Domain nvarchar(200) null
Status nvarchar(50) not null
Confidence nvarchar(20) not null
OwnerTeamId uniqueidentifier null
OwnerUserId uniqueidentifier null
CreatedByUserId uniqueidentifier not null
ApprovedByUserId uniqueidentifier null
ReviewedAt datetime2 null
ApprovedAt datetime2 null
ExpiresAt datetime2 null
Version int not null default 1
CreatedAt datetime2 not null
UpdatedAt datetime2 not null
IsDeleted bit not null default 0
```

#### Threads

```text
Id uniqueidentifier primary key
Title nvarchar(500) not null
ThreadType nvarchar(100) not null
Status nvarchar(50) not null
RepoId uniqueidentifier null
BranchName nvarchar(255) null
AdoWorkItemId nvarchar(100) null
AsanaTaskId nvarchar(255) null
CreatedByUserId uniqueidentifier not null
Summary nvarchar(max) null
CreatedAt datetime2 not null
UpdatedAt datetime2 not null
```

#### SourceArtifacts

```text
Id uniqueidentifier primary key
ArtifactType nvarchar(100) not null
Title nvarchar(500) not null
SourceUrl nvarchar(2000) null
RepoId uniqueidentifier null
FilePath nvarchar(2000) null
BranchName nvarchar(255) null
CommitSha nvarchar(100) null
Classification nvarchar(50) not null
ContentStorageUri nvarchar(2000) null
ContentHash nvarchar(128) null
IsQuarantined bit not null default 0
CreatedAt datetime2 not null
UpdatedAt datetime2 not null
```

#### Reviews

```text
Id uniqueidentifier primary key
LearningRecordId uniqueidentifier not null
ReviewerUserId uniqueidentifier not null
Decision nvarchar(50) not null
Comments nvarchar(max) null
CreatedAt datetime2 not null
```

#### AuditEvents

```text
Id uniqueidentifier primary key
EntityType nvarchar(100) not null
EntityId uniqueidentifier not null
Action nvarchar(100) not null
UserId uniqueidentifier null
PayloadJson nvarchar(max) null
CreatedAt datetime2 not null
```

#### QueryLogs

```text
Id uniqueidentifier primary key
UserId uniqueidentifier null
QuestionText nvarchar(max) not null
RepoId uniqueidentifier null
Domain nvarchar(200) null
Status nvarchar(50) not null
AnswerStatus nvarchar(50) not null
Confidence nvarchar(20) null
LearningRecordIds nvarchar(max) null
SourceArtifactIds nvarchar(max) null
ModelProvider nvarchar(100) null
ModelName nvarchar(200) null
PromptVersion nvarchar(50) null
LatencyMs int null
CreatedAt datetime2 not null
```

## 13. API Requirements

Base route:

```text
/api/v1
```

### 13.1 Threads

```http
POST   /threads
GET    /threads
GET    /threads/{threadId}
PATCH  /threads/{threadId}
DELETE /threads/{threadId}
POST   /threads/{threadId}/messages
POST   /threads/{threadId}/promote-learning
POST   /threads/{threadId}/participants
DELETE /threads/{threadId}/participants/{userId}
```

Note: `DELETE /threads/{threadId}` performs a soft delete (sets `IsDeleted = true`). Hard deletion is not supported.

### 13.2 Learning Records

```http
POST   /learning-records
GET    /learning-records
GET    /learning-records/{learningRecordId}
PATCH  /learning-records/{learningRecordId}
DELETE /learning-records/{learningRecordId}
POST   /learning-records/{learningRecordId}/review
POST   /learning-records/{learningRecordId}/sources
DELETE /learning-records/{learningRecordId}/sources/{artifactId}
POST   /learning-records/{learningRecordId}/deprecate
POST   /learning-records/{learningRecordId}/archive
GET    /learning-records/{learningRecordId}/versions
GET    /learning-records/{learningRecordId}/versions/{version}
```

Note: `DELETE /learning-records/{learningRecordId}` performs a soft delete. Approved records require admin role to delete.

### 13.3 Search and AI Query

```http
POST /query
POST /search
GET  /search/suggestions
```

Example query request:

```json
{
  "question": "I need to add a dropdown that loads all Guarantors on the loan.",
  "context": {
    "repo_id": "loan-platform-ui",
    "branch_name": "feature/guarantor-dropdown",
    "file_path": "/src/features/loan/LoanDetailsForm.tsx",
    "language": "typescript",
    "framework": "react",
    "ado_work_item_id": "ADO-18422"
  },
  "include_legacy_context": false
}
```

Example query response:

```json
{
  "answer_markdown": "Approved pattern found...",
  "status": "approved_pattern_found",
  "confidence": "high",
  "sources": [
    {
      "type": "learning_record",
      "id": "uuid",
      "title": "Approved Pattern for Loading Guarantors into Loan Dropdowns",
      "status": "approved",
      "url": "/learning-records/uuid"
    },
    {
      "type": "source_artifact",
      "id": "uuid",
      "title": "LoanContactsPanel.tsx",
      "classification": "gold_standard",
      "source_url": "https://dev.azure.com/..."
    }
  ],
  "suggested_tests": [
    "Loads guarantors for a loan",
    "Displays empty state when none exist",
    "Handles service failure",
    "Saves selected guarantor"
  ],
  "warnings": [
    "Do not use OldGuarantorSelect.jsx; it is marked legacy."
  ]
}
```

### 13.4 Ingestion

```http
POST /ingestion/documents
POST /ingestion/ado/wiki
POST /ingestion/ado/pull-request
POST /ingestion/source-file
GET  /ingestion/jobs/{jobId}
```

### 13.5 Integrations

```http
POST /integrations/ado/connect
POST /integrations/asana/connect
POST /webhooks/ado
POST /webhooks/asana
```

## 14. User Interface Requirements

### 14.1 MVP Pages

#### Dashboard

Display:

* Recent Threads
* Recently approved learning
* Pending reviews
* Most searched patterns
* Knowledge gaps
* Expiring standards
* Quick actions

Quick actions:

```text
Create Thread
Ask TeamBrain
Submit Learning
Review Candidates
Seed Knowledge
```

#### Ask TeamBrain

Inputs:

* Natural-language question
* Optional repo
* Optional branch
* Optional file path
* Optional ADO work item
* Optional domain

Outputs:

* Answer
* Approval status
* Confidence
* Gold examples
* Legacy warnings
* Suggested tests
* Related learning records
* Create Thread action

#### Thread Detail

Display:

* Thread title and metadata
* Linked ADO work item
* Linked Asana task
* Participants
* Conversation messages
* Findings
* Source artifacts
* Recommended learning records
* Promote to Learning action
* Export summary action

#### Learning Record Detail

Display:

* Title
* Status
* Confidence
* Summary
* Detailed guidance
* Sources
* Gold examples
* Legacy examples
* Anti-patterns
* Tags
* Owner
* Review history
* Version history
* Expiration date
* Related records

#### Review Queue

Display:

* Candidate record
* AI-generated summary
* Evidence
* Source files
* Source PRs
* Suggested classification
* Actions:

  * Approve
  * Reject
  * Request changes
  * Mark legacy
  * Mark deprecated

#### Seed Knowledge

Display:

* Ingestion source selection
* Source status
* Classification selection
* Job progress
* Quarantined secrets
* Review queue links

## 15. Initial Knowledge Seeding Plan

### 15.1 Seed priorities

Seed the following first:

1. System architecture overview
2. Domain glossary
3. Environment setup
4. Branching and pull request standards
5. UI component patterns
6. Service-layer patterns
7. API patterns
8. Testing patterns
9. Logging and error-handling standards
10. Azure deployment patterns
11. Security and secrets rules
12. Loan-party / Guarantor / Borrower domain patterns
13. Gold-standard source files
14. Legacy files to avoid

### 15.2 Seed sources

Initial source collection:

```text
README files
Architecture documentation
ADO Wiki pages
Top 20 recent high-quality pull requests
Top 20 recurring code review comments
Top 20 frequently changed files
Top 10 gold-standard files per domain
Known legacy files
Known anti-patterns
Senior engineer interviews / structured notes
```

### 15.3 Seed workflow

```text
Admin selects source
→ Source content is downloaded
→ Secret scanner runs
→ Sanitized content is stored in Blob Storage
→ Metadata stored in Azure SQL
→ Content chunked
→ Chunks embedded and indexed in Azure AI Search
→ AI proposes candidate learning records
→ Reviewer validates
→ Approved records become searchable
```

### 15.4 Seed candidate example

Candidate:

```text
Title: Approved Pattern for Loading Guarantors into Loan Dropdowns

Evidence:
- LoanContactsPanel.tsx
- CreditPackageParticipants.tsx
- PR #8842
- ADO-18422

AI proposal:
Use LoanPartyService for guarantor retrieval.
Use shared dropdown option mapper.
Use standard dropdown component.
Do not call API directly from component.

Reviewer decision required:
Approve / revise / reject / mark legacy.
```

## 16. Security Requirements

### 16.1 Authentication

* Use Entra ID.
* Require authenticated users for all app access.
* Use group-based role assignment where possible.

### 16.2 Authorization

* Enforce role checks in API, not only UI.
* Filter search results by repository and team permissions.
* Do not allow a user to query code or artifacts outside authorized repositories.

### 16.3 Secret protection

* Run secret detection before index storage.
* Redact secrets before embedding.
* Quarantine source artifacts with unresolved secret risk.
* Never return secret values in API responses or model prompts.
* Store integration secrets only in Azure Key Vault.

### 16.4 Auditability

Log:

* Search queries
* Record creation
* Record edits
* Approval/rejection decisions
* Source ingestion
* Integration actions
* Permission failures
* AI model used
* Prompt template version
* Source IDs used in AI response

## 17. Observability Requirements

Track:

```text
API latency
Search latency
LLM latency
Embedding job status
Indexing failures
Webhook failures
Asana sync failures
ADO sync failures
Secret detection events
Approval cycle time
Query volume
Top queries
No-answer queries
Helpful/unhelpful feedback
Learning reuse rate
```

Use Application Insights and Log Analytics.

## 18. Quality Requirements

### 18.1 Retrieval quality

For approved-pattern queries:

* Top result should be approved knowledge when one exists.
* Gold-standard sources should rank above ordinary code.
* Legacy sources should never rank above approved sources.
* System should return "No approved pattern found" when no approved record exists.

### 18.2 AI response quality

* Every answer must cite returned sources.
* Every answer must include confidence.
* Every answer must label pattern status.
* No unsupported implementation details.
* No fabricated file names, services, components, or APIs.
* No confidential content beyond user authorization.

### 18.3 Performance targets

MVP targets:

```text
Standard search response: under 3 seconds without LLM generation
AI answer response: under 10 seconds typical
Thread page load: under 3 seconds
Learning Record page load: under 2 seconds
Ingestion job status update: under 30 seconds
```

## 19. Acceptance Criteria for MVP Release

MVP is complete when all conditions are met:

1. Users can authenticate with Entra ID.
2. Users can create and reopen TeamBrain Threads.
3. Users can submit candidate Learning Records.
4. Reviewers can approve, reject, revise, or mark records legacy.
5. Approved records are searchable.
6. Search answers include status, confidence, and sources.
7. Admin can ingest at least Markdown, README, ADO Wiki, and manually selected source files.
8. Source content is secret-scanned before indexing.
9. Azure AI Search supports hybrid retrieval over approved knowledge.
10. TeamBrain can answer a seeded Guarantor dropdown question with approved source links.
11. TeamBrain can link a candidate record to an Asana review task.
12. Audit logs exist for approvals and changes.
13. Legacy records are not presented as recommended current standards.
14. A basic dashboard displays recent threads, pending reviews, and approved learning.
15. Application is deployed to Azure with telemetry enabled.

## 20. Phased Delivery Plan

### Phase 0: Foundation

Deliver:

```text
Solution architecture
Azure resource setup
Entra ID auth
Azure SQL schema
Blob Storage
Azure AI Search index
Key Vault
Application Insights
Basic CI/CD pipeline
```

### Phase 1: Knowledge Core

Deliver:

```text
Learning Record CRUD
Review workflow
Audit events
Source Artifact storage
Manual source ingestion
Secret scanning
Search index pipeline
Basic Ask TeamBrain page
```

### Phase 2: Persistent Threads

Deliver:

```text
Thread creation
Thread messages
Source attachment
Thread summaries
Promote thread to candidate learning
Thread-to-record linking
```

### Phase 3: Integrations

Deliver:

```text
ADO pull request linking
ADO work item linking
ADO Wiki ingestion
Asana review task creation
Asana status sync
```

### Phase 4: Pilot Experience

Deliver:

```text
Dashboard
Review queue
Search filters
Gold/legacy labeling
Feedback buttons
Usage analytics
Pilot with 3-5 developers
```

### Phase 5: VS Code Extension

Deliver:

```text
Ask TeamBrain command
Find approved pattern command
Open related Learning Record
Context-aware repo/file metadata
Submit learning from selected code
```

### Phase 6: PR Advisory Bot

Deliver:

```text
ADO service hook integration
PR context retrieval
Approved-pattern warnings
Legacy pattern warnings
Suggested tests
No automatic blocking initially
```

## 21. Implementation Instructions for AI Coding Agent

### 21.1 Build order

Implement in this order:

1. Create monorepo or solution structure.
2. Create backend API project.
3. Create React frontend.
4. Add Entra ID authentication.
5. Add Azure SQL database and EF Core models.
6. Create migration scripts.
7. Add Blob Storage abstraction.
8. Add Learning Record CRUD API.
9. Add Thread CRUD API.
10. Add Review workflow API.
11. Add audit logging.
12. Add source artifact upload and storage.
13. Add secret scanning service.
14. Add Azure AI Search indexing service.
15. Add search endpoint.
16. Add AI query endpoint using retrieved context only.
17. Add dashboard and core pages.
18. Add Asana integration.
19. Add ADO integration.
20. Add tests.
21. Add CI/CD and deployment configuration.

### 21.2 Required project structure

```text
/teambrain
  /src
    /TeamBrain.Api
    /TeamBrain.Application
    /TeamBrain.Domain
    /TeamBrain.Infrastructure
    /TeamBrain.Ingestion
    /TeamBrain.Web
  /tests
    /TeamBrain.Api.Tests
    /TeamBrain.Application.Tests
    /TeamBrain.Infrastructure.Tests
    /TeamBrain.Web.Tests
  /infra
    /bicep
  /docs
    PRD.md
    architecture.md
    api.md
    setup.md
    security.md
  /scripts
```

Note: Azure Bicep is the primary IaC tool for this project (Azure-native, no external state backend required). Terraform support is a future enhancement if multi-cloud or Terraform-first enterprise standards are required.

### 21.3 Coding standards

* Use TypeScript strict mode in frontend.
* Use nullable reference types in .NET.
* Use async/await for all I/O.
* Use dependency injection.
* Use structured logging.
* Do not hardcode credentials or connection strings.
* Store secrets in Key Vault.
* Use repository/service abstractions where appropriate.
* Write unit tests for business logic.
* Write integration tests for APIs.
* Use request validation.
* Return standardized error responses.
* Include correlation IDs in logs.
* Use role checks at API level.
* Create migrations for database changes.
* Document all public API endpoints.

### 21.4 AI generation constraints

The AI coding agent must not:

* Invent business-domain services or codebase file names as facts.
* Assume existing BC code patterns unless seeded source evidence exists.
* Mark any Learning Record approved automatically.
* Store secrets in database, logs, prompts, or search index.
* Add unrestricted repo access without authorization controls.
* Create autonomous code changes to main/master branches.
* Use Azure AI Search as the system of record.
* Return raw model chain-of-thought.
* Expose integration credentials in frontend code.
* Skip audit logging for review actions.

## 22. Future Enhancements

Not required for MVP, but design for future support:

```text
VS Code extension
ADO PR review bot
Code pattern auto-detection
Worktree/sandbox patch generation
Teams integration
Incident response rooms
Architecture decision templates
Automated stale-standard review reminders
Knowledge-gap analytics
Model cost analytics
Repository permission synchronization
GitHub integration
SharePoint/Confluence ingestion
Fine-grained code ownership routing
Automated test-generation recommendations
Feature-level agent rooms
Multi-agent orchestration
```

## 23. Success Metrics

Initial targets:

```text
50 approved learning records within first 90 days
20 gold-standard code examples tagged
80% of candidate records reviewed within 10 business days
25% reduction in repeated code review comments for seeded patterns
25% reduction in time for new developers to find approved implementation examples
At least 50% of pilot developers use TeamBrain weekly
At least 70% positive helpfulness feedback on approved-pattern answers
Zero secret exposures through TeamBrain query responses
```

## 24. Example MVP Demonstration Scenario

Scenario:

A developer opens TeamBrain and asks:

"I need to add a dropdown that loads all Guarantors on the loan."

System behavior:

1. Search approved Learning Records.
2. Find approved record:
   "Approved Pattern for Loading Guarantors into Loan Dropdowns."
3. Retrieve gold-standard source files.
4. Retrieve linked PR and ADO work item if available.
5. Return:

```text
Approved pattern found: Yes
Status: Approved
Confidence: High

Recommended approach:
Use the approved loan-party service.
Map results using the shared dropdown option mapper.
Render using the standard dropdown component.
Include loading, empty, and error states.

Gold examples:
- LoanContactsPanel.tsx
- CreditPackageParticipants.tsx

Avoid:
- OldGuarantorSelect.jsx
Reason: marked legacy because it calls APIs directly from the UI component.

Suggested tests:
- Loads guarantors for current loan
- Shows empty state when none exist
- Handles service failure
- Saves selected guarantor

Sources:
- Learning Record #...
- Source files
- PR #...
- ADO work item #...
```

If no approved record exists:

```text
No approved pattern found.

I found similar implementations, but none are approved as the current team standard.

Recommended next step:
Create a TeamBrain Thread or Candidate Learning Record and route it to the Loan Platform reviewer.
```

## 25. Final Product Statement

TeamBrain is a governed engineering memory system.

It captures useful technical discoveries, connects them to real evidence, validates them with human review, and puts approved knowledge back into the hands of developers when they need it.

The MVP must prioritize:

```text
Trust
Traceability
Governance
Security
Practical developer usefulness
Incremental adoption
```

Do not optimize first for flashy autonomous behavior.

Optimize first for trusted answers and reusable team learning.
