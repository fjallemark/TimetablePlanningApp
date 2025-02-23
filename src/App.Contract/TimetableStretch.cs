namespace Tellurian.Trains.Planning.App.Contracts;

public class TimetableStretch
{
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public IList<TimetableStretchStation> Stations { get; set; } = Array.Empty<TimetableStretchStation>();
    public IList<TimetableTrainSection> TrainSections { get; set; } = Array.Empty<TimetableTrainSection>();
    public int? StartHour { get; set; }
    public int? BreakHour { get; set; }
    public int? EndHour { get; set; }
    public bool ShowTrainOperatorSignature { get; set; }
    public override string ToString() => $"{Number} {Name}";
}

public static class TimetableStretchExtensions
{
    public static int FirstHour(this TimetableStretch me, int dayPart = 0) =>
        me is null ? 0 :
        dayPart == 2 && me.BreakHour.HasValue ? me.BreakHour.Value :
        me.StartHour ?? me.TrainSections.Min(ts => ts.StartTime).Hour();

    public static int LastHour(this TimetableStretch me, int dayPart = 0)
    {
        if (me is null) return 0;
        if (dayPart == 1 && me.BreakHour.HasValue) return me.BreakHour.Value;
        if (me.EndHour.HasValue) return me.EndHour.Value;
        var last = me.TrainSections.Max(ts => ts.EndTime) - 0.001;
        return last > last.Hour() ? last.Hour() + 1 : last.Hour();
    }
    private static int Hour(this double me) => (int)(me / 60);
}

public class TimetableStretchStation
{
    public int DisplayOrder { get; set; }
    public double DistanceFromPrevious { get; set; }
    public Station Station { get; set; } = new Station();
    public override bool Equals(object? obj) => obj is TimetableStretchStation other && other.Station.Equals(Station);
    public override int GetHashCode() => Station.GetHashCode();
    public override string ToString() => Station.Signature;
}

public class TimetableTrainSection
{
    public int TrainId { get; set; }
    public int FromStationId { get; set; }
    public int FromTrackId { get; set; }
    public int ToStationId { get; set; }
    public int ToTrackId { get; set; }
    public double StartTime { get; set; }
    public double EndTime { get; set; }

    public string? OperatorSignature { get; set; }
    public int TrainNumber { get; set; }
    public OperationDays OperationDays { get; set; } = new OperationDays();
    public bool IsCargo { get; set; }
    public bool IsPassenger { get; set; }
    public string Color { get; set; } = string.Empty;
    public override string ToString() => $"Train {TrainNumber} {OperationDays.ShortName} {FromStationId}:{StartTime.ToTime()}-{EndTime.ToTime()}:{ToStationId}";

}

public static class TimetableTrainSectionExtensions
{
    public static bool IsSameTrainButDifferentOperatingDays(this TimetableTrainSection me, TimetableTrainSection other) =>
        other.FromStationId == me.FromStationId &&
        other.ToStationId == me.ToStationId &&
        other.TrainNumber == me.TrainNumber &&
        other.OperationDays.Flags != me.OperationDays.Flags;
    public static string ToTime(this double t) => $"{TimeSpan.FromHours(t):HH:mm}";
}
