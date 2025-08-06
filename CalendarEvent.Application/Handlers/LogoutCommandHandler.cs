using CalendarEvent.Application.Commands;
using CalendarEvent.Application.Services;
using MediatR;

namespace CalendarEvent.Application.Handlers
{
    public class LogoutCommandHandler(IAuthService telegramLoginService) : IRequestHandler<LogoutCommand>
    {
        public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            await telegramLoginService.LogoutAsync(request.UserId, cancellationToken);
        }
    }
}
