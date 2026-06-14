using AlNady.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AlNady.Infrastructure.Services;

/// <summary>
/// Local disk file storage provider.
/// </summary>
public class LocalStorageProvider : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalStorageProvider> _logger;

    public LocalStorageProvider(IConfiguration config, ILogger<LocalStorageProvider> logger)
    {
        _basePath = config["FileStorage:Local:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _baseUrl = config["FileStorage:Local:BaseUrl"] ?? "http://localhost:5000/uploads";
        _logger = logger;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string folder, CancellationToken ct = default)
    {
        var sanitized = Path.GetFileName(fileName);
        var uniqueName = $"{Guid.NewGuid():N}_{sanitized}";
        var folderPath = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(folderPath);
        var fullPath = Path.Combine(folderPath, uniqueName);

        using var fs = File.Create(fullPath);
        await fileStream.CopyToAsync(fs, ct);

        return $"{folder}/{uniqueName}";
    }

    public Task DeleteAsync(string filePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<Stream> DownloadAsync(string filePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public string GetPublicUrl(string filePath) => $"{_baseUrl}/{filePath}";

    public bool IsValidFileType(string contentType, IEnumerable<string> allowedTypes)
        => allowedTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);

    public bool IsValidFileSize(long sizeInBytes, long maxSizeInBytes)
        => sizeInBytes <= maxSizeInBytes;
}
