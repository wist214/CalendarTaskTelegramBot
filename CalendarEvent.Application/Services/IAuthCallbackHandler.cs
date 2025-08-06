using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarEvent.Application.Services
{
    public interface IAuthCallbackHandler
    {
        /// <param name="code">Авторизационный код из Google</param>
        /// <param name="state">Ваш state (userId)</param>
        /// <param name="ct">Token</param>
        Task HandleCallbackAsync(string code, string state, CancellationToken ct);
    }
}
