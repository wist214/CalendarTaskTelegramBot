using System.Text.RegularExpressions;
using CalendarEvent.Application.Services;
using CalendarEvent.Application.Services.Models;
using CalendarEvent.Domain.Entities;

namespace CalendarEvent.Infrastructure.Calendar
{
    public class CalendarEntryParser : ICalendarEntryParser
    {
        // Дата: dd.MM, dd.MM.yyyy, а также / и -
        // + слова + дни недели
        private static readonly Regex DatePattern = new(
            @"\b(?<date>(?<day>\d{1,2})[.\-/](?<mon>\d{1,2})(?:[.\-/](?<year>\d{2,4}))?)\b" +
            @"|(?<word>\bсегодня\b|\bзавтра\b|\bпослезавтра\b|\bпослепослезавтра\b)" +
            @"|(?<dow>\bпонедельник\b|\bвторник\b|\bсред[ау]\b|\bчетверг\b|\bпятниц[ау]\b|\bсуббот[ау]\b|\bвоскресенье\b)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Время с минутами: HH:MM или H:MM (только двоеточие, чтобы не путать с 22.08)
        private static readonly Regex TimeExactPattern = new(
            @"\b(?<hour>[01]?\d|2[0-3]):(?<min>[0-5]\d)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Время-час: "в 16", "на 9"
        private static readonly Regex TimeHourWithPrepPattern = new(
            @"\b(?:в|на)\s*(?<hour>[01]?\d|2[0-3])\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Одиночное время-час без предлога: "16"
        // Гарантируем, что это НЕ часть даты вида "22.08" / "22-08" / "22/08" и т.п.
        private static readonly Regex TimeHourBarePattern = new(
            @"(?<!\d[.\-/])\b(?<hour>[01]?\d|2[0-3])\b(?!\s*[.\-/]\s*\d)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public Task<CalendarEntryParseResult> ParseAsync(string message, CancellationToken ct)
        {
            // 1) Находим дату (первое совпадение)
            var dateMatch = DatePattern.Matches(message).Cast<Match>().FirstOrDefault(m => m.Success);

            DateTime date = DateTime.Today;
            if (dateMatch != null)
            {
                if (dateMatch.Groups["date"].Success)
                {
                    int day = int.Parse(dateMatch.Groups["day"].Value);
                    int mon = int.Parse(dateMatch.Groups["mon"].Value);
                    int year = DateTime.Today.Year;

                    if (dateMatch.Groups["year"].Success)
                    {
                        var y = dateMatch.Groups["year"].Value;
                        year = y.Length == 2 ? 2000 + int.Parse(y) : int.Parse(y);
                    }

                    date = new DateTime(year, mon, day);
                }
                else if (dateMatch.Groups["word"].Success)
                {
                    date = dateMatch.Groups["word"].Value.ToLower() switch
                    {
                        "сегодня" => DateTime.Today,
                        "завтра" => DateTime.Today.AddDays(1),
                        "послезавтра" => DateTime.Today.AddDays(2),
                        "послепослезавтра" => DateTime.Today.AddDays(3),
                        _ => DateTime.Today
                    };
                }
                else if (dateMatch.Groups["dow"].Success)
                {
                    date = NextWeekday(DateTime.Today, ParseDayOfWeek(dateMatch.Groups["dow"].Value));
                }
            }

            // 2) Находим время. Приоритет: HH:MM -> "в/на 16" -> голое "16".
            bool Overlaps(Match? a, Match? b) =>
                a != null && b != null && !(a.Index + a.Length <= b.Index || b.Index + b.Length <= a.Index);

            TimeSpan time = TimeSpan.Zero;
            Match? timeMatch = TimeExactPattern.Matches(message).Cast<Match>()
                .FirstOrDefault(m => dateMatch == null || !Overlaps(m, dateMatch));

            if (timeMatch != null)
            {
                var h = int.Parse(timeMatch.Groups["hour"].Value);
                var mm = int.Parse(timeMatch.Groups["min"].Value);
                time = new TimeSpan(h, mm, 0);
            }
            else
            {
                timeMatch = TimeHourWithPrepPattern.Matches(message).Cast<Match>()
                    .FirstOrDefault(m => dateMatch == null || !Overlaps(m, dateMatch));

                if (timeMatch != null)
                {
                    var h = int.Parse(timeMatch.Groups["hour"].Value);
                    time = new TimeSpan(h, 0, 0);
                }
                else
                {
                    timeMatch = TimeHourBarePattern.Matches(message).Cast<Match>()
                        .FirstOrDefault(m => dateMatch == null || !Overlaps(m, dateMatch));

                    if (timeMatch != null)
                    {
                        var h = int.Parse(timeMatch.Groups["hour"].Value);
                        time = new TimeSpan(h, 0, 0);
                    }
                }
            }

            var dateTime = date.Date + time;

            var spans = new List<(int start, int length)>();
            if (dateMatch != null) spans.Add((dateMatch.Index, dateMatch.Length));
            if (timeMatch != null)
            {
                int start = timeMatch.Index;
                int len = timeMatch.Length;

                // прихватываем предлог "в"/"на" + пробел перед временем
                var pre = Regex.Match(message[..start], @"(?<=^|\s)(в|на)\s*$", RegexOptions.IgnoreCase);
                if (pre.Success)
                {
                    start = pre.Index;
                    len += timeMatch.Index - pre.Index;
                }
                spans.Add((start, len));
            }

            string cleaned = RemoveSpans(message, spans);

            // 1) прибираем одиночные «0», которые могли остаться как мусор
            cleaned = Regex.Replace(cleaned, @"(?<=^|\s)0(?=\s|$)", " ", RegexOptions.IgnoreCase);

            // 2) нормализуем пробелы/знаки
            cleaned = Regex.Replace(cleaned, @"[,\s]+", " ").Trim();

            // 3) капитализация первой буквы
            if (!string.IsNullOrEmpty(cleaned))
                cleaned = char.ToUpper(cleaned[0]) + cleaned[1..];

            // 4) Intent
            var isMeeting = dateMatch != null || timeMatch != null;
            var type = isMeeting ? MessageType.Meeting : MessageType.Task;

            var result = new CalendarEntryParseResult
            {
                Type = type,
                Task = type == MessageType.Task ? new CalendarTask(cleaned, dateTime, false) : null,
                Meeting = type == MessageType.Meeting ? new CalendarMeeting(cleaned, dateTime, dateTime.AddHours(1)) : null
            };

            return Task.FromResult(result);
        }

        private static string RemoveSpans(string text, List<(int start, int length)> spans)
        {
            if (spans.Count == 0) return text;
            var ordered = spans
                .Where(s => s.start >= 0 && s.start + s.length <= text.Length)
                .OrderByDescending(s => s.start)
                .ToList();

            var sb = new System.Text.StringBuilder(text);
            foreach (var (start, length) in ordered) sb.Remove(start, length);
            return sb.ToString();
        }

        private static DayOfWeek ParseDayOfWeek(string dow) =>
            dow.ToLower() switch
            {
                "понедельник" => DayOfWeek.Monday,
                "вторник" => DayOfWeek.Tuesday,
                "среда" or "среду" => DayOfWeek.Wednesday,
                "четверг" => DayOfWeek.Thursday,
                "пятница" or "пятницу" => DayOfWeek.Friday,
                "суббота" or "субботу" => DayOfWeek.Saturday,
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
