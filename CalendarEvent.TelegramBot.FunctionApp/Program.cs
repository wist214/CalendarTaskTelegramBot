using System;
using Azure.Data.Tables;
using CalendarEvent.Application.Extensions;
using CalendarEvent.Infrastructure.Extensions;
using CalendarEvent.Infrastructure.Settings;
using CalendarEvent.TelegramBot.FunctionApp;
using CalendarEvent.TelegramBot.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();

builder.Services.AddApplication();

builder.Services.AddInfrastructure("test");

//builder.Services.Configure<GoogleSettings>(builder.Configuration.GetSection("Google"));

builder.Services.Configure<TelegramSettings>(builder.Configuration.GetSection("Telegram"));

builder.Services.AddSingleton<ITelegramBotClient>(_ =>
    new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")));

builder.Services.AddHostedService<WebhookSetupService>();

var storageConn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
if (string.IsNullOrWhiteSpace(storageConn))
    throw new InvalidOperationException("Не найдена настройка AzureWebJobsStorage");
builder.Services.AddSingleton(sp => new TableServiceClient(storageConn));

builder.Build().Run();
