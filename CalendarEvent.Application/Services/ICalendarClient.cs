using CalendarEvent.Application.Services.Models;
using CalendarEvent.Domain.Entities;

namespace CalendarEvent.Application.Services
{
    public interface ICalendarClient
    {
        public Task<CalendarTask> CreateTaskAsync(CalendarTask task, UserToken token, CancellationToken ct);

        public Task<CalendarMeeting> CreateMeetingAsync(CalendarMeeting meeting, UserToken token, CancellationToken ct);

        public Task<List<CalendarTask>> GetTasksAsync(DateTimeOffset date, UserToken token, CancellationToken ct);
    }
}
