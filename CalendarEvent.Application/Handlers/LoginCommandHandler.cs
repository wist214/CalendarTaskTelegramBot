using CalendarEvent.Application.Commands;
using CalendarEvent.Application.Services;
using MediatR;

namespace CalendarEvent.Application.Handlers
{
    public class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand>
    {
        public async Task Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            await authService.LoginAsync(request.UserId, request.ChatId, cancellationToken);
        }
    }
}
