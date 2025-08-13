using MediatR;

namespace CalendarEvent.Application.Commands
{
    public record SendHelpCommand(string UserId, long ChatId) : IRequest;
}
