using TeamBrain.Domain.Common;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Domain.Entities;

public class Review : BaseEntity
{
    public Guid LearningRecordId { get; set; }
    public LearningRecord LearningRecord { get; set; } = null!;
    public Guid ReviewerUserId { get; set; }
    public User ReviewerUser { get; set; } = null!;
    public ReviewDecision Decision { get; set; }
    public string? Comments { get; set; }

    public ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();
}

public class ReviewComment : BaseEntity
{
    public Guid ReviewId { get; set; }
    public Review Review { get; set; } = null!;
    public Guid AuthorUserId { get; set; }
    public User AuthorUser { get; set; } = null!;
    public string Comment { get; set; } = string.Empty;
}
