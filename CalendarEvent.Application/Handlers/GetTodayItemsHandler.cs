using CalendarEvent.Application.DTOs;
using CalendarEvent.Application.Queries;
using CalendarEvent.Application.Services;
using MediatR;

namespace CalendarEvent.Application.Handlers
{
    public class GetTodayItemsHandler(ICalendarEventProvider calendarEventProvider) : IRequestHandler<GetTodayItemsQuery, List<CalendarItemDto>>
    {
        public async Task<List<CalendarItemDto>> Handle(GetTodayItemsQuery query, CancellationToken cancellationToken)
        {
            return await calendarEventProvider.GetAsync(query.UserId, query.Date, cancellationToken);
        }
    }
}
