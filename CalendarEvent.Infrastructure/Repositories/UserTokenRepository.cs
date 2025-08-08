using Azure;
using Azure.Data.Tables;
using CalendarEvent.Application.Services;
using CalendarEvent.Application.Services.Models;

namespace CalendarEvent.Infrastructure.Repositories
{
    public class UserTokenRepository : IUserTokenRepository // In-memory implementation for test purposes
    {
        private readonly TableClient _table;
        public UserTokenRepository(TableServiceClient svc)
        {
            _table = svc.GetTableClient("UserTokens");
            _table.CreateIfNotExists();
        }
        public async Task StoreAsync(string userId, UserToken token, CancellationToken ct)
        {
            var tokenEntity = new UserTokenEntity()
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresAt = token.ExpiresAt,
                PartitionKey = userId,
                RowKey = userId
            };

            await _table.UpsertEntityAsync(tokenEntity, TableUpdateMode.Replace, ct);
        }

        public async Task<UserToken?> GetAsync(string userId, CancellationToken ct)
        {
            try
            {
                var resp = await _table.GetEntityAsync<UserTokenEntity>(userId, userId, cancellationToken: ct);
                var token = new UserToken(resp.Value.AccessToken, resp.Value.RefreshToken, resp.Value.ExpiresAt);
                return token;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }


        public async Task DeleteAsync(string userId, CancellationToken ct)
        {
            try
            {
                await _table.DeleteEntityAsync(userId, userId, ETag.All, ct);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
            }
        }
    }

    public class UserTokenEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; } 
        public DateTime ExpiresAt { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
