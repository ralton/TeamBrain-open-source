# TeamBrain VS Code Extension

**Governed Engineering Intelligence inside your IDE.**

The TeamBrain VS Code Extension delivers approved team patterns, architecture decisions, gold-standard code references, and pattern advisories directly inside Microsoft VS Code.

---

## Key Features

### 1. Ask TeamBrain Sidebar Panel
- Dedicated **TeamBrain Activity Bar** view.
- Type technical questions (e.g. *"How do we implement Azure Blob upload retries in .NET?"*) and receive instant approved pattern guidance, confidence scores, and suggested unit tests.

### 2. Submit Code Selection to TeamBrain
- Highlight any code block in your editor, right-click, and select **Submit Selection to TeamBrain**.
- Automatically captures file name, language, line numbers, and selected code snippet to create a candidate learning record.

### 3. Quick Pick Pattern Search (`Ctrl+Shift+P` / `Cmd+Shift+P`)
- Run **TeamBrain: Search Approved Patterns** from the Command Palette to search approved team standards without leaving your editor.

### 4. Legacy Pattern Hover Advisories
- Hover over legacy or deprecated code keywords to view warning tooltips and clickable links to approved alternative patterns.

---

## Prerequisites & Setup Scripts

Before building or packaging the extension, run the prerequisite setup script for your operating system:

### Windows (PowerShell)

```powershell
.\scripts\setup-extension-prereqs.ps1
```

### Linux / macOS (Bash)

```bash
chmod +x ./scripts/setup-extension-prereqs.sh
./scripts/setup-extension-prereqs.sh
```

**Prerequisites Checked:**
- Node.js (v20+) & npm
- VS Code CLI (`code`)
- `@vscode/vsce` extension packaging tool (`npm install -g @vscode/vsce`)

---

## Building an Independent VSIX Package

To package the extension into an independent `.vsix` file for internal developer distribution:

```bash
cd extensions/vscode
npm install
npm run package
```

This generates `teambrain-vscode-1.0.0.vsix` inside the `extensions/vscode` folder.

---

## Installing the `.vsix` Package for Developers

Developers can install the generated package into VS Code using either of the following methods:

### Method A — VS Code CLI
```bash
code --install-extension extensions/vscode/teambrain-vscode-1.0.0.vsix
```

### Method B — VS Code UI
1. Open VS Code.
2. Go to **Extensions** (`Ctrl+Shift+X` / `Cmd+Shift+X`).
3. Click the **...** (More Actions) menu at the top right of the Extensions panel.
4. Select **Install from VSIX...**
5. Select `teambrain-vscode-1.0.0.vsix` and click **Install**.

---

## Configuration Settings

Configure the extension settings in VS Code (`Settings -> Extensions -> TeamBrain` or `settings.json`):

| Setting | Default | Description |
|---|---|---|
| `teambrain.apiUrl` | `https://localhost:5001/api/v1` | The base URL for the deployed or local TeamBrain Web API. |
| `teambrain.authToken` | `""` | Entra ID Bearer JWT token for authenticating API requests. |

---

## Command Reference

| Command | Title | Context / Shortcut |
|---|---|---|
| `teambrain.ask` | TeamBrain: Ask Question | Sidebar & Command Palette |
| `teambrain.submitSelection` | Submit Selection to TeamBrain | Editor Context Menu (Right-Click) |
| `teambrain.searchPatterns` | TeamBrain: Search Approved Patterns | Command Palette & Status Bar |
| `teambrain.configureApi` | TeamBrain: Configure API Endpoint | Command Palette |
