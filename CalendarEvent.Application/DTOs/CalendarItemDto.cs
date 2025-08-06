namespace CalendarEvent.Application.DTOs
{
    public class CalendarItemDto
    {
        public string Title { get; init; } = null!;
        public DateTimeOffset Start { get; init; }
        public DateTimeOffset? End { get; init; }
        public CalendarItemType Type { get; init; }
    }

    public enum CalendarItemType
    {
        Task = 1,
        Meeting = 2,
    }
}
