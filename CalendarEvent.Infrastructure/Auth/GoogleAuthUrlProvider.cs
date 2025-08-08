using CalendarEvent.Application.Services;
using CalendarEvent.Infrastructure.Settings;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Calendar.v3;
using Google.Apis.Tasks.v1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CalendarEvent.Infrastructure.Auth
{
    public class GoogleAuthUrlProvider : IAuthUrlProvider
    {
        private readonly GoogleSettings _settings;
        private readonly ILogger<GoogleAuthUrlProvider> _logger;

        public GoogleAuthUrlProvider(
            ILogger<GoogleAuthUrlProvider> logger)
        {
            _settings = new();
            _logger = logger;
        }

        public string GetAuthorizationUrl(string userId)
        {
            // Собираем redirectUri
            var host = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            var redirectUri = $"https://{host}/api/oauth2callback";

            // Логируем его для отладки
            _logger.LogInformation("Using Google OAuth redirect URI: {RedirectUri}", redirectUri);

            // Настраиваем flow
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _settings.ClientId,
                    ClientSecret = _settings.ClientSecret
                },
                Scopes = new[]
                {
                    CalendarService.Scope.Calendar,
                    TasksService.Scope.Tasks
                }
            });

            var authRequest = flow.CreateAuthorizationCodeRequest(redirectUri)
                                  as GoogleAuthorizationCodeRequestUrl
                              ?? throw new InvalidOperationException("Ожидался GoogleAuthorizationCodeRequestUrl");

            authRequest.AccessType = "offline";   // offline-доступ (refresh-token)
            authRequest.Prompt = "consent";   // принудительно показать форму согласия
            authRequest.State = userId;

            var authorizationUrl = authRequest.Build().AbsoluteUri;

            //// Создаём запрос с этим redirectUri
            //var request = flow.CreateAuthorizationCodeRequest(redirectUri);
            //request.State = userId;

            //var url = request.Build().AbsoluteUri;

            //// И логируем готовый URL (можно скрыть секреты)
            //_logger.LogDebug("Built Google OAuth URL: {Url}", url);

            return authorizationUrl;
        }
    }
}
