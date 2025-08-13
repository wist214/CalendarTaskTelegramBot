using CalendarEvent.Application.Services;
using Microsoft.Extensions.Logging;

namespace CalendarEvent.Infrastructure.Speech
{
    /// <summary>
    /// Mock implementation of speech-to-text service for development and testing.
    /// Returns predefined responses based on audio duration.
    /// </summary>
    public class MockSpeechToTextService : ISpeechToTextService
    {
        private readonly ILogger<MockSpeechToTextService> _logger;
        
        private readonly string[] _mockResponses = new[]
        {
            "Создать задачу купить продукты завтра в 18:00",
            "Встреча с командой в понедельник в 10 утра",
            "Позвонить клиенту сегодня до 17:00",
            "Задача написать отчет до пятницы",
            "Встреча с врачом во вторник в 15:30",
            "Купить подарок на день рождения",
            "Забронировать столик в ресторане на субботу",
            "Подготовить презентацию к совещанию"
        };

        public MockSpeechToTextService(ILogger<MockSpeechToTextService> logger)
        {
            _logger = logger;
        }

        public async Task<SpeechToTextResult> TranscribeAsync(Stream audioStream, string language = "ru-RU", CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Using mock speech-to-text service. This should not be used in production!");

            try
            {
                // Simulate processing delay
                await Task.Delay(1000, cancellationToken);

                // Read stream length to determine which mock response to use
                var streamLength = audioStream.Length;
                var responseIndex = (int)(streamLength % _mockResponses.Length);
                var mockText = _mockResponses[responseIndex];

                _logger.LogInformation("Mock transcription: '{Text}' (based on stream length: {Length})", 
                    mockText, streamLength);

                return SpeechToTextResult.Success(mockText, 0.95);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mock speech transcription");
                return SpeechToTextResult.Failure("Mock transcription failed");
            }
        }
    }
}
