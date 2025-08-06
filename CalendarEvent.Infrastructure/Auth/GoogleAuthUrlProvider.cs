using CalendarEvent.Application.Services;
using CalendarEvent.Infrastructure.Settings;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Tasks.v1;
using Microsoft.Extensions.Options;

namespace CalendarEvent.Infrastructure.Auth
{
    public class GoogleAuthUrlProvider(IOptions<GoogleSettings> googleSettings) : IAuthUrlProvider
    {
        private readonly GoogleSettings _settings = googleSettings.Value;

        public string GetAuthorizationUrl(string userId)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _settings.ClientId,
                    ClientSecret = _settings.ClientSecret
                },
                Scopes = [CalendarService.Scope.Calendar, TasksService.Scope.Tasks]
            });

            var request = flow.CreateAuthorizationCodeRequest(_settings.RedirectUri);

            request.State = userId;

            return request.Build().AbsoluteUri;
        }
    }
}
