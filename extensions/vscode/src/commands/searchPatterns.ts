import * as vscode from 'vscode';
import { searchPatternsApi } from '../api/client.js';

export async function registerSearchPatternsCommand(): Promise<vscode.Disposable> {
  return vscode.commands.registerCommand('teambrain.searchPatterns', async () => {
    const query = await vscode.window.showInputBox({
      prompt: 'Search TeamBrain approved patterns & standards',
      placeHolder: 'e.g. Retry policy, Azure Blob, EF Core'
    });

    if (!query) return;

    try {
      const results = await searchPatternsApi(query);

      if (results.length === 0) {
        vscode.window.showInformationMessage(`No matching team patterns found for '${query}'.`);
        return;
      }

      const quickPickItems: vscode.QuickPickItem[] = results.map(r => ({
        label: `[${r.status}] ${r.title}`,
        description: `${r.category} · Confidence: ${r.confidence}`,
        detail: r.summary
      }));

      const selected = await vscode.window.showQuickPick(quickPickItems, {
        placeHolder: 'Select a pattern to inspect details'
      });

      if (selected) {
        vscode.window.showInformationMessage(`${selected.label}\n\n${selected.detail}`);
      }
    } catch (err: any) {
      vscode.window.showErrorMessage(`Error searching patterns: ${err?.message || 'API error'}`);
    }
  });
}
