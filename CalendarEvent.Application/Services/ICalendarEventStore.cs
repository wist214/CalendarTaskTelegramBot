using CalendarEvent.Application.DTOs;
using CalendarEvent.Application.Services.Models;

namespace CalendarEvent.Application.Services
{

    public interface ICalendarEventProvider
    {
        Task<List<CalendarItemDto>> GetAsync(Guid userId, DateTimeOffset date, CancellationToken ct);
    }

    public interface ICalendarEventStore : ICalendarEventProvider
    {
        Task<CreatedCalendarItem> CreateAsync(string userId, string title, CancellationToken ct);
    }
}
