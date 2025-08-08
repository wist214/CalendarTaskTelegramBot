using CalendarEvent.Application.Extensions;
using CalendarEvent.Infrastructure.Extensions;
using CalendarEvent.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();

builder.Services.AddInfrastructure("test");

builder.Services.AddControllers();

//builder.Services.Configure<GoogleSettings>(
//    builder.Configuration.GetSection("Google")
//);

builder.Services.Configure<TelegramSettings>(
    builder.Configuration.GetSection("Telegram")
);

var app = builder.Build();
app.MapControllers();
await app.RunAsync();