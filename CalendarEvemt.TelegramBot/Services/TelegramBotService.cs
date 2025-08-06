using CalendarEvent.Application.Commands;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums; 

namespace CalendarEvent.TelegramBot.Services
{
    public class TelegramBotService(IConfiguration config, IMediator mediator)
        : BackgroundService
    {
        private readonly TelegramBotClient _botClient = new(config["Telegram:BotToken"]);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
                cancellationToken: stoppingToken);
            return Task.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
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

        private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
        {
            Console.Error.WriteLine(exception);
            return Task.CompletedTask;
        }
    }
}
