using MediatR;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Application.Integrations.Queries.GetGitHubPullRequest;

public record GetGitHubPullRequestQuery(
    string Owner,
    string Repo,
    int PrNumber
) : IRequest<PullRequestDetailsResult?>;
