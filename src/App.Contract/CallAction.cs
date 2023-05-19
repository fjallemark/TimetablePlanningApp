namespace Tellurian.Trains.Planning.App.Contracts;

public class CallAction
{
    public static CallAction ArrivalCallAction(StationCall call) => new() { Time = call.Arrival, Station = call.Station, Track = call.TrackNumber, UnassignTime = call.Departure };
    public static CallAction DepartureCallAction(StationCall call) => new() { Time = call.Departure, Station = call.Station, Track = call.TrackNumber, AssignTime = call.Arrival };
    public CallAction() { }
    public StationInfo? Station { get; init; }
    public string Track { get; init; } = string.Empty;
    public CallTime? Time { get; init; }
    public CallTime? AssignTime { get; init; }
    public CallTime? UnassignTime { get; init; }
    public override string ToString() => $"{Station} {Track} {Time}";
}
