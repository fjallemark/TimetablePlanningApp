using System.Globalization;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Contracts.Extensions;

namespace Tellurian.Trains.Planning.App.Client.Extensions;

public static class GraphicScheduleExtensions
{
    public static readonly GraphicScheduleOptions Options = new();

    public static string Heading(this TimetableStretch me, byte onlyDays = OperationDays.AllDays)
    {
        var days = onlyDays == OperationDays.AllDays ? string.Empty : onlyDays.OperationDays().ShortName;
        return $"{me.Number} {me.Name} : {me.Stations[0].Station.Name} - {me.Stations.Last().Station.Name} {days}";
    }

    #region Time


    public static IEnumerable<(int XHour, string Text)> Hours(this TimetableStretch me, int dayPart =0)
    {
        var first = dayPart==2 && me.BreakHour.HasValue ? me.BreakHour.Value : me.FirstHour();
        var last = dayPart == 1 && me.BreakHour.HasValue? me.BreakHour.Value : me.LastHour();
        return Enumerable.Range(first, last - first + 1).Select(i => (me.XHour(i, dayPart), i.ToString("00", CultureInfo.InvariantCulture)));
    }

    public static int XHourText(this (int h, string) me) => me.h - 12;

    #endregion

    #region Y-axis

    public enum Yaxis
    {
        Top,
        Bottom,
        Hour
    }
    public static int YCanvas(this TimetableStretch me) =>
        me.YStationsBottom() + 20;

    public static int YHour(this TimetableStretch me) => me is null ? 0 : Options.Yoffset;
    public static int YStationsTop(this TimetableStretch me) => me is null ? 0 : Options.Yoffset + Options.HourHeight;
    public static int YStationsBottom(this TimetableStretch me) =>
        (me?.YStation(me.Stations.Last(), me.Stations.Last().Tracks().OrderBy(t => t.DisplayOrder).Last())) ?? 0;

    public static int YTrack(this TimetableStretch me, TimetableStretchStation station, StationTrack track) =>
        me is null ? 0 : me.YStation(station, track);

    public static int YStation(this TimetableStretch me, TimetableStretchStation station, StationTrack? track = null)
    {
        if (me is null) return 0;
        var index = me.Stations.IndexOf(station);
        double y = me.YStationsTop();
        for (var i = index; i > 0; i--)
        {
            y += i > 0 ? me.Stations[i - 1].XHeight() : 0;
            y += Math.Max(me.Stations[i].DistanceFromPrevious * Options.DistanceFactor, Options.MinDistanceBeweenStations);
        }
        if (track != null)
        {
            y += (track.DisplayOrder - 1) * Options.TrackHeight;
        }
        return (int)y;
    }

    public static int YStationName(this TimetableStretch me, TimetableStretchStation station) =>
        me is null ? 0 : me.YStation(station) + station.XHeight() / 2;

    public static int YTrackNumber(this TimetableStretch me, TimetableStretchStation station, StationTrack track) =>
        me.YTrack(station, track) + Options.TrackHeight / 2 - 2;

    #endregion

    #region X-axis

    public static int XCanvas(this TimetableStretch me, int dayPart) => me.XLastHour(dayPart) + 20;
    public static int XStation(this TimetableStretch me) => me is null ? 0 : 1;
    public static int XTrackNumber(this TimetableStretch me) => me is null ? 0 : me.XFirstHour() - 16;
    public static int XFirstHour(this TimetableStretch me, int dayPart=0) => (me?.XHour(me.FirstHour(dayPart), dayPart)) ?? 0;
    public static int XLastHour(this TimetableStretch me, int dayPart) => (me?.XHour(me.LastHour(dayPart), dayPart)) ?? 0;
    public static int XHour(this TimetableStretch me, int hour, int dayPart) => 
        me is null ? 0 : Options.FirstHourOffset + (hour - me.FirstHour(dayPart)) * Options.HourWidth;

    public static int XHeight(this TimetableStretchStation me) =>
        Options.OnlyScheduledTracks ?
        (me.Station.Tracks.Count(t => t.IsScheduledTrack) - 1) * Options.TrackHeight :
        (me.Station.Tracks.Count - 1) * Options.TrackHeight;

    #endregion

    #region Station Track

    public static string Style(this StationTrack me) =>
        me.HasPlatform ? "stroke: #404040; stroke-width:1px" :
        me.IsScheduledTrack ? "stroke: #bfbfbf; stroke-width:1px" : "stroke: #ffb3b3; stroke-width:1px;";

