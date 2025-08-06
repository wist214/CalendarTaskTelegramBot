using MediatR;

namespace CalendarEvent.Application.Notifications
{
    public record CalendarItemFailedNotification(string UserId, long ChatId, string Title, string ErrorMessage) : INotification;
}
