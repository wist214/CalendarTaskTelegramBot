using CalendarEvent.Application.Services.Models;
using MediatR;

namespace CalendarEvent.Application.Commands
{
    public record CreateCalendarItemCommand(string UserId, long ChatId, string Title) : IRequest;
}
