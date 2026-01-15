namespace Tellurian.Trains.Planning.App.Contracts;

public class TrainDeparture
{
    public string StationName { get; set; } = string.Empty;
    public string TrackNumber { get; set; } = string.Empty;
    public TrainLoco Loco { get; set; } = new TrainLoco();
    public TrainInfo Train { get; set; } = new TrainInfo();
    public CallTime DepartureTime { get; set; } = new CallTime();
    public string CountryCode { get; set; } = string.Empty;


    public override int GetHashCode() => HashCode.Combine(StationName, TrackNumber, Train);
    public override bool Equals(object? obj) =>
        obj is TrainDeparture td && td.StationName.Equals(StationName) && td.TrackNumber.Equals(TrackNumber) && td.Train.Equals(Train);
}
