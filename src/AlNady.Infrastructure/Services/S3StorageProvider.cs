using AlNady.Application.Interfaces;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AlNady.Infrastructure.Services;

/// <summary>
/// AWS S3 file storage provider.
/// </summary>
public class S3StorageProvider : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicBaseUrl;
    private readonly ILogger<S3StorageProvider> _logger;

    public S3StorageProvider(IAmazonS3 s3Client, IConfiguration config, ILogger<S3StorageProvider> logger)
    {
        _s3Client = s3Client;
        _bucketName = config["FileStorage:S3:BucketName"] ?? throw new InvalidOperationException("S3 bucket not configured");
        _publicBaseUrl = config["FileStorage:S3:PublicBaseUrl"] ?? $"https://{_bucketName}.s3.amazonaws.com";
        _logger = logger;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string folder, CancellationToken ct = default)
    {
        var sanitized = Path.GetFileName(fileName);
        var key = $"{folder}/{Guid.NewGuid():N}_{sanitized}";

        var utility = new TransferUtility(_s3Client);
        var request = new TransferUtilityUploadRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead
        };

        await utility.UploadAsync(request, ct);
        return key;
    }

    public async Task DeleteAsync(string filePath, CancellationToken ct = default)
    {
        await _s3Client.DeleteObjectAsync(_bucketName, filePath, ct);
    }

    public async Task<Stream> DownloadAsync(string filePath, CancellationToken ct = default)
    {
        var response = await _s3Client.GetObjectAsync(_bucketName, filePath, ct);
        return response.ResponseStream;
    }

    public string GetPublicUrl(string filePath) => $"{_publicBaseUrl}/{filePath}";

    public bool IsValidFileType(string contentType, IEnumerable<string> allowedTypes)
        => allowedTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);

    public bool IsValidFileSize(long sizeInBytes, long maxSizeInBytes)
        => sizeInBytes <= maxSizeInBytes;
}
