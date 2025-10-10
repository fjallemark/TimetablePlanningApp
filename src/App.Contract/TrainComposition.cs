using System.Text.Json.Serialization;

namespace Tellurian.Trains.Planning.App.Contracts;

[JsonSerializable(typeof(TrainComposition))]
public class TrainComposition
{
    public required string OperatorName { get; init; }
    public required string  TrainCategoryResourceCode { get; init; }
    public string Prefix { get; set; } = string.Empty;
    public int TrainNumber { get; init; }
    public byte OperationDaysFlags { get; init; }

    [JsonInclude]
    public List<TrainsetInTrainComposition> Trainsets = [];
}

[JsonSerializable(typeof(List<TrainsetInTrainComposition>))]
public class TrainsetInTrainComposition
{
    public required string OperatorName { get; set; }
    public int Number { get; set; }
    public byte OperationDaysFlags { get; set; }
    public string? Class {  get; set; }
    public int OrderInTrain { get; set; }
    public int MaxNumberOfUnits { get; set; }
    public required string DepartureStationName { get; set; } 
    public TimeSpan DepartureTime { get; set; }
    public required string ArrivalStationName { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public string? Note {  get; set; }
    public string? FinalStationName { get; set; }
}

public static class TrainCompositionExtensions
{
    public static string StartStationName(this TrainComposition train) =>
         train.Trainsets.Count > 0 ? train.Trainsets.OrderBy(ts => ts.DepartureTime).First().DepartureStationName : string.Empty;
}