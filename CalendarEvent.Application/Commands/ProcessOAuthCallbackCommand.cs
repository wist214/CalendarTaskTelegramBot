using MediatR;

namespace CalendarEvent.Application.Commands
{
    public record ProcessOAuthCallbackCommand(string Code, string State) : IRequest;
}
