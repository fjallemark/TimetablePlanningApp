namespace Tellurian.Trains.Planning.App.Contracts;

public class TrainPart
{
    public string? TrainNumber { get; set; }
    public string? LocoNumber { get; set; }
    public CallAction? FromDeparture { get; set; }
    public CallAction? ToArrival { get; set; }
    public Train? Train { get; set; }
    public override string ToString() => $"{TrainNumber}";
}

