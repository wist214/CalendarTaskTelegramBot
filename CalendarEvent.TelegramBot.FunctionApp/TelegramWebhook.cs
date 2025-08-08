using System.Text.Json;
using CalendarEvent.Infrastructure.Settings;
using CalendarEvent.TelegramBot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CalendarEvent.TelegramBot.FunctionApp;

public class TelegramWebhook(ITelegramBotService telegramBotService, IOptions<GoogleSettings> options2, ILogger<TelegramWebhook> logger)
{
    [Function("TelegramWebhook")]
    public async Task Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TelegramWebhook")] HttpRequest req,
        CancellationToken ct)
    {
        var settings = options2.Value;

        logger.LogInformation($"Settings={settings.ApplicationName}, {settings.ClientId}, {settings.RedirectUri}");
        var body = await new StreamReader(req.Body).ReadToEndAsync(ct);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var update = JsonSerializer.Deserialize<Update>(body, options);

        await telegramBotService.HandleUpdateAsync(update, ct);
    }
}