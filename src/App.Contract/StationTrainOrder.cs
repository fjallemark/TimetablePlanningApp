using System.Collections.Generic;

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
    public double SortTime { get; init; }
    public string ProductName { get; init; }
    public string OperatorName { get; init; }
    public string TrainNumber { get; init; }
    public string TrackNumber { get; init; }
    public string ArrivalTime { get; init; }
    public string DepartureTime { get; init; }
    public string OriginName { get; init; }
    public string DestinationName { get; init; }
    public byte OperatingDayFlag { get; init; }
    public bool IsStop { get; init; }
    public override string ToString() => $"{TrainNumber} {ArrivalTime} {DepartureTime}";
}


