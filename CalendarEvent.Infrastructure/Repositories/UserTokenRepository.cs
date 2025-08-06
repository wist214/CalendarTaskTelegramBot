using CalendarEvent.Application.Services;
using CalendarEvent.Application.Services.Models;

namespace CalendarEvent.Infrastructure.Repositories
{
    public class UserTokenRepository : IUserTokenRepository // In-memory implementation for test purposes
    {
        private readonly Dictionary<string, UserToken> _tokens = new();

        public Task StoreAsync(string userId, UserToken token, CancellationToken ct)
        {
            _tokens.Add(userId, token);
            return Task.FromResult(token);
        }

        public Task<UserToken?> GetAsync(string userId, CancellationToken ct)
        {
            _tokens.TryGetValue(userId, out var token);
            return Task.FromResult(token);
        }

        public Task DeleteAsync(string userId, CancellationToken ct)
        {
           _tokens.Remove(userId);
            return Task.CompletedTask;
        }
    }
}
