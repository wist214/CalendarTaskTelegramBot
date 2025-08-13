using CalendarEvent.Application.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CalendarEvent.Infrastructure.Telegram
{
    public class TelegramFileService : ITelegramFileService
    {
        private readonly TelegramBotClient _client;
        private readonly ILogger<TelegramFileService> _logger;

        public TelegramFileService(ILogger<TelegramFileService> logger)
        {
            _logger = logger;
            var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ??
                throw new InvalidOperationException("TELEGRAM_BOT_TOKEN environment variable is required");
            _client = new TelegramBotClient(botToken);
        }

        public async Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Downloading file with ID: {FileId}", fileId);

                var file = await _client.GetFile(fileId, cancellationToken);
                
                if (string.IsNullOrEmpty(file.FilePath))
                {
                    throw new InvalidOperationException($"File path is empty for file ID: {fileId}");
                }

                var stream = new MemoryStream();
                await _client.DownloadFile(file.FilePath, stream, cancellationToken);
                stream.Position = 0; // Reset position for reading

                _logger.LogDebug("Successfully downloaded file {FileId}, size: {Size} bytes", fileId, stream.Length);
                
                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download file with ID: {FileId}", fileId);
                throw;
            }
        }

        public async Task<TelegramFileInfo> GetFileInfoAsync(string fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Getting file info for ID: {FileId}", fileId);

                var file = await _client.GetFile(fileId, cancellationToken);

                var fileInfo = new TelegramFileInfo
                {
                    FileId = file.FileId,
                    FilePath = file.FilePath,
                    FileSize = file.FileSize ?? 0
                };

                _logger.LogDebug("File info retrieved for {FileId}: Path={FilePath}, Size={Size}", 
                    fileId, file.FilePath, file.FileSize);

                return fileInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file info for ID: {FileId}", fileId);
                throw;
            }
        }
    }
}
