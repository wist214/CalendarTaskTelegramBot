using CalendarEvent.Application.Extensions;
using CalendarEvent.Infrastructure.Extensions;
using CalendarEvent.Infrastructure.Settings;
using CalendarEvent.TelegramBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Сначала Application (регистрируются все IRequestHandler-ы)
builder.Services.AddApplication();

// ПОТОМ Infrastructure (регистрация GoogleCalendarClient, ICalendarClient, IUserTokenStore и т. д.)
builder.Services.AddInfrastructure("test");

// И, наконец, Presentation
builder.Services.AddControllers();
builder.Services.AddHostedService<TelegramBotService>();

builder.Services.Configure<GoogleSettings>(
    builder.Configuration.GetSection("Google")
);

builder.Services.Configure<TelegramSettings>(
    builder.Configuration.GetSection("Telegram")
);

var app = builder.Build();
app.MapControllers();
await app.RunAsync();