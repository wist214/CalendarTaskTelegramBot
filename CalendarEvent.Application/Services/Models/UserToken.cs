using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarEvent.Application.Services.Models
{
    public class UserToken(string AccessToken, string RefreshToken, DateTime ExpiresAt)
    {
        public string AccessToken { get; } = AccessToken;
        public string RefreshToken { get; } = RefreshToken;
        public DateTime ExpiresAt { get; } = ExpiresAt;
    }
}
