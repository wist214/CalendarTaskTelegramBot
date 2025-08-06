using CalendarEvent.Application.Services;
using CalendarEvent.Application.Services.Models;

namespace CalendarEvent.Infrastructure.Telegram
{
    public class TelegramAuthService(IUserTokenStore tokenStore, IAuthUrlProvider authUrlProvider, IMessageSender messageSender) : IAuthService
    {
        public async Task LoginAsync(string userId, long chatId, CancellationToken ct)
        {
            var existing = await tokenStore.GetAsync(userId, ct);
            if (existing != null)
            {
                await messageSender.SendMessageAsync(chatId, "Вы уже авторизованы. Если хотите выйти — /logout", null, ct);
                return;
            }

            var url = authUrlProvider.GetAuthorizationUrl(userId);
            var markup = new KeyboardMarkup("Войти через Google", url);
            await messageSender.SendMessageAsync(chatId,
                "Нажмите, чтобы войти через Google и связать свой календарь:", markup, ct);
        }

        public async Task LogoutAsync(string userId, CancellationToken ct)
        {
            await tokenStore.DeleteAsync(userId, ct);
        }
    }
}
