using System.Text.RegularExpressions;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Infrastructure.Services;

public class SecretScannerService : ISecretScannerService
{
    private static readonly (string Type, Regex Pattern)[] Patterns =
    [
        ("AzureStorageKey", new Regex(@"DefaultEndpointsProtocol=https;AccountName=[^;]+;AccountKey=[A-Za-z0-9+/]{86}==", RegexOptions.Compiled)),
        ("ApiKey", new Regex(@"(?i)(api[_-]?key|apikey)\s*[:=]\s*['""]?[A-Za-z0-9\-_]{20,}", RegexOptions.Compiled)),
        ("BearerToken", new Regex(@"(?i)Bearer\s+[A-Za-z0-9\-_.~+/]+=*", RegexOptions.Compiled)),
        ("Jwt", new Regex(@"eyJ[A-Za-z0-9\-_]+\.eyJ[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+", RegexOptions.Compiled)),
        ("ConnectionString", new Regex(@"(?i)(Data Source|Server|Initial Catalog|Password|User ID)\s*=\s*[^;]{3,}", RegexOptions.Compiled)),
        ("PasswordAssignment", new Regex(@"(?i)(password|pwd|passwd)\s*[:=]\s*['""]?[^\s'"",;]{6,}", RegexOptions.Compiled)),
        ("PrivateKey", new Regex(@"-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----", RegexOptions.Compiled)),
        ("AdoPat", new Regex(@"(?i)(AZURE_DEVOPS_PAT|ADO_PAT|devops_token)\s*[:=]\s*[A-Za-z0-9]{52}", RegexOptions.Compiled)),
        ("OpenAiKey", new Regex(@"sk-[A-Za-z0-9]{32,}", RegexOptions.Compiled)),
        ("AzureOpenAiKey", new Regex(@"[A-Fa-f0-9]{32}", RegexOptions.Compiled | RegexOptions.IgnoreCase)),
    ];

    public Task<SecretScanResult> ScanAsync(string content, CancellationToken cancellationToken = default)
    {
        var matches = new List<SecretMatch>();

        foreach (var (type, pattern) in Patterns)
        {
            foreach (Match match in pattern.Matches(content))
            {
                var preview = match.Value.Length > 20
                    ? match.Value[..8] + "..." + match.Value[^4..]
                    : "[REDACTED]";
                matches.Add(new SecretMatch(type, match.Index, preview));
            }
        }

        return Task.FromResult(new SecretScanResult(matches.Count > 0, matches));
    }

    public string Redact(string content)
    {
        var result = content;
        foreach (var (_, pattern) in Patterns)
            result = pattern.Replace(result, "[REDACTED]");
        return result;
    }
}
