using TeamBrain.Domain.Common;

namespace TeamBrain.Domain.Entities;

public class QueryLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? RepoId { get; set; }
    public string? Domain { get; set; }
    public string AnswerStatus { get; set; } = string.Empty;
    public string? Confidence { get; set; }
    public string? LearningRecordIds { get; set; }
    public string? SourceArtifactIds { get; set; }
    public string? ModelProvider { get; set; }
    public string? ModelName { get; set; }
    public string? PromptVersion { get; set; }
    public int? LatencyMs { get; set; }
}

public class FeedbackEvent : BaseEntity
{
    public Guid? UserId { get; set; }
    public Guid QueryLogId { get; set; }
    public QueryLog QueryLog { get; set; } = null!;
    public bool IsHelpful { get; set; }
    public string? Comment { get; set; }
}
