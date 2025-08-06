using CalendarEvent.Application.Services;
using CalendarEvent.Application.Services.Models;

namespace CalendarEvent.Infrastructure.Services
{
    public class UserTokenStore(IUserTokenRepository repository) : IUserTokenStore
    {
        public async Task StoreAsync(string userId, UserToken tokens, CancellationToken ct)
        {
            await repository.StoreAsync(userId, tokens, ct);
        }

        public async Task<UserToken?> GetAsync(string userId, CancellationToken ct)
        {
            return await repository.GetAsync(userId, ct);
        }

        public async Task DeleteAsync(string userId, CancellationToken ct)
        {
            await repository.DeleteAsync(userId, ct);
        }
    }
}
