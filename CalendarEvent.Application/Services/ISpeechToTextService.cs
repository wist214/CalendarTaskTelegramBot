namespace CalendarEvent.Application.Services
{
    public interface ISpeechToTextService
    {
        Task<SpeechToTextResult> TranscribeAsync(Stream audioStream, string language = "ru-RU", CancellationToken cancellationToken = default);
    }

    public record SpeechToTextResult
    {
        public bool IsSuccess { get; init; }
        public string? TranscribedText { get; init; }
        public string? ErrorMessage { get; init; }
        public double Confidence { get; init; }

        public static SpeechToTextResult Success(string text, double confidence = 1.0) =>
            new() { IsSuccess = true, TranscribedText = text, Confidence = confidence };

        public static SpeechToTextResult Failure(string errorMessage) =>
            new() { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
