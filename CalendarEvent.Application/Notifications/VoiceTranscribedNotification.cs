using MediatR;

namespace CalendarEvent.Application.Notifications
{
    public record VoiceTranscribedNotification(
        string UserId,
        long ChatId,
        string TranscribedText,
        double Confidence) : INotification;
}
