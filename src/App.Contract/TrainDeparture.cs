namespace Tellurian.Trains.Planning.App.Contracts;

public class TrainDeparture
{
    public string StationName { get; set; } = string.Empty;
    public string TrackNumber { get; set; } = string.Empty;
    public TrainLoco Loco { get; set; } = new TrainLoco();
    public TrainInfo Train { get; set; } = new TrainInfo();
    public CallTime DepartureTime { get; set; } = new CallTime();
    public string CountryCode { get; set; } = string.Empty;
}
