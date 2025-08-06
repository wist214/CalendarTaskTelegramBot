using CalendarEvent.Domain.Entities;

namespace CalendarEvent.Application.Services.Models
{
    public record CalendarEntryParseResult
    {
        public MessageType Type { get; init; }
        public CalendarTask? Task { get; init; }
        public CalendarMeeting? Meeting { get; init; }

    }
}
