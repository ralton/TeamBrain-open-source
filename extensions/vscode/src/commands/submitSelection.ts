import * as vscode from 'vscode';
import { submitLearningRecordApi } from '../api/client.js';

export async function registerSubmitSelectionCommand(): Promise<vscode.Disposable> {
  return vscode.commands.registerCommand('teambrain.submitSelection', async () => {
    const editor = vscode.window.activeTextEditor;
    if (!editor) {
      vscode.window.showWarningMessage('No active code editor found.');
      return;
    }

    const selection = editor.selection;
    const selectedText = editor.document.getText(selection);

    if (!selectedText.trim()) {
      vscode.window.showWarningMessage('Please select a code block to submit to TeamBrain.');
      return;
    }

    const fileName = editor.document.fileName.split(/[\\/]/).pop() || 'CodeSnippet';
    const languageId = editor.document.languageId;
    const startLine = selection.start.line + 1;
    const endLine = selection.end.line + 1;

    const title = await vscode.window.showInputBox({
      prompt: 'Enter a title for this candidate learning record',
      placeHolder: `Pattern discovery in ${fileName}`
    });

    if (!title) return;

    const summary = await vscode.window.showInputBox({
      prompt: 'Enter a brief summary of what this code pattern solves',
      placeHolder: 'Approved pattern for handling transient retry exceptions'
    });

    if (!summary) return;

    const bodyMarkdown = `### Code Snippet Reference\n**File:** \`${fileName}\` (Lines ${startLine}-${endLine})\n**Language:** \`${languageId}\`\n\n\`\`\`${languageId}\n${selectedText}\n\`\`\``;

    try {
      const recordId = await submitLearningRecordApi({
        title,
        summary,
        bodyMarkdown,
        category: 'Code Pattern',
        tags: [languageId, fileName]
      });

      vscode.window.showInformationMessage(`Candidate learning submitted successfully! ID: ${recordId.substring(0, 8)}...`);
    } catch (err: any) {
      vscode.window.showErrorMessage(`Failed to submit learning: ${err?.message || 'API error'}`);
    }
  });
}
