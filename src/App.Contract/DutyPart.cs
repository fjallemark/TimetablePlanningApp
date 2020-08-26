using System;
using System.Collections.Generic;
using System.Linq;

namespace Tellurian.Trains.Planning.App.Contract
{
    public class DutyPart
    {
        public DutyPart(Train train) : this(train, null, 0, train.Calls.Count - 1) { }
        public DutyPart(Train train, int fromCallId, int toCallId) : this(train, null, fromCallId, toCallId) { }
        public DutyPart(Train train, Loco? loco) : this(train, loco, 0, train.Calls.Count - 1) { }

        public DutyPart(Train train, Loco? loco, int fromCallId, int toCallId)
        {
            Train = train;
            FromCallId = fromCallId;
            ToCallId = toCallId;
            if (loco != null) { Locos = new List<Loco> { loco }; }
        }
        public Train Train { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Required for deserialization.")]
        public ICollection<Loco> Locos { get; set; } = Array.Empty<Loco>();
        public bool IsLastPart { get; set; }
        public int FromCallId { get; set; }
        public int ToCallId { get; set; }
        public bool PutLocoAtParking { get; set; }
        public bool GetLocoAtParking { get; set; }
        public bool ReverseLoco { get; set; }
        public bool TurnLoco { get; set; }
        public int FromCallIndex { get; set; } = -1;
        public int ToCallIndex { get; set; } = -1;
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public DutyPart() { } // For deserlialization only.
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }

    public static class DutyPartExtensions
    {
        public static Station StartStation(this DutyPart me) => me.Train.Calls[me.FromCallIndex()].Station;
        public static Station EndStation(this DutyPart me) => me.Train.Calls[me.ToCallIndex()].Station;
        public static string? StartTime(this DutyPart me) => me.Train.Calls[me.FromCallIndex()].Arrival?.Time ?? me.Train.Calls[me.FromCallIndex()].Departure?.Time;
        public static string? EndTime(this DutyPart me) => me.Train.Calls[me.ToCallIndex()].Departure?.Time ?? me.Train.Calls[me.ToCallIndex()].Arrival?.Time;
        public static bool IsFirstDutyCall(this DutyPart me, StationCall call) => me.FromCallId == call.Id;
        public static bool IsLastDutyCall(this DutyPart me, StationCall call) => me.ToCallId == call.Id;

        internal static int FromCallIndex(this DutyPart me)
        {
            if (me.FromCallIndex == -1) me.FromCallIndex = me.FromCallId == 0 ? 0 : me.Train.Calls.IndexOf(me.Train.Calls.Single(c => c.Id == me.FromCallId));
            return me.FromCallIndex;
        }
        internal static int ToCallIndex(this DutyPart me)
        {
            if (me.ToCallIndex == -1) me.ToCallIndex = me.ToCallId == 0 ? me.Train.Calls.Count - 1 : me.Train.Calls.IndexOf(me.Train.Calls.Single(c => c.Id == me.ToCallId));
            return me.ToCallIndex;
        }

        public static IEnumerable<DutyStationCall> Calls(this DutyPart me) => me.Train.Calls.Select((c, i) => new DutyStationCall
        {
            Id = c.Id,
            IsArrivalInDuty = i > me.FromCallIndex() && i <= me.ToCallIndex(),
            IsDepartureInDuty = i >= me.FromCallIndex() && i < me.ToCallIndex(),
            Arrival = c.Arrival,
            Departure = c.Departure,
            IsStop = c.IsStop,
            SequenceNumber = c.SequenceNumber,
            Station = c.Station,
            Track = c.Track,
            IsLast = (i == me.Train.Calls.Count - 1)
        }).ToArray();
    }

    public class DutyStationCall : StationCall
    {
        public bool IsLast { get; set; }
        public bool IsArrivalInDuty { get; set; }
        public bool IsDepartureInDuty { get; set; }
        public string ArrivalCssClass => IsArrivalInDuty ? "duty call part" : "duty call notpart";
        public string DepartureCssClass => IsDepartureInDuty ? "duty call part" : "duty call notpart";
        public bool ShowArrival => Arrival?.IsHidden == false && (IsStop || IsLast);
        public bool ShowDeparture => Departure?.IsHidden == false;
    }
}
