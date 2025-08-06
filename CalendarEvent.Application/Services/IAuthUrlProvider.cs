namespace CalendarEvent.Application.Services
{
    public interface IAuthUrlProvider
    {
        string GetAuthorizationUrl(string userId);
    }
}
