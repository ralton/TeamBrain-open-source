using Microsoft.EntityFrameworkCore;
using TeamBrain.Domain.Entities;

namespace TeamBrain.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserRoleAssignment> UserRoleAssignments { get; }
    DbSet<Team> Teams { get; }
    DbSet<LearningRecord> LearningRecords { get; }
    DbSet<LearningRecordTag> LearningRecordTags { get; }
    DbSet<LearningRecordSource> LearningRecordSources { get; }
    DbSet<LearningRecordVersion> LearningRecordVersions { get; }
    DbSet<TeamBrainThread> Threads { get; }
    DbSet<ThreadMessage> ThreadMessages { get; }
    DbSet<ThreadParticipant> ThreadParticipants { get; }
    DbSet<ThreadSourceArtifact> ThreadSourceArtifacts { get; }
    DbSet<SourceArtifact> SourceArtifacts { get; }
    DbSet<SourceArtifactChunk> SourceArtifactChunks { get; }
    DbSet<Review> Reviews { get; }
    DbSet<ReviewComment> ReviewComments { get; }
    DbSet<AuditEvent> AuditEvents { get; }
    DbSet<IngestionJob> IngestionJobs { get; }
    DbSet<IngestionError> IngestionErrors { get; }
    DbSet<QueryLog> QueryLogs { get; }
    DbSet<FeedbackEvent> FeedbackEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
