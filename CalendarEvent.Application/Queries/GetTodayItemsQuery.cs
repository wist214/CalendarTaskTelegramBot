using CalendarEvent.Application.DTOs;
using MediatR;

namespace CalendarEvent.Application.Queries
{
    public record GetTodayItemsQuery(Guid UserId, DateTime Date) : IRequest<List<CalendarItemDto>>;
}
