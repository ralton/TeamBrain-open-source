import * as vscode from 'vscode';
import { AskTeamBrainViewProvider } from './providers/AskTeamBrainViewProvider.js';
import { KnowledgeHoverProvider } from './providers/KnowledgeHoverProvider.js';
import { registerSubmitSelectionCommand } from './commands/submitSelection.js';
import { registerSearchPatternsCommand } from './commands/searchPatterns.js';

export function activate(context: vscode.ExtensionContext) {
  console.log('TeamBrain VS Code extension activated.');

  // 1. Register Webview Provider for Sidebar
  const askProvider = new AskTeamBrainViewProvider(context.extensionUri);
  context.subscriptions.push(
    vscode.window.registerWebviewViewProvider(AskTeamBrainViewProvider.viewType, askProvider)
  );

  // 2. Register Hover Provider for Pattern Advisories
  context.subscriptions.push(
    vscode.languages.registerHoverProvider({ scheme: 'file' }, new KnowledgeHoverProvider())
  );

  // 3. Register Commands
  registerSubmitSelectionCommand().then(disposable => context.subscriptions.push(disposable));
  registerSearchPatternsCommand().then(disposable => context.subscriptions.push(disposable));

  // Configure API Command
  context.subscriptions.push(
    vscode.commands.registerCommand('teambrain.configureApi', async () => {
      const config = vscode.workspace.getConfiguration('teambrain');
      const currentUrl = config.get<string>('apiUrl', 'https://localhost:5001/api/v1');

      const newUrl = await vscode.window.showInputBox({
        prompt: 'Enter the TeamBrain API Base URL',
        value: currentUrl
      });

      if (newUrl) {
        await config.update('apiUrl', newUrl, vscode.ConfigurationTarget.Global);
        vscode.window.showInformationMessage(`Updated TeamBrain API URL to: ${newUrl}`);
      }
    })
  );

  // Status bar indicator
  const statusBarItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Right, 100);
  statusBarItem.text = '$(brain) TeamBrain';
  statusBarItem.tooltip = 'Click to Ask TeamBrain';
  statusBarItem.command = 'teambrain.searchPatterns';
  statusBarItem.show();
  context.subscriptions.push(statusBarItem);
}

export function deactivate() {}
