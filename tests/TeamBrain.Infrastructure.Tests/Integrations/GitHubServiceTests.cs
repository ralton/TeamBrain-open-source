using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TeamBrain.Infrastructure.Services.Integrations;
using Xunit;

namespace TeamBrain.Infrastructure.Tests.Integrations;

public class GitHubServiceTests
{
    [Fact]
    public async Task GetPullRequestAsync_ReturnsMappedPRDetails()
    {
        var jsonPayload = JsonSerializer.Serialize(new
        {
            number = 42,
            title = "Add GitHub Integration",
            state = "open",
            user = new { login = "octocat" },
            html_url = "https://github.com/octocat/Hello-World/pull/42",
            head = new { @ref = "feature/github" },
            @base = new { @ref = "main" },
            created_at = DateTime.UtcNow.ToString("o")
        });

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, jsonPayload);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new GitHubOptions { ApiBaseUrl = "https://api.github.com" });

        var service = new GitHubService(httpClient, options, NullLogger<GitHubService>.Instance);

        var result = await service.GetPullRequestAsync("octocat", "Hello-World", 42);

        Assert.NotNull(result);
        Assert.Equal(42, result!.Number);
        Assert.Equal("Add GitHub Integration", result.Title);
        Assert.Equal("octocat", result.Author);
        Assert.Equal("feature/github", result.HeadBranch);
    }
}

public class MockHttpMessageHandler(HttpStatusCode statusCode, string content) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content)
        };
        return Task.FromResult(response);
    }
}
