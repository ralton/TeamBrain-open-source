namespace TeamBrain.Application.Common.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string storageUri, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageUri, CancellationToken cancellationToken = default);
}
