using System.Diagnostics;

namespace Tellurian.Trains.Planning.App.Contracts;

public class DriverDutyPart
{
    public DriverDutyPart(Train train) : this(train, null, 0, train.Calls.Count - 1) { }
    public DriverDutyPart(Train train, int fromCallId, int toCallId) : this(train, null, fromCallId, toCallId) { }
    public DriverDutyPart(Train train, TrainLoco? loco) : this(train, loco, 0, train.Calls.Count - 1) { }

    public DriverDutyPart(Train train, TrainLoco? loco, int fromCallId, int toCallId)
    {
        Train = train;
        FromCallId = fromCallId;
        ToCallId = toCallId;
        if (loco != null) { Locos = [loco]; }
    }
    public Train Train { get; set; }
    public ICollection<TrainLoco> Locos { get; set; } = Array.Empty<TrainLoco>();
    public bool IsLastPart { get; set; }
    public int FromCallId { get; set; }
    public int ToCallId { get; set; }
    public bool PutLocoAtParking { get; set; }
    public bool GetLocoAtParking { get; set; }
    public bool ReverseLoco { get; set; }
    public bool TurnLoco { get; set; }
    public bool IsReinforcement { get; set; }
    public int FromCallIndex { get; set; } = -1;
    public int ToCallIndex { get; set; } = -1;
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public DriverDutyPart() { } // For deserlialization only.
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}

public static class DutyPartExtensions
{
    public static StationInfo StartStation(this DriverDutyPart me) => me.Train.Calls[me.FromCallIndex()].Station;
    public static StationInfo EndStation(this DriverDutyPart me) => me.Train.Calls[me.ToCallIndex()].Station;
    public static string? StartTime(this DriverDutyPart me) => me.Train.Calls[me.FromCallIndex()].Arrival?.Time ?? me.Train.Calls[me.FromCallIndex()].Departure?.Time;
    public static string? EndTime(this DriverDutyPart me) => me.Train.Calls[me.ToCallIndex()].Departure?.Time ?? me.Train.Calls[me.ToCallIndex()].Arrival?.Time;
    public static bool IsFirstDutyCall(this DriverDutyPart me, StationCall call) => me.FromCallId == call.Id;
    public static bool IsLastDutyCall(this DriverDutyPart me, StationCall call) => me.ToCallId == call.Id;

    internal static int FromCallIndex(this DriverDutyPart me)
    {
        try
        {
            if (me.FromCallIndex == -1) me.FromCallIndex = me.FromCallId == 0 ? 0 : me.Train.Calls.IndexOf(me.Train.Calls.Single(c => c.Id == me.FromCallId));
            return me.FromCallIndex;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}: Call id = {me.FromCallId}");
            Debugger.Break();
            throw;
        }
    }

    internal static int ToCallIndex(this DriverDutyPart me)
    {
        try
        {
            if (me.ToCallIndex == -1) me.ToCallIndex = me.ToCallId == 0 ? me.Train.Calls.Count - 1 : me.Train.Calls.IndexOf(me.Train.Calls.Single(c => c.Id == me.ToCallId));
            return me.ToCallIndex;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Debugger.Break();
            throw;
        }
    }

    public static IEnumerable<DutyStationCall> Calls(this DriverDutyPart me) => me.Train.Calls.Select((c, i) =>
    new DutyStationCall
    {
        Id = c.Id,
        IsArrivalInDuty = i > me.FromCallIndex() && i <= me.ToCallIndex(),
        IsDepartureInDuty = i >= me.FromCallIndex() && i < me.ToCallIndex(),
        Arrival = c.Arrival,
        Departure = c.Departure,
        IsStop = c.IsStop,
        SequenceNumber = c.SequenceNumber,
        Station = c.Station,
        TrackNumber = c.TrackNumber,
        IsLast = (i == me.Train.Calls.Count - 1)
    }).ToArray();



    public static int NumberOfCalls(this DriverDutyPart me) => 5 +
        me.CallsInDutyPart().Count();

    public static IEnumerable<StationCall> CallsInDutyPart(this DriverDutyPart me)
    {
        return me.Train.Calls
            .Where((c, i) => i >= me.FromCallIndex() && i <= me.ToCallIndex());
    }

    public static double Height(this DriverDutyPart dutyPart)
    {
        var calls = dutyPart.Train.Calls; // dutyPart.CallsInDutyPart();
        return 7 + calls.Sum(c => c.CallTimes().Length * 1.5 + c.ArrivalAndDepartureNotesCount() * 1.3 + dutyPart.Train.Instruction.Length / 60 );
    }
}

public class DutyStationCall : StationCall
{
    public bool IsLast { get; set; }
    public bool IsArrivalInDuty { get; set; }
    public bool IsDepartureInDuty { get; set; }
    public string ArrivalCssClass => IsArrivalInDuty || Arrival?.Notes.Any() == true ? "train call arrival" : "train call notpart";
    public string DepartureCssClass => IsDepartureInDuty ? "train call part" : "train call notpart";
    public bool ShowArrival => (Arrival?.IsHidden == false || Arrival?.Notes.Any() == true) && (IsStop || IsLast);
    public bool ShowDeparture => Departure?.IsHidden == false;
}
