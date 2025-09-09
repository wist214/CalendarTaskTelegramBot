using System.Text.RegularExpressions;
using CalendarEvent.Application.Services;
using CalendarEvent.Application.Services.Models;
using CalendarEvent.Domain.Entities;

namespace CalendarEvent.Infrastructure.Calendar
{
    public class CalendarEntryParser : ICalendarEntryParser
    {
        // Дата: dd.MM, dd.MM.yyyy, а также / и -
        // + слова + дни недели (полные и сокращенные)
        private static readonly Regex DatePattern = new(
            @"\b(?<date>(?<day>\d{1,2})[.\-/](?<mon>\d{1,2})(?:[.\-/](?<year>\d{2,4}))?)\b" +
            @"|(?<word>\bсегодня\b|\bзавтра\b|\bпослезавтра\b|\bпослепослезавтра\b)" +
            @"|(?<dow>\bпонедельник\b|\bвторник\b|\bсред[ау]\b|\bчетверг\b|\bпятниц[ау]\b|\bсуббот[ау]\b|\bвоскресенье\b|\bпн\b|\bвт\b|\bср\b|\bчт\b|\bпт\b|\bсб\b|\bвс\b)",
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

            // --- 3) Чистим summary: вырезаем найденные куски + «в/на» рядом со временем/датой
            var spans = new List<(int start, int length)>();

            if (dateMatch != null)
            {
                int dStart = dateMatch.Index;
                int dLen = dateMatch.Length;

                // прихватываем предлог перед датой: "в понедельник", "на субботу", "во вторник"
                var preDate = Regex.Match(message[..dStart], @"(?<=^|\s)(в|во|на|к)\s*$",
                    RegexOptions.IgnoreCase);
                if (preDate.Success)
                {
                    dStart = preDate.Index;
                    dLen += dateMatch.Index - preDate.Index;
                }

                // прихватываем разделители сразу после даты: "понедельник -", "понедельник, "
                var afterDate = Regex.Match(message[(dStart + dLen)..], @"^\s*[-,:]\s*");
                if (afterDate.Success)
                    dLen += afterDate.Length;

                spans.Add((dStart, dLen));
            }

            if (timeMatch != null)
            {
                int tStart = timeMatch.Index;
                int tLen = timeMatch.Length;

                var preTime = Regex.Match(message[..tStart], @"(?<=^|\s)(в|на)\s*$",
                    RegexOptions.IgnoreCase);
                if (preTime.Success)
                {
                    tStart = preTime.Index;
                    tLen += timeMatch.Index - preTime.Index;
                }
                spans.Add((tStart, tLen));
            }

            string cleaned = RemoveSpans(message, spans);

            // прибираем одиночные «0»‑хвосты
            cleaned = Regex.Replace(cleaned, @"(?<=^|\s)0(?=\s|$)", " ", RegexOptions.IgnoreCase);

            // нормализуем пробелы/знаки
            cleaned = Regex.Replace(cleaned, @"[,\s]+", " ").Trim();
            if (!string.IsNullOrEmpty(cleaned))
                cleaned = char.ToUpper(cleaned[0]) + cleaned[1..];

            // 4) Intent
            bool hasDate = dateMatch != null;
            bool hasTime = timeMatch != null;

            // если есть время — всегда Meeting; иначе Task
            var type = hasTime ? MessageType.Meeting : MessageType.Task;

            var startDate = date.Date + time; // для Task это будет просто дата + 00:00

            var result = new CalendarEntryParseResult
            {
                Type = type,
                Task = type == MessageType.Task
                    ? new CalendarTask(cleaned, startDate, false)
                    : null,
                Meeting = type == MessageType.Meeting
                    ? new CalendarMeeting(cleaned, startDate, startDate.AddHours(1))
                    : null
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
                "понедельник" or "пн" => DayOfWeek.Monday,
                "вторник" or "вт" => DayOfWeek.Tuesday,
                "среда" or "среду" or "ср" => DayOfWeek.Wednesday,
                "четверг" or "чт" => DayOfWeek.Thursday,
                "пятница" or "пятницу" or "пт" => DayOfWeek.Friday,
                "суббота" or "субботу" or "сб" => DayOfWeek.Saturday,
                "воскресенье" or "вс" => DayOfWeek.Sunday,
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
