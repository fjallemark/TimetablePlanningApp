namespace Tellurian.Trains.Planning.App.Contracts;

#nullable disable
public class StationTrainOrder
{
    public string Name { get; init; }
    public string Designation { get; init; }
    public List<StationTrain> Trains { get; init; }
    public override string ToString() => Name;
}

public class StationTrain
{
    public int CallId { get; init; }
    public double SortTime { get; init; }
    public string ProductResourcName { get; init; }
    public string OperatorName { get; init; }
    public string TrainPrefix { get; init; }
    public int TrainNumber { get; init; }
    public string TrackNumber { get; init; }
    public bool IsArrival { get; init; }
    public string ArrivalTime { get; init; }
    public bool HideArrival { get; init; }
    public bool IsDeparture { get; init; }
    public string DepartureTime { get; init; }
    public bool HideDeparture { get; init; }
    public string OriginName { get; init; }
    public string DestinationName { get; init; }
    public byte OperatingDayFlag { get; init; }
    public bool IsStop { get; init; }
    public bool ShowOperatorName { get; init; }
    public string Time => $"{ArrivalTime}{DepartureTime}";
    public List<string> Notes { get; init; } = [];
    public override string ToString() => $"{OperatorName} {TrainPrefix} {TrainNumber} {ArrivalTime} {DepartureTime}".Trim();
    public TrainInfo ToTrainInfo() => new ()
    {
        OperationDaysFlags = OperatingDayFlag,
    };
}

public static class StationTrainExtensions
{

}


