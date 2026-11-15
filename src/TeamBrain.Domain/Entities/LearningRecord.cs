using TeamBrain.Domain.Common;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Domain.Entities;

public class LearningRecord : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? BodyMarkdown { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public KnowledgeStatus Status { get; set; } = KnowledgeStatus.Candidate;
    public ConfidenceLevel Confidence { get; set; } = ConfidenceLevel.Medium;
    public int Version { get; set; } = 1;
    public bool IsDeleted { get; set; } = false;

    public string? ExternalWorkItemId { get; set; }
    public string? ExternalWorkItemProvider { get; set; }
    public string? ExternalWorkItemUrl { get; set; }

    public bool IsAutoPromoted { get; set; } = false;
    public string? AutoPromotionReason { get; set; }
    public int RequiredPeerApprovals { get; set; } = 2;

    public Guid? OwnerTeamId { get; set; }
    public Team? OwnerTeam { get; set; }
    public Guid? OwnerUserId { get; set; }
    public User? OwnerUser { get; set; }
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public Guid? ApprovedByUserId { get; set; }
    public User? ApprovedByUser { get; set; }

    public DateTime? ReviewedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public ICollection<LearningRecordTag> Tags { get; set; } = new List<LearningRecordTag>();
    public ICollection<LearningRecordSource> Sources { get; set; } = new List<LearningRecordSource>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<LearningRecordVersion> Versions { get; set; } = new List<LearningRecordVersion>();
}

public class LearningRecordTag : BaseEntity
{
    public Guid LearningRecordId { get; set; }
    public LearningRecord LearningRecord { get; set; } = null!;
    public string Tag { get; set; } = string.Empty;
}

public class LearningRecordSource : BaseEntity
{
    public Guid LearningRecordId { get; set; }
    public LearningRecord LearningRecord { get; set; } = null!;
    public Guid SourceArtifactId { get; set; }
    public SourceArtifact SourceArtifact { get; set; } = null!;
    public ArtifactClassification Classification { get; set; } = ArtifactClassification.Unknown;
}

public class LearningRecordVersion : BaseEntity
{
    public Guid LearningRecordId { get; set; }
    public LearningRecord LearningRecord { get; set; } = null!;
    public int Version { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? BodyMarkdown { get; set; }
    public Guid ChangedByUserId { get; set; }
    public User ChangedByUser { get; set; } = null!;
}
