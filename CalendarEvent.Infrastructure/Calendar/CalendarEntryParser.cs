using System.Text.RegularExpressions;
using CalendarEvent.Application.Services;
using CalendarEvent.Application.Services.Models;
using CalendarEvent.Domain.Entities;

namespace CalendarEvent.Infrastructure.Calendar
{
    public class CalendarEntryParser : ICalendarEntryParser
    {
        private static readonly Regex DatePattern = new Regex(
            @"\b(?:(?<dm>\d{1,2}[.]\d{1,2})|" +
            @"(?<word>сегодня|завтра|послезавтра)|" +
            @"(?<dow>понедельник|вторник|сред[ау]|четверг|пятниц[ау]|суббот[ау]|воскресенье))\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TimePattern = new Regex(
            @"\b(?:в|на)?\s*(?<hour>\d{1,2})(?::(?<min>\d{1,2}))?\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public Task<CalendarEntryParseResult> ParseAsync(string message, CancellationToken ct)
        {
            // 1) Найти dateMatch и timeMatch
            var dateMatch = DatePattern.Match(message);
            var timeMatch = TimePattern.Match(message);

            // 2) Получить конкретную дату (по умолчанию — сегодня)
            DateTime date = DateTime.Today;
            if (dateMatch.Success)
            {
                if (dateMatch.Groups["dm"].Success)
                {
                    var parts = dateMatch.Groups["dm"].Value.Split('.');
                    int day = int.Parse(parts[0]);
                    int mon = int.Parse(parts[1]);
                    date = new DateTime(DateTime.Today.Year, mon, day);
                }
                else if (dateMatch.Groups["word"].Success)
                {
                    switch (dateMatch.Groups["word"].Value.ToLower())
                    {
                        case "сегодня": date = DateTime.Today; break;
                        case "завтра": date = DateTime.Today.AddDays(1); break;
                        case "послезавтра": date = DateTime.Today.AddDays(2); break;
                    }
                }
                else if (dateMatch.Groups["dow"].Success)
                {
                    date = NextWeekday(DateTime.Today,
                        ParseDayOfWeek(dateMatch.Groups["dow"].Value));
                }
            }

            // 3) Получить конкретное время (по умолчанию — 00:00)
            TimeSpan time = TimeSpan.Zero;
            if (timeMatch.Success)
            {
                var h = int.Parse(timeMatch.Groups["hour"].Value);
                var m = timeMatch.Groups["min"].Success
                        ? int.Parse(timeMatch.Groups["min"].Value)
                        : 0;
                time = new TimeSpan(h, m, 0);
            }

            // 4) Составить итоговый DateTime
            var dateTime = date.Date + time;

            // 5) Очистить текст для summary
            var cleaned = DatePattern.Replace(message, "");
            cleaned = TimePattern.Replace(cleaned, "");
            cleaned = Regex.Replace(cleaned, @"\b(в|на)\b", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"[,\.\s]+", " ").Trim();
            if (!string.IsNullOrEmpty(cleaned))
                cleaned = char.ToUpper(cleaned[0]) + cleaned.Substring(1);

            // 6) Определить intent
            var isMeeting = dateMatch.Success || timeMatch.Success;
            var type = isMeeting ? MessageType.Meeting : MessageType.Task;

            // 7) Построить результат
            var result = new CalendarEntryParseResult
            {
                Type = type,
                Task = type == MessageType.Task
                    ? new CalendarTask(
                        cleaned,
                        dateTime,
                        false // или true, если хотите
                    )
                    : null,
                Meeting = type == MessageType.Meeting
                    ? new CalendarMeeting(
                    
                        cleaned,
                        dateTime,
                        dateTime.AddHours(1))
                    : null
            };

            return Task.FromResult(result);
        }

        private static DayOfWeek ParseDayOfWeek(string dow) =>
            dow.ToLower() switch
            {
                "понедельник" => DayOfWeek.Monday,
                "вторник" => DayOfWeek.Tuesday,
                "среда" => DayOfWeek.Wednesday,
                "среду" => DayOfWeek.Wednesday,
                "четверг" => DayOfWeek.Thursday,
                "пятница" => DayOfWeek.Friday,
                "пятницу" => DayOfWeek.Friday,
                "суббота" => DayOfWeek.Saturday,
                "субботу" => DayOfWeek.Saturday,
                "воскресенье" => DayOfWeek.Sunday,
                _ => throw new ArgumentOutOfRangeException(nameof(dow))
            };

        private static DateTime NextWeekday(DateTime start, DayOfWeek dow)
        {
            int delta = ((int)dow - (int)start.DayOfWeek + 7) % 7;
            if (delta == 0) delta = 7;
            return start.AddDays(delta);
        }
    }
}
