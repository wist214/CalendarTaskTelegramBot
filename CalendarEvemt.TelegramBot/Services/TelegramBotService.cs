using CalendarEvent.Application.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace CalendarEvent.TelegramBot.Services
{
    public interface ITelegramBotService
    {
        Task HandleUpdateAsync(Update update, CancellationToken ct);
    }

    public class TelegramBotService : ITelegramBotService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TelegramBotService> _logger;

        public TelegramBotService(IMediator mediator, ILogger<TelegramBotService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task HandleUpdateAsync(Update update, CancellationToken ct)
        {
            try
            {
                if (update.Message == null)
                {
                    _logger.LogDebug("Received update without message");
                    return;
                }

                var userId = update.Message.From?.Id.ToString() ?? "unknown_user";
                var chatId = update.Message.Chat.Id;

                // Handle text messages
                if (!string.IsNullOrEmpty(update.Message.Text))
                {
                    await HandleTextMessage(userId, chatId, update.Message.Text, ct);
                }
                // Handle voice messages
                else if (update.Message.Voice != null)
                {
                    await HandleVoiceMessage(userId, chatId, update.Message.Voice, ct);
                }
                // Handle audio messages (some clients send audio instead of voice)
                else if (update.Message.Audio != null)
                {
                    await HandleAudioMessage(userId, chatId, update.Message.Audio, ct);
                }
                else
                {
                    _logger.LogDebug("Received unsupported message type from user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Telegram update");
            }
        }

        private async Task HandleTextMessage(string userId, long chatId, string message, CancellationToken ct)
        {
            _logger.LogDebug("Handling text message from user {UserId}: {Message}", userId, message);

            if (message == "/login")
            {
                await _mediator.Send(new LoginCommand(userId, chatId), ct);
            }
            else if (message == "/logout")
            {
                await _mediator.Send(new LogoutCommand(userId), ct);
            }
            else if (message == "/help")
            {
                await _mediator.Send(new SendHelpCommand(userId, chatId), ct);
            }
            else
            {
                await _mediator.Send(new CreateCalendarItemCommand(userId, chatId, message), ct);
            }
        }

        private async Task HandleVoiceMessage(string userId, long chatId, Voice voice, CancellationToken ct)
        {
            _logger.LogInformation("Handling voice message from user {UserId}, duration: {Duration}s",
                userId, voice.Duration);

            if (voice.Duration > 60) // Limit voice messages to 60 seconds
            {
                _logger.LogWarning("Voice message too long: {Duration}s from user {UserId}", voice.Duration, userId);
                // You might want to send a notification about the length limit
                return;
            }

            await _mediator.Send(new ProcessVoiceMessageCommand(userId, chatId, voice.FileId, voice.Duration), ct);
        }

        private async Task HandleAudioMessage(string userId, long chatId, Audio audio, CancellationToken ct)
        {
            _logger.LogInformation("Handling audio message from user {UserId}, duration: {Duration}s",
                userId, audio.Duration);

            var duration = audio.Duration;
            if (duration > 60) // Limit audio messages to 60 seconds
            {
                _logger.LogWarning("Audio message too long: {Duration}s from user {UserId}", duration, userId);
                return;
            }

            // Treat audio messages the same as voice messages
            await _mediator.Send(new ProcessVoiceMessageCommand(userId, chatId, audio.FileId, duration), ct);
        }
    }
}
