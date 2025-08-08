using CalendarEvent.Application.Services;
using CalendarEvent.Application.Services.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CalendarEvent.Infrastructure.Telegram
{
    public class TelegramApiClient: IMessageSender
    {
        private readonly TelegramBotClient _client;
        
        public TelegramApiClient()
        {
            _client = new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN"));
        }
        
        public async Task SendMessageAsync(long chatId, string message, KeyboardMarkup? markup, CancellationToken ct = default)
        {
            InlineKeyboardMarkup? keyboard = null;
            if (markup != null)
            {
                keyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl(markup.Text, markup.Url));
            }

            await _client.SendMessage(chatId, message, ParseMode.Markdown, replyMarkup: keyboard, cancellationToken:ct);
        }
    }
}
