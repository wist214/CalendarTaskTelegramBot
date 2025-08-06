using CalendarEvent.Application.Services.Models;

namespace CalendarEvent.Application.Services
{
    public interface IUserTokenRepository
    {
        Task StoreAsync(string userId, UserToken token, CancellationToken ct);
        Task<UserToken?> GetAsync(string userId, CancellationToken ct);
        Task DeleteAsync(string userId, CancellationToken ct);
    }
}
