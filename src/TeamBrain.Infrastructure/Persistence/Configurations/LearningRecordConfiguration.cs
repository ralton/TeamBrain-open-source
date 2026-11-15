using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamBrain.Domain.Entities;

namespace TeamBrain.Infrastructure.Persistence.Configurations;

public class LearningRecordConfiguration : IEntityTypeConfiguration<LearningRecord>
{
    public void Configure(EntityTypeBuilder<LearningRecord> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Title).HasMaxLength(500).IsRequired();
        builder.Property(r => r.Summary).IsRequired();
        builder.Property(r => r.Category).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Domain).HasMaxLength(200);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(r => r.Confidence).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.HasOne(r => r.CreatedByUser).WithMany().HasForeignKey(r => r.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.OwnerUser).WithMany().HasForeignKey(r => r.OwnerUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(r => r.ApprovedByUser).WithMany().HasForeignKey(r => r.ApprovedByUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(r => r.OwnerTeam).WithMany().HasForeignKey(r => r.OwnerTeamId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Category);
        builder.HasIndex(r => r.Domain);
    }
}

public class SourceArtifactConfiguration : IEntityTypeConfiguration<SourceArtifact>
{
    public void Configure(EntityTypeBuilder<SourceArtifact> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).HasMaxLength(500).IsRequired();
        builder.Property(a => a.ArtifactType).HasConversion<string>().HasMaxLength(100).IsRequired();
        builder.Property(a => a.Classification).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(a => a.SourceUrl).HasMaxLength(2000);
        builder.Property(a => a.FilePath).HasMaxLength(2000);
        builder.Property(a => a.BranchName).HasMaxLength(255);
        builder.Property(a => a.CommitSha).HasMaxLength(100);
        builder.Property(a => a.ContentStorageUri).HasMaxLength(2000);
        builder.Property(a => a.ContentHash).HasMaxLength(128);
    }
}

public class ThreadConfiguration : IEntityTypeConfiguration<TeamBrainThread>
{
    public void Configure(EntityTypeBuilder<TeamBrainThread> builder)
    {
        builder.ToTable("Threads");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).HasMaxLength(500).IsRequired();
        builder.Property(t => t.ThreadType).HasConversion<string>().HasMaxLength(100).IsRequired();
        builder.Property(t => t.Status).HasMaxLength(50).IsRequired();
        builder.Property(t => t.BranchName).HasMaxLength(255);
        builder.Property(t => t.AdoWorkItemId).HasMaxLength(100);
        builder.Property(t => t.AsanaTaskId).HasMaxLength(255);
        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasOne(t => t.CreatedByUser).WithMany().HasForeignKey(t => t.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(t => t.Status);
    }
}

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.CreatedAt);
    }
}
