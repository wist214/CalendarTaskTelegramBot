using CalendarEvent.Application.Services;
using CalendarEvent.Application.Services.Models;
using CalendarEvent.Domain.Entities;
using CalendarEvent.Infrastructure.Settings;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Microsoft.Extensions.Options;

namespace CalendarEvent.Infrastructure.Calendar
{
    public class GoogleCalendarClient(IOptions<GoogleSettings> options) : ICalendarClient
    {
        private readonly GoogleSettings _settings = options.Value;
        public async Task<CalendarTask> CreateTaskAsync(CalendarTask task, UserToken token, CancellationToken ct)
        {
            var tasksService = InitializeTaskService(token);

            var gTask = new Google.Apis.Tasks.v1.Data.Task
            {
                Title = task.Title,
                Due = task.Due.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")
            };

            var created = await tasksService.Tasks.Insert(gTask, "@default").ExecuteAsync(ct);

            return new CalendarTask(created.Title, DateTime.Parse(created.Due), false);
        }

        public async Task<CalendarMeeting> CreateMeetingAsync(CalendarMeeting meeting, UserToken token, CancellationToken ct)
        {
            var gEvent = new Event
            {
                Summary = meeting.Title,
                Start = new EventDateTime { DateTimeDateTimeOffset = meeting.Start },
                End = new EventDateTime { DateTimeDateTimeOffset = meeting.End }
            };

            var service = InitializeCalendarService(token);
            var created = await service.Events.Insert(gEvent, "primary").ExecuteAsync(ct);
            return new CalendarMeeting(created.Description, created.Start.DateTimeDateTimeOffset.Value, created.End.DateTimeDateTimeOffset.Value);
        }

        public Task<List<CalendarTask>> GetTasksAsync(DateTimeOffset date, UserToken token, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        private CalendarService InitializeCalendarService(UserToken token)
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresInSeconds = (long)(token.ExpiresAt - DateTime.UtcNow).TotalSeconds
            };

            var flow = CreateFlow();

            var credential = new UserCredential(flow, "q", tokenResponse);

            return new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _settings.ApplicationName
            });
        }

        private TasksService InitializeTaskService(UserToken token)
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
            };

            var flow = CreateFlow();
            var credential = new UserCredential(flow, String.Empty, tokenResponse);

            var taskService = new TasksService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _settings.ApplicationName
            });

            return taskService;
        }

        private GoogleAuthorizationCodeFlow CreateFlow()
        {
            return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _settings.ClientId,
                    ClientSecret = _settings.ClientSecret
                },
                Scopes =
                [
                    CalendarService.Scope.Calendar,
                    TasksService.Scope.Tasks 
                ],
            });
        }
    }
}
