using CalendarEvent.Application.Notifications;
using CalendarEvent.Application.Services;
using CalendarEvent.Infrastructure.Auth;
using CalendarEvent.Infrastructure.Calendar;
using CalendarEvent.Infrastructure.Repositories;
using CalendarEvent.Infrastructure.Services;
using CalendarEvent.Infrastructure.Telegram;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CalendarEvent.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            // EF Core
            //services.AddDbContext<TokensDbContext>(opt => opt.UseSqlServer(connectionString));

            // Token store
            services.AddSingleton<IUserTokenStore, UserTokenStore>();
            // or services.AddSingleton<IUserTokenStore, InMemoryUserTokenStore>();

            // Google Calendar API client and facade
            services.AddSingleton<GoogleCalendarClient>();
            services.AddSingleton<ICalendarEventProvider, CalendarEventStore>();
            services.AddSingleton<ICalendarEventStore, CalendarEventStore>();
            services.AddSingleton<IUserTokenRepository, UserTokenRepository>();
            // Map domain ICalendarClient to thin client
            services.AddSingleton<ICalendarClient, GoogleCalendarClient>();
            services.AddSingleton<IAuthCallbackHandler, GoogleAuthCallbackHandler>();
            services.AddSingleton<IAuthUrlProvider, GoogleAuthUrlProvider>();
            services.AddSingleton<IMessageSender, TelegramApiClient>();
            services.AddSingleton<IAuthService, TelegramAuthService>();
            services.AddSingleton<ICalendarEntryParser, CalendarEntryParser>();

            // Notification handlers
            services.AddTransient<INotificationHandler<CalendarItemCreatedNotification>, TelegramNotificationHandler>();
            services.AddTransient<INotificationHandler<CalendarItemFailedNotification>, TelegramNotificationHandler>();
            services.AddTransient<INotificationHandler<VoiceTranscribedNotification>, TelegramNotificationHandler>();

            // Telegram services
            services.AddSingleton<ITelegramFileService, TelegramFileService>();

            // Speech services - use mock by default, can be overridden in production
            var useAzureSpeech = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY"));
            if (useAzureSpeech)
            {
                services.AddHttpClient();
                services.AddSingleton<ISpeechToTextService, Speech.AzureSpeechToTextService>();
            }
            else
            {
                services.AddSingleton<ISpeechToTextService, Speech.MockSpeechToTextService>();
            }

            return services;
        }
    }
}
