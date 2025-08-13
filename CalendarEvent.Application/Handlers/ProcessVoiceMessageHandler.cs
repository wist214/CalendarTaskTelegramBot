using CalendarEvent.Application.Commands;
using CalendarEvent.Application.Notifications;
using CalendarEvent.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CalendarEvent.Application.Handlers
{
    public class ProcessVoiceMessageHandler : IRequestHandler<ProcessVoiceMessageCommand>
    {
        private readonly ISpeechToTextService _speechToTextService;
        private readonly ICalendarEventStore _calendarEventStore;
        private readonly ITelegramFileService _telegramFileService;
        private readonly IMediator _mediator;
        private readonly ILogger<ProcessVoiceMessageHandler> _logger;

        public ProcessVoiceMessageHandler(
            ISpeechToTextService speechToTextService,
            ICalendarEventStore calendarEventStore,
            ITelegramFileService telegramFileService,
            IMediator mediator,
            ILogger<ProcessVoiceMessageHandler> logger)
        {
            _speechToTextService = speechToTextService;
            _calendarEventStore = calendarEventStore;
            _telegramFileService = telegramFileService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(ProcessVoiceMessageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing voice message for user {UserId} in chat {ChatId}", 
                    request.UserId, request.ChatId);

                // Download the voice file from Telegram
                using var audioStream = await _telegramFileService.DownloadFileAsync(request.FileId, cancellationToken);
                
                // Convert speech to text
                var transcriptionResult = await _speechToTextService.TranscribeAsync(audioStream, "ru-RU", cancellationToken);

                if (!transcriptionResult.IsSuccess || string.IsNullOrWhiteSpace(transcriptionResult.TranscribedText))
                {
                    await PublishFailureNotification(request, 
                        "Не удалось распознать речь. Попробуйте говорить четче или используйте текстовое сообщение.", 
                        cancellationToken);
                    return;
                }

                _logger.LogInformation("Voice transcribed: '{Text}' with confidence {Confidence}", 
                    transcriptionResult.TranscribedText, transcriptionResult.Confidence);

                // Send feedback to user about transcription
                await _mediator.Publish(new VoiceTranscribedNotification(
                    request.UserId,
                    request.ChatId,
                    transcriptionResult.TranscribedText,
                    transcriptionResult.Confidence
                ), cancellationToken);

                // Process the transcribed text as a calendar command
                var createCommand = new CreateCalendarItemCommand(request.UserId, request.ChatId, transcriptionResult.TranscribedText);
                await _mediator.Send(createCommand, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                await PublishFailureNotification(request, ex.Message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing voice message for user {UserId}", request.UserId);
                await PublishFailureNotification(request, 
                    "Произошла ошибка при обработке голосового сообщения. Попробуйте еще раз.", 
                    cancellationToken);
            }
        }

        private async Task PublishFailureNotification(ProcessVoiceMessageCommand request, string errorMessage, CancellationToken cancellationToken)
        {
            await _mediator.Publish(new CalendarItemFailedNotification(
                request.UserId,
                request.ChatId,
                "Голосовое сообщение",
                errorMessage
            ), cancellationToken);
        }
    }
}
