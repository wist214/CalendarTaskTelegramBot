using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;

namespace CalendarEvent.TelegramBot.FunctionApp
{
    public class WebhookSetupService : IHostedService
    {
        private readonly ITelegramBotClient _bot;
        private readonly string _webhookUrl;

        public WebhookSetupService(ITelegramBotClient bot)
        {
            _bot = bot;
            _webhookUrl = Environment.GetEnvironmentVariable("WEBHOOK_URL");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Устанавливаем webhook один раз при старте
            await _bot.SetWebhook(
                url: _webhookUrl,
                allowedUpdates: Array.Empty<UpdateType>(), // все апдейты
                cancellationToken: cancellationToken
            );
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
