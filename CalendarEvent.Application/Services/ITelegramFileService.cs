namespace CalendarEvent.Application.Services
{
    public interface ITelegramFileService
    {
        Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default);
        Task<TelegramFileInfo> GetFileInfoAsync(string fileId, CancellationToken cancellationToken = default);
    }

    public record TelegramFileInfo
    {
        public string FileId { get; init; } = string.Empty;
        public string? FilePath { get; init; }
        public long FileSize { get; init; }
        public string? MimeType { get; init; }
    }
}
