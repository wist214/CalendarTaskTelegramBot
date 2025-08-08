using CalendarEvent.Application.Commands;
using MediatR;
using Telegram.Bot.Types;

namespace CalendarEvent.TelegramBot.Services
{
    public interface ITelegramBotService
    {
        Task HandleUpdateAsync(Update update, CancellationToken ct);
    }

    public class TelegramBotService(IMediator mediator) : ITelegramBotService
    {

        public async Task HandleUpdateAsync(Update update, CancellationToken ct)
        {
            var message = update.Message?.Text;
             if (message != null)
            {
                var userId = update.Message.From?.Id.ToString() ?? "unknown_user";
                if (message == "/login")
                {
                    var chatId = update.Message.Chat.Id;
                    await mediator.Send(new LoginCommand(userId, chatId), ct);
                }
                else if (message == "/logout")
                {
                    await mediator.Send(new LogoutCommand(userId), ct);
                }
                else
                {
                    await mediator.Send(new CreateCalendarItemCommand(userId, update.Message.Chat.Id,message), ct);
                }
            }
        }
    }
}
