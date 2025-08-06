using CalendarEvent.Application.Services.Models;

namespace CalendarEvent.Application.Services
{
    public interface IMessageSender
    {
        Task SendMessageAsync(long chatId, string message, KeyboardMarkup? markup, CancellationToken ct = default);
    }
}
