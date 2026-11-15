# TeamBrain User Guide

This guide explains the day-to-day workflows in TeamBrain. For prerequisites, local setup, and Azure deployment, start with the [root README](../README.md).

## Roles and access

TeamBrain requires a Microsoft Entra ID bearer token for API access. The API checks role claims for selected write operations. Role names below must be present in the token with the exact values shown.

| Role | Intended use | API access currently enforced |
|---|---|---|
| Reader | Ask questions, browse knowledge, read threads, and leave answer feedback. | Authenticated read/query/feedback endpoints. |
| Contributor | Capture and improve team knowledge. | Can create/update/review learning records, add thread messages, update/promote threads, and link work items. |
| Reviewer | Govern candidate knowledge and run ingestion. | Contributor write operations plus ingestion jobs. |
| Admin | Manage the knowledge lifecycle and operational ingestion. | Reviewer access plus deleting learning records. |

These are API permissions, not separate page layouts. The web app currently shows the same navigation to signed-in users, so a page or button may be visible even when the caller lacks the required API role. A rejected write usually means the token lacks the required role. Configure Entra app roles and ensure the API token includes them; see the [authentication setup](../README.md#step-0--entra-id-app-registrations).

A few current authorization details matter:

- Browsing knowledge, asking TeamBrain, reading threads, and submitting feedback require sign-in but do not have an API role restriction.
- Thread creation also currently requires sign-in only. The intended role for creating and contributing to a thread is Contributor or above.
- The review endpoint allows Contributor, Reviewer, and Admin. For team governance, assign review responsibility to designated Reviewers/Admins even though the API currently also accepts Contributors.
- Ingestion jobs require Reviewer or Admin; deleting a learning record requires Admin.

## Ask an engineering question

**For:** Reader, Contributor, Reviewer, Admin

1. Open **Ask TeamBrain**.
2. Enter a specific engineering question. Include the framework, service, or decision you need guidance on.
3. Optionally add a repository ID and domain to narrow the context.
4. Select **Ask TeamBrain**.
5. Check the answer status and confidence. Prefer an **Approved Pattern Found** result for established team guidance. Treat partial matches and answers without an approved pattern as prompts for further review.
6. Use the Helpful or Not Helpful controls. Add a comment when useful so maintainers can improve the knowledge base.

TeamBrain returns guidance based on its available indexed context and approved knowledge. Validate code and recommendations against your project before applying them.

## Find and use an existing standard

**For:** Reader, Contributor, Reviewer, Admin

1. Open **Knowledge**.
2. Search by a meaningful phrase, then narrow results by category, status, or minimum confidence.
3. Open a result and read its summary, implementation guidance, source links, and review history.
4. Check the status before relying on it:
   - **Approved** is the current approved standard.
   - **Candidate** has not completed review.
   - **Legacy** or **Deprecated** indicates caution; look for a newer approved alternative.
   - **Rejected** or **Archived** should not be treated as current guidance.

Use approved records as the shared reference. If a record is outdated or incomplete, capture the correction in a thread or submit a new candidate.

## Capture a problem in a thread

**For:** Any signed-in user can create a thread; Contributor, Reviewer, or Admin is needed to add messages, update it, or promote it.

1. Open **Threads** and choose **Create Thread**.
2. Give it a clear title and choose Standard, Debug / Troubleshooting, Architecture Decision, or Ad-Hoc Discovery.
3. Add repository, branch, or external work item identifiers when relevant.
4. Open the thread and add investigation notes, decisions, and useful source links.
5. When the team has extracted a reusable lesson, choose **Promote to Learning**. The resulting candidate can then be reviewed from **Review Queue**.

A thread is a working record of a problem or decision. Keep sensitive credentials and secrets out of messages and source material.

## Submit a candidate learning record

**For:** Contributor, Reviewer, or Admin

There are two supported capture paths:

### From a VS Code selection

1. Configure the extension's API URL and Entra bearer token as described in the [extension guide](../extensions/vscode/README.md).
2. Select the relevant code and choose **Submit Selection to TeamBrain** from the editor context menu.
3. Include enough context in the resulting candidate for a reviewer to understand when the pattern applies and any limitations.

### From a thread

1. Document the problem, evidence, and conclusion in a thread.
2. Choose **Promote to Learning**.
3. Review the resulting candidate in the Review Queue and add missing context before approval.

A useful candidate has a focused title, short summary, implementation guidance, category, domain, confidence, and tags. Prefer links to source evidence instead of copying large amounts of source code.

> **Current UI note:** The Knowledge page includes a **Submit Learning** button, but the /learning-records/new destination is not currently registered as an application route. Use the VS Code or thread workflow above.

## Review candidate knowledge

**For:** Reviewer or Admin (the API currently also permits Contributor)

1. Open **Review Queue** and select a candidate.
2. Compare the title, summary, implementation details, tags, and source evidence with current team practice.
3. Choose one decision:
   - **Approve** when the guidance is accurate, reusable, and sufficiently supported.
   - **Request Changes** when the author should clarify or revise it.
   - **Reject** when it should not be kept as team guidance.
   - **Mark Legacy** when it remains useful context but has been superseded.
   - **Mark Deprecated** when it should be actively avoided.
4. Add comments explaining the decision, especially what needs to change or what replaces the old guidance.
5. Submit the review.

The review endpoint also supports **Archive**. The current Review Queue UI does not expose that choice.

## Run a knowledge ingestion job

**For:** Reviewer or Admin

1. Open **Seed Knowledge**.
2. Select a source type: Azure Blob Container, Git Repository, or Documentation Set.
3. Enter the source storage URI or reference and submit the job.
4. Monitor the job list for Pending, Running, Completed, or Failed. Open a job for details.
5. For failed jobs, inspect the job details and service logs, then correct the source/configuration before retrying.

Ingestion depends on the deployed Azure resources, permissions, and configured pipeline. Submitting a job does not itself approve resulting knowledge; review candidate records through the Review Queue.

## Use the VS Code extension

**For:** Reader for asking/searching; Contributor, Reviewer, or Admin for submitting code

See the [VS Code extension guide](../extensions/vscode/README.md) for packaging and configuration. The extension supports asking questions, searching approved patterns, submitting selected code as a candidate, and viewing legacy/deprecated advisories.

## Check pilot metrics

**For:** Reviewer or Admin as an operational responsibility

Open **Pilot Telemetry** to view adoption and retrieval metrics. The current API requires an authenticated token but does not restrict this endpoint to a particular role, so treat the dashboard as operational information and limit access through your deployment's identity and network controls.

## Troubleshooting

- **Sign-in works but an action fails:** confirm that the token has the role required for that write action. The API role values are Contributor, Reviewer, and Admin.
- **Ask/search returns little or no guidance:** verify that approved records exist and that the relevant content has been indexed. Candidate records are not approved standards.
- **An ingestion job fails:** inspect its details, validate the source reference and service configuration, and check Azure Function/Service Bus logs.
- **A page or button is visible but fails:** the web navigation is not role-filtered; the API decides whether the operation is authorized.
- **The VS Code extension cannot connect:** verify the API base URL, bearer token, and that the token targets the TeamBrain API.

