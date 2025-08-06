using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarEvent.Application.Services.Models;

namespace CalendarEvent.Application.Services
{
    public interface IUserTokenStore
    {
        Task StoreAsync(string userId, UserToken tokens, CancellationToken ct);
        Task<UserToken?> GetAsync(string userId, CancellationToken ct);
        Task DeleteAsync(string userId, CancellationToken ct);
    }
}
