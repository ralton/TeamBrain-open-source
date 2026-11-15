using TeamBrain.Domain.Common;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Domain.Entities;

public class SourceArtifact : BaseEntity
{
    public ArtifactType ArtifactType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public string? RepoId { get; set; }
    public string? FilePath { get; set; }
    public string? BranchName { get; set; }
    public string? CommitSha { get; set; }
    public ArtifactClassification Classification { get; set; } = ArtifactClassification.Unknown;
    public string? ContentStorageUri { get; set; }
    public string? ContentHash { get; set; }
    public bool IsQuarantined { get; set; } = false;

    public ICollection<SourceArtifactChunk> Chunks { get; set; } = new List<SourceArtifactChunk>();
}

public class SourceArtifactChunk : BaseEntity
{
    public Guid SourceArtifactId { get; set; }
    public SourceArtifact SourceArtifact { get; set; } = null!;
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? SearchIndexId { get; set; }
}
