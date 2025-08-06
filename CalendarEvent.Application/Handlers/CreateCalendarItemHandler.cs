using System.Security.Authentication;
using CalendarEvent.Application.Commands;
using CalendarEvent.Application.Notifications;
using CalendarEvent.Application.Services;
using MediatR;

namespace CalendarEvent.Application.Handlers
{
    public class CreateCalendarItemHandler(ICalendarEventStore calendarEventStore, IMediator mediator) : IRequestHandler<CreateCalendarItemCommand>
    {
        public async Task Handle(CreateCalendarItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await calendarEventStore.CreateAsync(request.UserId, request.Title, cancellationToken);

                await mediator.Publish(new CalendarItemCreatedNotification(
                    request.UserId,
                    request.ChatId,
                    result.Title,
                    result.Type,
                    result.Start
                ), cancellationToken);
            }
            catch (AuthenticationException ex)
            {
                var errorMessage = "Необходимо авторизоваться для создания события в календаре";
                await mediator.Publish(new CalendarItemFailedNotification(
                    request.UserId,
                    request.ChatId,
                    request.Title,
                    errorMessage
                ), cancellationToken);
            }
            catch (Exception ex)
            {
                var errorMessage = "Что-то уж совсем странное произошло, даже не знаю, что ты сделал(а), но ничего не создалось";
                await mediator.Publish(new CalendarItemFailedNotification(
                    request.UserId,
                    request.ChatId,
                    request.Title,
                    errorMessage
                ), cancellationToken);
            }
        }
    }
}
