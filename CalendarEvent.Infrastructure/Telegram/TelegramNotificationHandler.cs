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
            
            var message = notification.Type == MessageType.Task 
                ? $"✅ Задача [{notification.Title}] создана на {notification.Date.LocalDateTime:dd.MM.yyyy HH:mm}" 
                : $"✅ Событие [{notification.Title}] создано на {notification.Date.LocalDateTime:dd.MM.yyyy HH:mm}";

            await sender.SendMessageAsync(notification.ChatId, message, null, ct);
        }

        public async Task Handle(CalendarItemFailedNotification notification, CancellationToken ct)
        {
            var message = $"❌ Ошибка при создании: {notification.Title} {Environment.NewLine}{notification.ErrorMessage}";
            await sender.SendMessageAsync(notification.ChatId, message, null, ct);
        }
    }
}