    public static IEnumerable<StationTrack> Tracks(this TimetableStretchStation me) =>
        Options.OnlyScheduledTracks ?
        me.Station.Tracks.Where(t => t.IsScheduledTrack).OrderBy(t => t.DisplayOrder).Select(Track) :
        me.Station.Tracks;

    public static (TimetableStretchStation station, StationTrack track) StationAndTrack(this TimetableStretch me, int stationId, int trackId)
    {
        var station = me.Stations.Single(ts => ts.Station.Id == stationId);
        var track = station.Tracks().Single(t => t.Id == trackId);
        return (station, track);
    }

    private static StationTrack Track(this StationTrack me, int index)
    {
        me.DisplayOrder = index + 1;
        return me;
    }

    #endregion

    #region Train Section 

    public static int XStartTime(this TimetableStretch me, TimetableTrainSection section) =>
        me is null ? 0 : (int)(Options.FirstHourOffset + (section.StartTime / 60 - me.FirstHour()) * Options.HourWidth);

    public static int XEndTime(this TimetableStretch me, TimetableTrainSection section) =>
        me is null ? 0 : (int)(Options.FirstHourOffset + (section.EndTime / 60 - me.FirstHour()) * Options.HourWidth);

    public static int YStartTime(this TimetableStretch me, TimetableTrainSection section)
    {
        var (station, track) = me.StationAndTrack(section.FromStationId, section.FromTrackId);
        return me.YTrack(station, track);
    }

    public static int YEndTime(this TimetableStretch me, TimetableTrainSection section)
    {
        var (station, track) = me.StationAndTrack(section.ToStationId, section.ToTrackId);
        return me.YTrack(station, track);
    }

    public static bool IsBetweenStations(this TimetableTrainSection me) =>
        me.FromStationId != me.ToStationId;

    public static string TrainLabel(this TimetableTrainSection me, byte onlyDays = OperationDays.AllDays, bool showOperatorSignature = false) =>
        me.OperationDays.IsDaily ? $"{me.TrainIdentity(showOperatorSignature)}" :
        onlyDays.IsAllOtherDays(me.OperationDays.Flags) ? $"{me.TrainIdentity(showOperatorSignature)}" :
        $"{me.TrainIdentity(showOperatorSignature)}\n{me.OperationDays.ShortName}";

    private static string TrainIdentity(this TimetableTrainSection me, bool withOperatorSignature = false) =>
        withOperatorSignature ? $"{me.OperatorSignature} {me.TrainNumber}" :
        $"{me.TrainNumber}";

    public static string CssClass(this TimetableTrainSection me) =>
        $"{me.Stroke()}; {StrokeWidth}";

    private static string StrokeWidth => $"stroke-width: {Options.TrainGraphThicknesPx}px";
    private static string Stroke(this TimetableTrainSection me) =>
        me.Color.HasValue() ? $"stroke: {me.Color}; " :
        me.IsCargo && me.IsPassenger ? $"stroke: #cc00cc; " :
        me.IsCargo ? $"stroke: #0066cc; " :
        me.IsPassenger ? $"stroke: #cc3300; " :
        $"stroke: #006600; ";


    public static string PathId(this TimetableStretch me, TimetableTrainSection section) => $"{me.Number}-{section.TrainNumber}-{section.FromTrackId}-{section.ToTrackId}";

    #endregion

    #region Font Sizes
    public static int TrainNumberSize(this string? value, int minSize = 9, int maxSize = 16) =>
        string.IsNullOrWhiteSpace(value) ? minSize :
        value.Length <= 4 ? maxSize :
        Math.Max(maxSize + 4 - value.Length, minSize);

    #endregion
}

public class GraphicScheduleOptions
{
    public int Yoffset { get; set; } = 20;
    public int TrackHeight { get; set; } = 12;
    public int MinDistanceBeweenStations { get; set; } = 52;
    public int FirstHourOffset { get; set; } = 60;
    public int HourWidth { get; set; } = 240;
    public int DistanceFactor { get; set; } = 8;
    public bool OnlyScheduledTracks { get; set; } = true;
    public int HourHeight { get; set; } = 20;
    public int StaionNameOffset { get; set; }
    public int TrainGraphThicknesPx { get; set; } = 2;
}
