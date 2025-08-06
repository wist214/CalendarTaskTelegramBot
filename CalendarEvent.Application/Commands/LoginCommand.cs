using MediatR;

namespace CalendarEvent.Application.Commands
{
    public record LoginCommand(string UserId, long ChatId) : IRequest;
}
