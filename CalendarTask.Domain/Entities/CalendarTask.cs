namespace CalendarEvent.Domain.Entities
{
    public class CalendarTask(string title, DateTimeOffset due, bool isCompleted)
    {
        public string Title { get; } = title;

        public DateTimeOffset Due { get; } = due;

        public bool IsCompleted { get;} = isCompleted;
    }
}
