using TeamBrain.Domain.Common;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Domain.Entities;

public class TeamBrainThread : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public ThreadType ThreadType { get; set; }
    public string Status { get; set; } = "active";
    public string? RepoId { get; set; }
    public string? BranchName { get; set; }
    public string? ExternalWorkItemId { get; set; }
    public string? ExternalWorkItemProvider { get; set; } // github | ado | asana
    public string? ExternalWorkItemUrl { get; set; }
    public string? ExternalPullRequestId { get; set; }
    public string? ExternalPullRequestUrl { get; set; }

    public string? AdoWorkItemId
    {
        get => ExternalWorkItemProvider == "ado" ? ExternalWorkItemId : _adoWorkItemId;
        set => _adoWorkItemId = value;
    }
    private string? _adoWorkItemId;

    public string? AsanaTaskId
    {
        get => ExternalWorkItemProvider == "asana" ? ExternalWorkItemId : _asanaTaskId;
        set => _asanaTaskId = value;
    }
    private string? _asanaTaskId;

    public string? Summary { get; set; }
    public bool IsDeleted { get; set; } = false;

    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public ICollection<ThreadMessage> Messages { get; set; } = new List<ThreadMessage>();
    public ICollection<ThreadParticipant> Participants { get; set; } = new List<ThreadParticipant>();
    public ICollection<ThreadSourceArtifact> SourceArtifacts { get; set; } = new List<ThreadSourceArtifact>();
}

public class ThreadMessage : BaseEntity
{
    public Guid ThreadId { get; set; }
    public TeamBrainThread Thread { get; set; } = null!;
    public Guid AuthorUserId { get; set; }
    public User AuthorUser { get; set; } = null!;
    public string Role { get; set; } = "user"; // user | assistant
    public string Content { get; set; } = string.Empty;
}

public class ThreadParticipant : BaseEntity
{
    public Guid ThreadId { get; set; }
    public TeamBrainThread Thread { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}

public class ThreadSourceArtifact : BaseEntity
{
    public Guid ThreadId { get; set; }
    public TeamBrainThread Thread { get; set; } = null!;
    public Guid SourceArtifactId { get; set; }
    public SourceArtifact SourceArtifact { get; set; } = null!;
}
