# TeamBrain Web

The TeamBrain web client is a React 18 and TypeScript application built with Vite and Fluent UI. It provides the authenticated dashboard, knowledge directory, review queue, thread workspace, and Ask TeamBrain experience.

## Run locally

From this directory:

```bash
cp .env.example .env.local
```

Set the Entra ID client and tenant IDs, and point the API URL at the local API HTTPS endpoint:

```env
VITE_ENTRA_CLIENT_ID=<teambrain-web client ID>
VITE_ENTRA_TENANT_ID=<Entra tenant ID>
VITE_API_BASE_URL=https://localhost:7153/api/v1
```

Then install dependencies and start the development server:

```bash
npm install
npm run dev
```

The Vite development server runs at http://localhost:5173. See the [root README](../../README.md) for the full setup and infrastructure instructions.
