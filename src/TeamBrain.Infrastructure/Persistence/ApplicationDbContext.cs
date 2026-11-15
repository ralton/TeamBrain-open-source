using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;

namespace TeamBrain.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<LearningRecord> LearningRecords => Set<LearningRecord>();
    public DbSet<LearningRecordTag> LearningRecordTags => Set<LearningRecordTag>();
    public DbSet<LearningRecordSource> LearningRecordSources => Set<LearningRecordSource>();
    public DbSet<LearningRecordVersion> LearningRecordVersions => Set<LearningRecordVersion>();
    public DbSet<TeamBrainThread> Threads => Set<TeamBrainThread>();
    public DbSet<ThreadMessage> ThreadMessages => Set<ThreadMessage>();
    public DbSet<ThreadParticipant> ThreadParticipants => Set<ThreadParticipant>();
    public DbSet<ThreadSourceArtifact> ThreadSourceArtifacts => Set<ThreadSourceArtifact>();
    public DbSet<SourceArtifact> SourceArtifacts => Set<SourceArtifact>();
    public DbSet<SourceArtifactChunk> SourceArtifactChunks => Set<SourceArtifactChunk>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<IngestionJob> IngestionJobs => Set<IngestionJob>();
    public DbSet<IngestionError> IngestionErrors => Set<IngestionError>();
    public DbSet<QueryLog> QueryLogs => Set<QueryLog>();
    public DbSet<FeedbackEvent> FeedbackEvents => Set<FeedbackEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
