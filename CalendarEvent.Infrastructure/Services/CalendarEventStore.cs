using System.Security.Authentication;
using CalendarEvent.Application.DTOs;
using CalendarEvent.Application.Services;
using CalendarEvent.Application.Services.Models;
using CalendarEvent.Domain.Entities;

namespace CalendarEvent.Infrastructure.Services
{
    public class CalendarEventStore(ICalendarClient calendarClient, IUserTokenStore userTokenStore, ICalendarEntryParser calendarEntryParser) : ICalendarEventStore
    {
        public async Task<List<CalendarItemDto>> GetAsync(Guid userId, DateTimeOffset date, CancellationToken ct)
        {
            var token = await userTokenStore.GetAsync(userId.ToString(), ct);

            if (token == null)
            {
                throw new InvalidOperationException("User token not found.");
            }

            var calendarItems = await calendarClient.GetTasksAsync(date, token, ct);

            return calendarItems.Select(item => new CalendarItemDto //TODO create extension method for mapping
            {
                Title = item.Title,
                Start = item.Due,
            }).ToList();
        }

        public async Task<CreatedCalendarItem> CreateAsync(string userId, string title, CancellationToken ct)
        {
            CreatedCalendarItem result = null;
            var token = await userTokenStore.GetAsync(userId, ct);

            if (token == null)
            {
                throw new AuthenticationException("User token not found!");
            }

            var parsedResult = await calendarEntryParser.ParseAsync(title, ct);

            if (parsedResult.Type == MessageType.Task && parsedResult.Task != null)
            {
                var task = new CalendarTask(parsedResult.Task.Title, parsedResult.Task.Due, false);
                var createdTask = await calendarClient.CreateTaskAsync(task, token, ct);
                result = new CreatedCalendarItem(MessageType.Task, createdTask.Title, createdTask.Due, null);
            }
            else if (parsedResult.Type == MessageType.Meeting && parsedResult.Meeting != null)
            {
                var meeting = new CalendarMeeting(parsedResult.Meeting.Title, parsedResult.Meeting.Start, parsedResult.Meeting.End);
                var createdMeeting = await calendarClient.CreateMeetingAsync(meeting, token, ct);
                result = new CreatedCalendarItem(MessageType.Meeting, createdMeeting.Title, createdMeeting.Start, createdMeeting.End);
            }

            return result;
        }
    }
}
