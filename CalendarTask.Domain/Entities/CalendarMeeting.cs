namespace CalendarEvent.Domain.Entities
{
    public class CalendarMeeting(string title, DateTimeOffset start, DateTimeOffset end)
    {
        public string Title { get; } = title;

        public DateTimeOffset Start { get; } = start;

        public DateTimeOffset End { get;} = end;
    }
}
