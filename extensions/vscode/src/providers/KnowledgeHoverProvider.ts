import * as vscode from 'vscode';

export class KnowledgeHoverProvider implements vscode.HoverProvider {
  public provideHover(
    document: vscode.TextDocument,
    position: vscode.Position,
    _token: vscode.CancellationToken
  ): vscode.ProviderResult<vscode.Hover> {
    const range = document.getWordRangeAtPosition(position);
    if (!range) return null;

    const word = document.getText(range);

    // Check for known legacy patterns or anti-pattern keywords
    if (word === 'StaticDbContext' || word === 'ThreadSleep' || word === 'UnverifiedBlobUpload') {
      const contents = new vscode.MarkdownString();
      contents.appendMarkdown(`**TeamBrain Advisory Warning** ⚠️\n\n`);
      contents.appendMarkdown(`The pattern \`${word}\` has been marked as a **Legacy / Anti-Pattern** in TeamBrain memory.\n\n`);
      contents.appendMarkdown(`*Recommendation:* Use the approved retry policy service pattern.\n\n`);
      contents.appendMarkdown(`[Search Approved Alternatives](command:teambrain.searchPatterns)`);
      contents.isTrusted = true;

      return new vscode.Hover(contents, range);
    }

    return null;
  }
}
