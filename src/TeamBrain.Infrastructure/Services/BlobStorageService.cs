using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Infrastructure.Services;

public class BlobStorageOptions
{
    public string AccountUri { get; set; } = string.Empty;
    public string ArtifactsContainer { get; set; } = "artifacts";
    public string IngestionContainer { get; set; } = "ingestion";
}

public class BlobStorageService(
    BlobServiceClient blobServiceClient,
    IOptions<BlobStorageOptions> options
) : IBlobStorageService
{
    public async Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var container = blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);

        return blob.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string storageUri, CancellationToken cancellationToken = default)
    {
        var blob = new BlobClient(new Uri(storageUri));
        var download = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return download.Value.Content;
    }

    public async Task DeleteAsync(string storageUri, CancellationToken cancellationToken = default)
    {
        var blob = new BlobClient(new Uri(storageUri));
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
