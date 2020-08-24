using System;
using System.Globalization;
using System.Linq;
using System.Text;

#pragma warning disable CA1308 // Normalize strings to uppercase

namespace Tellurian.Trains.Planning.App.Contract
{
    public class OperationDays
    {
        public string FullName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;

        public override bool Equals(object obj) => obj is OperationDays other && other.FullName == FullName;
        public override int GetHashCode() => FullName.GetHashCode(StringComparison.OrdinalIgnoreCase);
        public override string ToString() => ShortName;
    }

    public static class OperationDaysExtensions
    {
        private static readonly Day[] Days = new[] {
            new Day(0, 0x7F, "Daily"),
            new Day(1, 0x01, "Monday"),
            new Day(2, 0x02, "Tuesday"),
            new Day(3, 0x04, "Wednesday"),
            new Day(4, 0x08, "Thursday"),
            new Day(5, 0x10, "Friday"),
            new Day(6, 0x20, "Saturday"),
            new Day(7, 0x40, "Sunday"),
            new Day(0, 0x80, "OnDemand") };

        private static Day[] GetDays(this byte flags) =>
            flags == Days[0].Flag ? new Day[] { Days[0] } :
            flags == Days[8].Flag ? new Day[] { Days[8] } :
            Days.Where(d => d.Number > 0 && (d.Flag & flags) > 0).ToArray();

        public static int DisplayOrder(this byte flags) => ~flags;

        public static OperationDays OperationDays(this byte flags)
        {
            var days = GetDays(flags);
            if (days.Length == 1) return new OperationDays { FullName = days[0].FullName, ShortName = days[0].ShortName };
            var fullName = new StringBuilder(20);
            var shortName = new StringBuilder(10);
            var dayNumber = 0;
            var lastDayNumber = days.Last().Number;
            if (days.IsConsectutive())
            {
                Append(days[0], fullName, shortName);
                Append(Resources.Notes.To, "-", fullName, shortName);
                Append(days.Last(), fullName, shortName, true);
            }
            else
            {
                foreach (var day in days)
                {
                    if (day.Number == lastDayNumber)
                    {
                        Append(Resources.Notes.And, ",", fullName, shortName);
                    }
                    else if (dayNumber > 0)
                    {
                        Append(",", ",", fullName, shortName);
                    }
                    Append(day, fullName, shortName, day.Number > days[0].Number);
                    dayNumber = day.Number;
                }
            }
            return new OperationDays { FullName = fullName.ToString(), ShortName = shortName.ToString() };
        }

        private static void Append(Day day, StringBuilder fullNames, StringBuilder shortNames, bool toLower = false)
        {
            _ = fullNames.Append(toLower && Resources.Notes.DayNameCasing.Equals("LOWER", System.StringComparison.OrdinalIgnoreCase) ? day.FullName.ToLowerInvariant() : day.FullName);
            shortNames.Append(day.ShortName);
        }
        public static void Append(this string fullText, string shortText, StringBuilder fullNames, StringBuilder shortNames)
        {
            if (fullText.Length > 1) fullNames.Append(' ');
            fullNames.Append(fullText);
            fullNames.Append(' ');
            shortNames.Append(shortText);
        }
    }

    internal class Day
    {
        public Day(byte number, byte flag, string resourceKey)
        {
            Number = number;
            Flag = flag;
            FullNameResourceKey = resourceKey;
            ShortNameResourceKey = resourceKey + "Short";
        }
        public byte Flag { get; }
        public byte Number { get; }
        private string FullNameResourceKey { get; }
        private string ShortNameResourceKey { get; }
        public string FullName => Resources.Notes.ResourceManager.GetString(FullNameResourceKey, CultureInfo.CurrentCulture);
        public string ShortName => Resources.Notes.ResourceManager.GetString(ShortNameResourceKey, CultureInfo.CurrentCulture);
    }

    internal static class DayExtensions
    {
        public static bool IsConsectutive(this Day[] days) => days.Length == days.Last().Number - days[0].Number + 1;
    }
}
