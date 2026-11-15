using TeamBrain.Domain.Common;

namespace TeamBrain.Domain.Entities;

public class IngestionJob : BaseEntity
{
    public string SourceType { get; set; } = string.Empty;
    public string SourceReference { get; set; } = string.Empty;
    public string Status { get; set; } = "pending"; // pending | running | completed | failed
    public int ArtifactsProcessed { get; set; }
    public int ArtifactsQuarantined { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<IngestionError> Errors { get; set; } = new List<IngestionError>();
}

public class IngestionError : BaseEntity
{
    public Guid IngestionJobId { get; set; }
    public IngestionJob IngestionJob { get; set; } = null!;
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SourceReference { get; set; }
}
