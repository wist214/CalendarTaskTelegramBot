using MediatR;

namespace CalendarEvent.Application.Commands
{
    public record LogoutCommand(string UserId) : IRequest;
}
