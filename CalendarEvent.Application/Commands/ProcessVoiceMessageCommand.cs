using MediatR;

namespace CalendarEvent.Application.Commands
{
    public record ProcessVoiceMessageCommand(
        string UserId, 
        long ChatId, 
        string FileId, 
        int Duration) : IRequest;
}
