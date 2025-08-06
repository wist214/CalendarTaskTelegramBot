namespace CalendarEvent.Application.Services
{
    public interface IAuthService
    {
        Task LoginAsync(string userId, long chatId, CancellationToken ct);
        Task LogoutAsync(string userId, CancellationToken ct);
    }
}
