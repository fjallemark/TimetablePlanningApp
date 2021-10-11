using System;
using System.Collections.Generic;
using System.Linq;

namespace Tellurian.Trains.Planning.App.Contracts
{
    public class TimetableStretch
    {
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public IList<TimetableStretchStation> Stations { get; set; } = Array.Empty<TimetableStretchStation>();
        public IList<TimetableTrainSection> TrainSections { get; set; } = Array.Empty<TimetableTrainSection>();
        public int? StartHour { get; set; }
        public int? EndHour { get; set; }
        public override string ToString() => $"{Number} {Name}";
    }

    public static class TimetableStretchExtensions
    {
        public static int FirstHour(this TimetableStretch me) =>
            me is null ? 0 :
            me.StartHour ?? me.TrainSections.Min(ts => ts.StartTime).Hour();

        public static int LastHour(this TimetableStretch me)
        {
            if (me is null) return 0;
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
        public int FromStationId { get; set; }
        public int FromTrackId { get; set; }
        public int ToStationId { get; set; }
        public int ToTrackId { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }

        public int TrainNumber { get; set; }
        public OperationDays OperationDays { get; set; } = new OperationDays();
        public bool IsCargo { get; set; }
        public bool IsPassenger { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
