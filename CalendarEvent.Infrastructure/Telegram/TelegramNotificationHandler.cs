using CalendarEvent.Application.Notifications;
using CalendarEvent.Application.Services;
using CalendarEvent.Domain.Entities;
using MediatR;

namespace CalendarEvent.Infrastructure.Telegram
{
    public class TelegramNotificationHandler(IMessageSender sender) : INotificationHandler<CalendarItemCreatedNotification>,
        INotificationHandler<CalendarItemFailedNotification>
    {
        public async Task Handle(CalendarItemCreatedNotification notification, CancellationToken ct)
        {
            //var message = new OutgoingMessage
            //{
            //    Text = $"✅ Создано: {notification.Title}",
            //    ReplyMarkup = new InlineKeyboardMarkup(new[]
            //    {
            //        new InlineKeyboardButton("Открыть", notification.Url)
            //    })
            //};

            var message = notification.Type == MessageType.Task 
                ? $"✅ Задача создана: {notification.Title} на {notification.Date:dd.MM.yyyy HH:mm}" 
                : $"✅ Событие создано: {notification.Title} на {notification.Date:dd.MM.yyyy HH:mm}";

            await sender.SendMessageAsync(notification.ChatId, message, null, ct);
        }

        public async Task Handle(CalendarItemFailedNotification notification, CancellationToken ct)
        {
            var message = $"❌ Ошибка при создании: {notification.Title} {Environment.NewLine}{notification.ErrorMessage}";
            await sender.SendMessageAsync(notification.ChatId, message, null, ct);
        }
    }
}
