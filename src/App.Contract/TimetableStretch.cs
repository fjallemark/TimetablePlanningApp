using System;
using System.Collections.Generic;

#pragma warning disable CA2227 // Collection properties should be read only

namespace Tellurian.Trains.Planning.App.Contract
{
    public class TimetableStretch
    {
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public IList<TimetableStretchStation> Stations { get; set; } = Array.Empty<TimetableStretchStation>();
        public IList<TimetableTrainSection> TrainSections { get; set; } = Array.Empty<TimetableTrainSection>();
        public override string ToString() => $"{Number} {Name}";
    }

    public class TimetableStretchStation
    {
        public int DisplayOrder { get; set; }
        public double DistanceFromPrevious { get; set; }
        public Station Station { get; set; } = new Station();
        public override bool Equals(object obj) => obj is TimetableStretchStation other && other.Station.Equals(Station);
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
        public OperationDays OperationDays { get; set; }
        public bool IsCargo { get; set; }
        public bool IsPassenger { get; set; }
    }
}
