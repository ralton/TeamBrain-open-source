using MediatR;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Application.Integrations.Queries.GetGitHubPullRequest;

public class GetGitHubPullRequestHandler(ICodeRepositoryService repositoryService)
    : IRequestHandler<GetGitHubPullRequestQuery, PullRequestDetailsResult?>
{
    public async Task<PullRequestDetailsResult?> Handle(GetGitHubPullRequestQuery request, CancellationToken cancellationToken)
    {
        return await repositoryService.GetPullRequestAsync(request.Owner, request.Repo, request.PrNumber, cancellationToken);
    }
}
