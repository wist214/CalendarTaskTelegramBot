using System.Globalization;
using CalendarEvent.Domain.Entities;
using CalendarEvent.Infrastructure.Calendar;

namespace CalendarEvent.Infrastructure.Tests
{
    [TestFixture]
    public class CalendarEntryParserTests
    {
        private CalendarEntryParser _parser;

        [SetUp]
        public void Setup()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            _parser = new CalendarEntryParser();
        }

        // ===== Helpers =====

        private static DateTimeOffset ToLocalDto(DateTime local)
            => new DateTimeOffset(local, TimeZoneInfo.Local.GetUtcOffset(local));

        private static void AssertDateTimeOffsetLocal(DateTimeOffset actual, DateTime expectedLocal)
        {
            var expected = ToLocalDto(expectedLocal);
            Assert.That(actual.Year, Is.EqualTo(expected.Year), "Year mismatch");
            Assert.That(actual.Month, Is.EqualTo(expected.Month), "Month mismatch");
            Assert.That(actual.Day, Is.EqualTo(expected.Day), "Day mismatch");
            Assert.That(actual.Hour, Is.EqualTo(expected.Hour), "Hour mismatch");
            Assert.That(actual.Minute, Is.EqualTo(expected.Minute), "Minute mismatch");
        }

        private static DateTime NextWeekday(DateTime start, DayOfWeek dow)
        {
            int delta = ((int)dow - (int)start.DayOfWeek + 7) % 7;
            if (delta == 0) delta = 7;
            return start.Date.AddDays(delta);
        }

        // ===== Tests =====

        [Test]
        public async Task Parses_Date_ddMM_Time_HH_MM_Title()
        {
            var res = await _parser.ParseAsync("22.08 16:30 Рыба", CancellationToken.None);

            Assert.NotNull(res);
            Assert.That(res.Type, Is.EqualTo(MessageType.Meeting));
            Assert.NotNull(res.Meeting);

            var expected = new DateTime(DateTime.Today.Year, 8, 22, 16, 30, 0);
            AssertDateTimeOffsetLocal(res.Meeting!.Start, expected);
            Assert.That(res.Meeting.Title, Is.EqualTo("Рыба"));
        }

        [Test]
        public async Task Parses_Date_ddMM_Time_HourOnly_WithSpace_Title()
        {
            var res = await _parser.ParseAsync("22.08 16 Рыба", CancellationToken.None);

            Assert.That(res.Type, Is.EqualTo(MessageType.Meeting));
            var expected = new DateTime(DateTime.Today.Year, 8, 22, 16, 0, 0);
            AssertDateTimeOffsetLocal(res.Meeting!.Start, expected);
            Assert.That(res.Meeting.Title, Is.EqualTo("Рыба"));
        }

        [Test]
        public async Task Parses_Date_ddMM_Time_WithPrep_Na()
        {
            var res = await _parser.ParseAsync("22.08 на 16 Рыба", CancellationToken.None);

            Assert.That(res.Type, Is.EqualTo(MessageType.Meeting));
            var expected = new DateTime(DateTime.Today.Year, 8, 22, 16, 0, 0);
            AssertDateTimeOffsetLocal(res.Meeting!.Start, expected);
            Assert.That(res.Meeting.Title, Is.EqualTo("Рыба"));
        }

        [Test]
        public async Task Parses_TimeWithPrep_And_Poslezavtra()
        {
            var res = await _parser.ParseAsync("на 16 послезавтра Рыба", CancellationToken.None);

            var d = DateTime.Today.AddDays(2);
            var expected = new DateTime(d.Year, d.Month, d.Day, 16, 0, 0);

            Assert.That(res.Type, Is.EqualTo(MessageType.Meeting));
            AssertDateTimeOffsetLocal(res.Meeting!.Start, expected);
            Assert.That(res.Meeting.Title, Is.EqualTo("Рыба"));
        }

        [Test]
        public async Task Parses_BareHour_NotPartOfDate()
        {
            var res = await _parser.ParseAsync("16 Рыба", CancellationToken.None);

            var expected = DateTime.Today.Date.AddHours(16);
            Assert.That(res.Type, Is.EqualTo(MessageType.Meeting));
            AssertDateTimeOffsetLocal(res.Meeting!.Start, expected);
            Assert.That(res.Meeting.Title, Is.EqualTo("Рыба"));
        }

        [Test]
        public async Task DoesNotTreat_DayFromDate_AsTime()
        {
            var res = await _parser.ParseAsync("22.08 Рыба", CancellationToken.None);

            var expected = new DateTime(DateTime.Today.Year, 8, 22, 0, 0, 0);
            Assert.That(res.Type, Is.EqualTo(MessageType.Meeting));
            AssertDateTimeOffsetLocal(res.Meeting!.Start, expected);
            Assert.That(res.Meeting.Title, Is.EqualTo("Рыба"));
        }

        [Test]
        public async Task Supports_DifferentDateSeparators_And_Year()
        {
            var res1 = await _parser.ParseAsync("22/08 16:05 Рыба", CancellationToken.None);
            var res2 = await _parser.ParseAsync("22-08-2026 7 Рыба", CancellationToken.None);
            var res3 = await _parser.ParseAsync("22.08.26 в 9 Рыба", CancellationToken.None);

            AssertDateTimeOffsetLocal(res1.Meeting!.Start, new DateTime(DateTime.Today.Year, 8, 22, 16, 05, 0));
            AssertDateTimeOffsetLocal(res2.Meeting!.Start, new DateTime(2026, 8, 22, 7, 0, 0));
            AssertDateTimeOffsetLocal(res3.Meeting!.Start, new DateTime(2026, 8, 22, 9, 0, 0));

            Assert.That(res1.Meeting.Title, Is.EqualTo("Рыба"));
            Assert.That(res2.Meeting.Title, Is.EqualTo("Рыба"));
            Assert.That(res3.Meeting.Title, Is.EqualTo("Рыба"));
        }

