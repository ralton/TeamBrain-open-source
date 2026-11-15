namespace TeamBrain.Application.Common.Interfaces;

public interface ISecretScannerService
{
    Task<SecretScanResult> ScanAsync(string content, CancellationToken cancellationToken = default);
    string Redact(string content);
}

public record SecretScanResult(bool HasSecrets, IReadOnlyList<SecretMatch> Matches);
public record SecretMatch(string Type, int Position, string RedactedPreview);
