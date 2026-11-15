namespace TeamBrain.Infrastructure.Services.Integrations;

public class GitHubOptions
{
    public string? PersonalAccessToken { get; set; }
    public string UserAgent { get; set; } = "TeamBrain-Platform";
    public string ApiBaseUrl { get; set; } = "https://api.github.com";
}
