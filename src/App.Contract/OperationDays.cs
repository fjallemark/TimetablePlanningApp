using System.Globalization;
using System.Text;

namespace Tellurian.Trains.Planning.App.Contracts;

public class OperationDays
{
    public string FullName { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public bool IsDaily { get; set; }
    public bool IsSingleDay { get; set; }
    public byte Flags { get; set; }

    public override bool Equals(object? obj) => obj is OperationDays other && other.ShortName.Equals(ShortName, StringComparison.OrdinalIgnoreCase);
    public override int GetHashCode() => ShortName.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => ShortName;

    public const byte AllDays = 0x7F;
    public const byte OnDemand = 0x80;
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

    public static byte And(this byte flags, byte and) => flags == Contracts.OperationDays.OnDemand ? flags : (byte)(flags & and);

    public static bool IsAnyOtherDays(this byte it, byte other) => And(it, other) > 0;
    public static bool IsAllOtherDays(this byte it, byte other) => And(it, other) == it;
    public static bool IsAllDays(this byte it) => it == Contracts.OperationDays.AllDays || it.IsOnDemand();
    public static bool IsOnDemand(this byte it) => it == Contracts.OperationDays.OnDemand;

    public static byte AsFlags(this string? value) => (byte)(string.IsNullOrWhiteSpace(value) ? 0 : GetFlagsFromDigits(value));

    private static byte GetFlagsFromDigits(this string value)
    {
        var x = value.Where(c => char.IsDigit(c))
            .Select(c => c switch
            {
                '1' => 0b00000001,
                '2' => 0b00000010,
                '3' => 0b00000100,
                '4' => 0b00001000,
                '5' => 0b00010000,
                '6' => 0b00100000,
                '7' => 0b01000000,
                _ => 0
            });
        return (byte)x.Sum() ;
    }

    public static OperationDays OperationDays(this byte flags)
    {
        var days = GetDays(flags);
        var isDaily = flags.IsAllDays();
        var fullName = new StringBuilder(20);
        var shortName = new StringBuilder(10);
        if (days.Length == 0)
        {
            return new OperationDays() { Flags = 0 };
        }
        if (days.Length == 1)
        {
            fullName.Append(days[0].FullName);
            shortName.Append(days[0].ShortName);
        }
        else
        {
            var dayNumber = 0;
            var lastDayNumber = days.Last().Number;
            if (days.IsConsecutiveFromMonday())
            {
                Append(days[0], fullName, shortName);
                Append(Resources.Notes.To.ToLowerInvariant(), "-", fullName, shortName);
                Append(days.Last(), fullName, shortName, true);
            }
            else if (days.IsConsecutiveFromSunday())
            {
                Append(days[^1], fullName, shortName);
                Append(Resources.Notes.To.ToLowerInvariant(), "-", fullName, shortName);
                Append(days[^2], fullName, shortName, true);

            }
            else if (flags == 0x5F)
            {
                Append(Days[1], fullName, shortName);
                Append(Resources.Notes.To.ToLowerInvariant(), "-", fullName, shortName);
                Append(Days[5], fullName, shortName, true);
                Append(Resources.Notes.And.ToLowerInvariant(), ",", fullName, shortName);
                Append(Days[7], fullName, shortName, true);
            }
            else if (flags == 0x4F)
            {
                Append(Days[7], fullName, shortName);
                Append(Resources.Notes.To.ToLowerInvariant(), "-", fullName, shortName);
                Append(Days[4], fullName, shortName, true);
            }
            else
            {
                foreach (var day in days)
                {
                    if (day.Number == lastDayNumber)
                    {
                        Append(Resources.Notes.And.ToLowerInvariant(), ",", fullName, shortName);
                    }
                    else if (dayNumber > 0)
                    {
                        Append(",", ",", fullName, shortName);
                    }
                    Append(day, fullName, shortName, day.Number > days[0].Number);
                    dayNumber = day.Number;
                }
            }
        }
        return new OperationDays
        {
            IsDaily = isDaily,
            IsSingleDay = days.Length == 1,
            FullName = fullName.ToString(),
            ShortName = shortName.ToString(),
            Flags = flags
        };
    }

    public static byte[] UniqueDayCombinations(this byte[] flags)
    {
        if (flags.Length <= 1) return flags;
        ReadOnlySpan<byte> sortedFlags = flags.OrderBy(f => f).ToArray();
        var days = new byte[sortedFlags.Length];
#pragma warning disable CS0219 // Variable is assigned but its value is never used
        var di = 0;
#pragma warning restore CS0219 // Variable is assigned but its value is never used

        for (var b = 1; b < 8; b++)
        {

        }


        //for (var b = 1; b < 8; b++)
        //{
        //    for (var i = 0; i < sortedFlags.Length; i++)
        //    {
        //        if (sortedFlags[i].SetBitsCount() == b)
        //        {
        //            byte f = sortedFlags[i];
        //            for (var k = 0; k < d; k++)
        //            {
        //                f = (byte)(f & ~days[k]);
        //            }
        //            if (f > 0) days[d++] = f;

        //        }
        //    }

        //}
        return days.Where(f => f > 0).OrderBy(f => f).ToArray();
    }

    public static int OneBitsCount(this byte it)
    {
        ReadOnlySpan<byte> nibbleLookup = new byte[] { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4 };
        return nibbleLookup[it & 0x0F] + nibbleLookup[it >> 4];
    }

    public static bool IsBitSet(this byte it, int bit) => bit >= 1 && bit <= 8 && (it & Bit(bit)) > 0;
    public static byte Bit(int bit) => (byte)(Math.Pow(2, bit - 1));
    public static byte ZeroBit(this byte it, int bit) => (byte)(it & (~(byte)(1 << (bit - 1))));

    private static void Append(Day day, StringBuilder fullNames, StringBuilder shortNames, bool toLower = false)
    {
        _ = fullNames.Append(toLower && Resources.Notes.DayNameCasing.Equals("LOWER", StringComparison.OrdinalIgnoreCase) ? day.FullName.ToLowerInvariant() : day.FullName);
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
    public string FullName => Resources.Notes.ResourceManager.GetString(FullNameResourceKey, CultureInfo.CurrentCulture) ?? FullNameResourceKey;
    public string ShortName => Resources.Notes.ResourceManager.GetString(ShortNameResourceKey, CultureInfo.CurrentCulture) ?? ShortNameResourceKey;
}

internal static class DayExtensions
{
    public static bool IsConsecutiveFromMonday(this Day[] days) => 
        days.Length == days.Last().Number - days[0].Number + 1;

    public static bool IsConsecutiveFromSunday(this Day[] days) =>
        days.Length >= 3 && days.Last().Number == 7 && days[0..^2].IsConsecutiveFromMonday();
}
