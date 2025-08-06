using CalendarEvent.Application.Services.Models;

namespace CalendarEvent.Application.Services
{
    public interface ICalendarEntryParser
    {
        Task<CalendarEntryParseResult> ParseAsync(string message, CancellationToken ct);
    }
}
