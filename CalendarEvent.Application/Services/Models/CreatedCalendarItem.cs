using CalendarEvent.Domain.Entities;

namespace CalendarEvent.Application.Services.Models
{
    public class CreatedCalendarItem(MessageType messageType, string title, DateTimeOffset start, DateTimeOffset? end)
    {
        public MessageType Type { get; } = messageType;
        public string Title { get; } = title;
        public DateTimeOffset Start { get; } = start;
        public DateTimeOffset? End { get; } = end;
    }
}
