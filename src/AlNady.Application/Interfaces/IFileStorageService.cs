namespace AlNady.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string folder, CancellationToken ct = default);
    Task DeleteAsync(string filePath, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string filePath, CancellationToken ct = default);
    string GetPublicUrl(string filePath);
    bool IsValidFileType(string contentType, IEnumerable<string> allowedTypes);
    bool IsValidFileSize(long sizeInBytes, long maxSizeInBytes);
}
