using CalendarEvent.Application.Services;
using CalendarEvent.Application.Services.Models;
using CalendarEvent.Domain.Entities;
using CalendarEvent.Infrastructure.Settings;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Microsoft.Extensions.Logging;

namespace CalendarEvent.Infrastructure.Calendar
{
    public class GoogleCalendarClient(ILogger<GoogleCalendarClient> logger) : ICalendarClient
    {
        private readonly GoogleSettings _settings = new();

        public async Task<CalendarTask> CreateTaskAsync(CalendarTask task, UserToken token, CancellationToken ct)
        {
            try
            {
                var tasksService = await InitializeTaskService(token, ct);

                var gTask = new Google.Apis.Tasks.v1.Data.Task
                {
                    Title = task.Title,
                    Due = task.Due.LocalDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")
                };

                var created = await tasksService.Tasks.Insert(gTask, "@default").ExecuteAsync(ct);

                return new CalendarTask(created.Title, DateTime.Parse(created.Due), false);
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while creating a meeting, ex={ex}");
                throw;
            }
        }

        public async Task<CalendarMeeting> CreateMeetingAsync(CalendarMeeting meeting, UserToken token, CancellationToken ct)
        {
            try
            {
                var service = await InitializeCalendarService(token, ct);

                var settings = await service.Settings.Get("timezone").ExecuteAsync(ct);
                var tz = TimeZoneInfo.FindSystemTimeZoneById(settings.Value);
                var correctedStart = ConvertToDateTimeOffset(meeting.Start.DateTime, tz);
                var correctedEnd = correctedStart.AddHours(1);

                var gEvent = new Event
                {
                    Summary = meeting.Title,
                    Start = new EventDateTime { DateTimeDateTimeOffset= correctedStart},
                    End = new EventDateTime { DateTimeDateTimeOffset= correctedEnd }
                };

                var created = await service.Events.Insert(gEvent, "primary").ExecuteAsync(ct);

                logger.LogInformation($" [1]{created.Start}, [2]{created.Start.DateTimeDateTimeOffset.Value}, [3]{created.Start.DateTimeDateTimeOffset.Value.LocalDateTime}, [4]{created.Start.DateTimeDateTimeOffset.Value.DateTime}, [5]{created.Start.DateTime}");
                return new CalendarMeeting(meeting.Title, created.Start.DateTimeDateTimeOffset.Value.DateTime, created.End.DateTimeDateTimeOffset.Value.DateTime);
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while creating a meeting, ex={ex}");
                throw;
            }
        }

        public Task<List<CalendarTask>> GetTasksAsync(DateTimeOffset date, UserToken token, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        private DateTimeOffset ConvertToDateTimeOffset(DateTime dateTime, TimeZoneInfo tz)
        {
            var offset = tz.GetUtcOffset(dateTime);
            return new DateTimeOffset(dateTime, offset);
        }

        private async Task<CalendarService> InitializeCalendarService(UserToken token, CancellationToken ct)
        {
            var creds = await CreateCredentialAsync(token, ct);
            return new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = creds,
                ApplicationName = _settings.ApplicationName
            });
        }

        private async Task<TasksService> InitializeTaskService(UserToken token, CancellationToken ct)
        {
            var creds = await CreateCredentialAsync(token, ct);

            return new TasksService(new BaseClientService.Initializer
            {
                HttpClientInitializer = creds,
                ApplicationName = _settings.ApplicationName
            });
        }

        private async Task<UserCredential> CreateCredentialAsync(UserToken token, CancellationToken ct)
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                IssuedUtc = DateTime.UtcNow,
                ExpiresInSeconds = (long)Math.Max(0, (token.ExpiresAt - DateTime.UtcNow).TotalSeconds)
            };

            var flow = CreateFlow();

            var credential = new UserCredential(flow, "user", tokenResponse);

            var expiresAtUtc = tokenResponse.IssuedUtc.AddSeconds(tokenResponse.ExpiresInSeconds.Value);
            if (DateTime.UtcNow >= expiresAtUtc.AddSeconds(-60))
            {
                var success = Task.Run(async () => await credential.RefreshTokenAsync(CancellationToken.None)).Result;
                if (!success)
                {
                    throw new InvalidOperationException("Не удалось обновить access token через refresh token");
                }
            }

            return credential;
        }

        private UserCredential CreateCredential(UserToken token)
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                IssuedUtc = DateTime.UtcNow,
                ExpiresInSeconds = (long)Math.Max(0, (token.ExpiresAt - DateTime.UtcNow).TotalSeconds)
            };


            var flow = CreateFlow();
            return new UserCredential(flow, "user", tokenResponse);
        }

        private GoogleAuthorizationCodeFlow CreateFlow()
        {
            logger.LogError($"ClientID={_settings.ClientId}, ClientSecret={_settings.ClientSecret}");

            return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _settings.ClientId,
                    ClientSecret = _settings.ClientSecret,
                },
                Scopes = new[]
                {
                    CalendarService.Scope.Calendar,
                    TasksService.Scope.Tasks,
                    TasksService.Scope.TasksReadonly
                },
                Prompt = "consent",  
            });
        }
    }
}
