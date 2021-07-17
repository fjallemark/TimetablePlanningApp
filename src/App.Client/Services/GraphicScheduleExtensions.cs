using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client.Services
{
    public static class GraphicScheduleExtensions
    {
        public static readonly GraphicScheduleOptions Options = new GraphicScheduleOptions();

        public static string Heading(this TimetableStretch me) =>
            $"{me.Number} {me.Name} : {me.Stations[0].Station.Name} - {me.Stations.Last().Station.Name}";

        #region Time


        public static IEnumerable<(int XHour, string Text)> Hours(this TimetableStretch me)
        {
            var first = me.FirstHour();
            var last = me.LastHour();
            return Enumerable.Range(first, last - first + 1).Select(i => (me.XHour(i), i.ToString("00", CultureInfo.InvariantCulture)));
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
            me is null ? 0 : YStation(me, station, track);

        public static int YStation(this TimetableStretch me, TimetableStretchStation station, StationTrack? track = null)
        {
            if (me is null) return 0;
            var index = me.Stations.IndexOf(station);
            double y = me.YStationsTop();
            for (var i = index; i > 0; i--)
            {
                y += (i > 0) ? me.Stations[i - 1].XHeight() : 0;
                y += Math.Max(me.Stations[i].DistanceFromPrevious * Options.DistanceFactor, Options.MinDistanceBeweenStations);
            }
            if (track != null)
            {
                y += (track.DisplayOrder - 1) * Options.TrackHeight;
            }
            return (int)y;
        }

        public static int YStationName(this TimetableStretch me, TimetableStretchStation station) =>
            me is null ? 0 : me.YStation(station) + (station.XHeight() / 2);

        public static int YTrackNumber(this TimetableStretch me, TimetableStretchStation station, StationTrack track) =>
            me.YTrack(station, track) + (Options.TrackHeight / 2) - 2;

        #endregion

        #region X-axis

        public static int XCanvas(this TimetableStretch me) => me.XLastHour() + 20;
        public static int XStation(this TimetableStretch me) => me is null ? 0 : 1;
        public static int XTrackNumber(this TimetableStretch me) => me is null ? 0 : me.XFirstHour() - 10;
        public static int XFirstHour(this TimetableStretch me) => (me?.XHour(me.FirstHour())) ?? 0;
        public static int XLastHour(this TimetableStretch me) => (me?.XHour(me.LastHour())) ?? 0;
        public static int XHour(this TimetableStretch me, int hour) => me is null ? 0 : Options.FirstHourOffset + ((hour - me.FirstHour()) * Options.HourWidth);

        public static int XHeight(this TimetableStretchStation me) =>
            Options.OnlyScheduledTracks ?
            (me.Station.Tracks.Count(t => t.IsScheduledTrack) - 1) * Options.TrackHeight :
            (me.Station.Tracks.Count - 1) * Options.TrackHeight;

        #endregion

        #region Station Track

        public static string Style(this StationTrack me) =>
            me.IsScheduledTrack ? "stroke: gray; stroke-width:1px" : "stroke: lightgray; stroke-width:1px;";

        public static IEnumerable<StationTrack> Tracks(this TimetableStretchStation me) =>
            Options.OnlyScheduledTracks ?
            me.Station.Tracks.Where(t => t.IsScheduledTrack).OrderBy(t => t.DisplayOrder).Select(It) :
            me.Station.Tracks;

        public static (TimetableStretchStation station, StationTrack track) StationAndTrack(this TimetableStretch me, int stationId, int trackId)
        {
            var station = me.Stations.Single(ts => ts.Station.Id == stationId);
            var track = station.Tracks().Single(t => t.Id == trackId);
            return (station, track);
        }

        private static StationTrack It(this StationTrack me, int index)
        {
            me.DisplayOrder = index + 1;
            return me;
        }

        #endregion

        #region Train Section 

        public static int XStartTime(this TimetableStretch me, TimetableTrainSection section) =>
            me is null ? 0 : (int)(Options.FirstHourOffset + (((section.StartTime / 60) - me.FirstHour()) * Options.HourWidth));

        public static int XEndTime(this TimetableStretch me, TimetableTrainSection section) =>
            me is null ? 0 : (int)(Options.FirstHourOffset + (((section.EndTime / 60) - me.FirstHour()) * Options.HourWidth));

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

        public static string TrainLabel(this TimetableTrainSection me) =>
            me.OperationDays.IsDaily ? $"{me.TrainNumber}" : $"{me.TrainNumber} {me.OperationDays.ShortName}";

        public static string CssClass(this TimetableTrainSection me) =>
            me.IsCargo ? "stroke: #0066cc; stroke-width:3px" :
            me.IsPassenger ? "stroke: #cc3300; stroke-width:3px" :
            "stroke: #006600; stroke-width:3px";

        public static string PathId(this TimetableStretch me, TimetableTrainSection section) => $"{me.Number}-{section.TrainNumber}-{section.FromTrackId}-{section.ToTrackId}";

        #endregion
    }

    public class GraphicScheduleOptions
    {
        public int Yoffset { get; set; } = 20;
        public int TrackHeight { get; set; } = 8;
        public int MinDistanceBeweenStations { get; set; } = 60;
        public int FirstHourOffset { get; set; } = 60;
        public int HourWidth { get; set; } = 180;
        public int DistanceFactor { get; set; } = 10;
        public bool OnlyScheduledTracks { get; set; } = true;
        public int HourHeight { get; set; } = 20;
        public int StaionNameOffset { get; set; }
    }
}
