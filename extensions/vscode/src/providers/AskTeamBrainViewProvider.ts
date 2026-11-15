import * as vscode from 'vscode';
import { askTeamBrainApi } from '../api/client.js';

export class AskTeamBrainViewProvider implements vscode.WebviewViewProvider {
  public static readonly viewType = 'teambrain.askView';
  private _view?: vscode.WebviewView;

  constructor(private readonly _extensionUri: vscode.Uri) {}

  public resolveWebviewView(
    webviewView: vscode.WebviewView,
    context: vscode.WebviewViewResolveContext,
    _token: vscode.CancellationToken
  ) {
    this._view = webviewView;

    webviewView.webview.options = {
      enableScripts: true,
      localResourceRoots: [this._extensionUri]
    };

    webviewView.webview.html = this._getHtmlForWebview();

    webviewView.webview.onDidReceiveMessage(async (message) => {
      switch (message.type) {
        case 'askQuestion': {
          try {
            const result = await askTeamBrainApi(message.question);
            webviewView.webview.postMessage({ type: 'answerResult', data: result });
          } catch (err: any) {
            webviewView.webview.postMessage({ type: 'answerError', error: err?.message || 'Failed to query TeamBrain.' });
          }
          break;
        }
      }
    });
  }

  private _getHtmlForWebview(): string {
    return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Ask TeamBrain</title>
  <style>
    body {
      font-family: var(--vscode-font-family);
      font-size: var(--vscode-font-size);
      color: var(--vscode-foreground);
      background-color: var(--vscode-editor-background);
      padding: 12px;
      margin: 0;
    }
    h3 { margin-top: 0; color: var(--vscode-titleBar-activeForeground); }
    textarea {
      width: 100%;
      box-sizing: border-box;
      background: var(--vscode-input-background);
      color: var(--vscode-input-foreground);
      border: 1px solid var(--vscode-input-border);
      padding: 8px;
      border-radius: 4px;
      resize: vertical;
      font-family: inherit;
    }
    button {
      background: var(--vscode-button-background);
      color: var(--vscode-button-foreground);
      border: none;
      padding: 8px 14px;
      border-radius: 4px;
      cursor: pointer;
      margin-top: 8px;
      width: 100%;
      font-weight: bold;
    }
    button:hover {
      background: var(--vscode-button-hoverBackground);
    }
    .badge {
      display: inline-block;
      padding: 2px 8px;
      border-radius: 4px;
      font-size: 11px;
      font-weight: bold;
      margin-right: 6px;
      margin-bottom: 8px;
    }
    .badge-approved { background: #107c41; color: #fff; }
    .badge-confidence { background: #0078d4; color: #fff; }
    .response-card {
      margin-top: 16px;
      padding: 12px;
      border: 1px solid var(--vscode-widget-border);
      border-radius: 6px;
      background: var(--vscode-editorWidget-background);
    }
    .error-card {
      margin-top: 16px;
      padding: 10px;
      color: var(--vscode-errorForeground);
      border: 1px solid var(--vscode-errorForeground);
      border-radius: 4px;
    }
  </style>
</head>
<body>
  <h3>Ask TeamBrain</h3>
  <p style="color: var(--vscode-descriptionForeground); font-size: 12px;">
    Query approved team patterns & standards inside VS Code.
  </p>
  <textarea id="questionInput" rows="4" placeholder="e.g. How do we implement Azure Blob upload retries in .NET?"></textarea>
  <button id="askBtn">Ask TeamBrain</button>

  <div id="resultContainer"></div>

  <script>
    const vscode = acquireVsCodeApi();
    const askBtn = document.getElementById('askBtn');
    const questionInput = document.getElementById('questionInput');
    const resultContainer = document.getElementById('resultContainer');

    askBtn.addEventListener('click', () => {
      const q = questionInput.value.trim();
      if (q) {
        resultContainer.innerHTML = '<p style="color: var(--vscode-descriptionForeground)">Searching TeamBrain memory...</p>';
        vscode.postMessage({ type: 'askQuestion', question: q });
      }
    });

    window.addEventListener('message', event => {
      const message = event.data;
      if (message.type === 'answerResult') {
        const data = message.data;
        let html = '<div class="response-card">';
        html += '<span class="badge badge-approved">' + (data.status === 'approved_pattern_found' ? 'Approved Pattern' : 'Knowledge Result') + '</span>';
        html += '<span class="badge badge-confidence">Confidence: ' + data.confidence + '</span>';
        html += '<div style="margin-top:10px; line-height:1.4;">' + data.answerMarkdown.replace(/\\n/g, '<br>') + '</div>';
        
        if (data.suggestedTests && data.suggestedTests.length > 0) {
          html += '<h4 style="margin-bottom:4px;">Suggested Tests:</h4><ul>';
          data.suggestedTests.forEach(t => { html += '<li>' + t + '</li>'; });
          html += '</ul>';
        }
        html += '</div>';
        resultContainer.innerHTML = html;
      } else if (message.type === 'answerError') {
        resultContainer.innerHTML = '<div class="error-card">' + message.error + '</div>';
      }
    });
  </script>
</body>
</html>`;
  }
}