        [Test]
        public async Task Words_Today_Tomorrow_Poslezavtra_Posleposlezavtra()
        {
            var today = await _parser.ParseAsync("сегодня 10 Рыба", CancellationToken.None);
            var tomorrow = await _parser.ParseAsync("завтра 10 Рыба", CancellationToken.None);
            var afterTomorrow = await _parser.ParseAsync("послезавтра 10 Рыба", CancellationToken.None);
            var afterAfterTomorrow = await _parser.ParseAsync("послепослезавтра 10 Рыба", CancellationToken.None);

            AssertDateTimeOffsetLocal(today.Meeting!.Start, DateTime.Today.Date.AddHours(10));

            var d1 = DateTime.Today.AddDays(1);
            AssertDateTimeOffsetLocal(tomorrow.Meeting!.Start, new DateTime(d1.Year, d1.Month, d1.Day, 10, 0, 0));

            var d2 = DateTime.Today.AddDays(2);
            AssertDateTimeOffsetLocal(afterTomorrow.Meeting!.Start, new DateTime(d2.Year, d2.Month, d2.Day, 10, 0, 0));

            var d3 = DateTime.Today.AddDays(3);
            AssertDateTimeOffsetLocal(afterAfterTomorrow.Meeting!.Start, new DateTime(d3.Year, d3.Month, d3.Day, 10, 0, 0));
        }

        [Test]
        public async Task DayOfWeek_Parses_To_Next_Weekday()
        {
            var resMon = await _parser.ParseAsync("понедельник 9 Рыба", CancellationToken.None);
            var resWed = await _parser.ParseAsync("среду в 14 Рыба", CancellationToken.None);
            var resSat = await _parser.ParseAsync("суббота 18 Рыба", CancellationToken.None);

            AssertDateTimeOffsetLocal(resMon.Meeting!.Start, NextWeekday(DateTime.Today, DayOfWeek.Monday).AddHours(9));
            AssertDateTimeOffsetLocal(resWed.Meeting!.Start, NextWeekday(DateTime.Today, DayOfWeek.Wednesday).AddHours(14));
            AssertDateTimeOffsetLocal(resSat.Meeting!.Start, NextWeekday(DateTime.Today, DayOfWeek.Saturday).AddHours(18));

            Assert.That(resMon.Meeting.Title, Is.EqualTo("Рыба"));
            Assert.That(resWed.Meeting.Title, Is.EqualTo("Рыба"));
            Assert.That(resSat.Meeting.Title, Is.EqualTo("Рыба"));
        }

        [Test]
        public async Task Cleans_Title_Removes_Preludes_And_ExtraSpaces()
        {
            var res = await _parser.ParseAsync("22.08 в 16:30   ,    Рыба   ", CancellationToken.None);
            Assert.That(res.Meeting!.Title, Is.EqualTo("Рыба"));
        }

        [Test]
        public async Task When_NoDateNoTime_Type_Is_Task_With_DefaultMidnightToday()
        {
            var res = await _parser.ParseAsync("Принять рыбу", CancellationToken.None);

            Assert.That(res.Type, Is.EqualTo(MessageType.Task));
            Assert.NotNull(res.Task);
            Assert.That(res.Task!.Title, Is.EqualTo("Принять рыбу"));

            var expected = DateTime.Today; // 00:00
            AssertDateTimeOffsetLocal(res.Task.Due, expected);
        }

        [Test]
        public async Task TimeAfterTitle_IsRecognized()
        {
            var res = await _parser.ParseAsync("Рыба 22.08 в 18", CancellationToken.None);

            var expected = new DateTime(DateTime.Today.Year, 8, 22, 18, 0, 0);
            Assert.That(res.Type, Is.EqualTo(MessageType.Meeting));
            AssertDateTimeOffsetLocal(res.Meeting!.Start, expected);
            Assert.That(res.Meeting.Title, Is.EqualTo("Рыба"));
        }

        [Test]
        public async Task Prevents_GarbageZero_In_Title()
        {
            var res = await _parser.ParseAsync("на 16 послезавтра   0   Рыба", CancellationToken.None);
            Assert.That(res.Meeting!.Title, Is.EqualTo("Рыба"));
        }

        [Test]
        public async Task BareHour_NearDate_StillWorks_WhenNotOverlapping()
        {
            var res = await _parser.ParseAsync("22.08 16 Рыба встреча", CancellationToken.None);

            var expected = new DateTime(DateTime.Today.Year, 8, 22, 16, 0, 0);
            AssertDateTimeOffsetLocal(res.Meeting!.Start, expected);
            Assert.That(res.Meeting.Title, Is.EqualTo("Рыба встреча"));
        }

        [Test]
        public async Task TimeWithColon_NotMistaken_For_Date()
        {
            // обратная проверка: «16:30» — это время, а не дата 16.30
            var res = await _parser.ParseAsync("16:30 Рыба", CancellationToken.None);

            var expected = DateTime.Today.Date.AddHours(16).AddMinutes(30);
            Assert.That(res.Type, Is.EqualTo(MessageType.Meeting));
            AssertDateTimeOffsetLocal(res.Meeting!.Start, expected);
            Assert.That(res.Meeting.Title, Is.EqualTo("Рыба"));
        }
    }
}
