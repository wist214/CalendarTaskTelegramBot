using CalendarEvent.Application.Services;
using CalendarEvent.Application.Services.Models;
using CalendarEvent.Infrastructure.Settings;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.Extensions.Options;

namespace CalendarEvent.Infrastructure.Auth
{
    public class GoogleAuthCallbackHandler(
        IUserTokenStore tokenStore,
        IMessageSender telegram)
        : IAuthCallbackHandler
    {
        private readonly GoogleSettings _settings = new();

        public async Task HandleCallbackAsync(string code, string state, CancellationToken ct)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _settings.ClientId,
                    ClientSecret = _settings.ClientSecret!
                }
            });

            var token = await flow.ExchangeCodeForTokenAsync(
                userId: state,
                code: code,
                redirectUri: _settings.RedirectUri!,
                CancellationToken.None);

            var tokens = new UserToken(
                token.AccessToken!,
                token.RefreshToken!,
                DateTime.UtcNow.AddSeconds(token.ExpiresInSeconds ?? 3600));

            await tokenStore.StoreAsync(state, tokens, CancellationToken.None);

            // 3) Уведомляем пользователя в Telegram
            if (long.TryParse(state, out var chatId))
            {
                await telegram.SendMessageAsync(chatId, "✅ Вы успешно авторизовались и теперь я могу создавать задачи в вашем Google Календаре!", null,
                    ct
                );
            }
        }
    }
}
