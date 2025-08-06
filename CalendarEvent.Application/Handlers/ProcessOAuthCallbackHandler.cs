using CalendarEvent.Application.Commands;
using CalendarEvent.Application.Services;
using MediatR;

namespace CalendarEvent.Application.Handlers
{
    public class ProcessOAuthCallbackHandler(IAuthCallbackHandler authCallbackHandler) : IRequestHandler<ProcessOAuthCallbackCommand>
    {
        public Task Handle(ProcessOAuthCallbackCommand request, CancellationToken cancellationToken)
        {
            authCallbackHandler.HandleCallbackAsync(request.Code, request.State, cancellationToken);
            return Task.CompletedTask;
        }
    }
}
