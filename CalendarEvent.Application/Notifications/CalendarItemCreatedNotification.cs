using CalendarEvent.Domain.Entities;
using MediatR;

namespace CalendarEvent.Application.Notifications
{
    public record CalendarItemCreatedNotification(
        string UserId,
        long ChatId,
        string Title,
        MessageType Type,
        DateTimeOffset Date
    ) : INotification;
}
