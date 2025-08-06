using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarEvent.Infrastructure.Settings
{
    public class GoogleSettings
    {
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string RedirectUri { get; set; } = null!;
        public string TokensFolder { get; set; } = null!;
        public string PostLoginRedirectUrl { get; set; } = null!;
        public string ApplicationName { get; set; }
    }
}
