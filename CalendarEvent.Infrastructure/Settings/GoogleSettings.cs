namespace CalendarEvent.Infrastructure.Settings
{
    public class GoogleSettings
    {
        public string ClientId
            => Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
               ?? throw new InvalidOperationException("Environment variable 'GOOGLE_CLIENT_ID' is not set.");

        public string ClientSecret
            => Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
               ?? throw new InvalidOperationException("Environment variable 'GOOGLE_CLIENT_SECRET' is not set.");

        public string RedirectUri
            => Environment.GetEnvironmentVariable("GOOGLE_REDIRECT_URI")
               ?? throw new InvalidOperationException("Environment variable 'GOOGLE_REDIRECT_URI' is not set.");

        public string ApplicationName
            => Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_NAME")
               ?? throw new InvalidOperationException("Environment variable 'GOOGLE_APPLICATION_NAME' is not set.");
    }
}
