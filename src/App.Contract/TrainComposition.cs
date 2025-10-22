using System.Text.Json.Serialization;

namespace Tellurian.Trains.Planning.App.Contracts;

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

    public static TrainComposition MergeTrainsets(this TrainComposition composition)
    {
        var trainsets = new List<TrainsetInTrainComposition>(50);
        var groups = composition.Trainsets.GroupBy(ts => $"{ts.Number}{ts.Class}{ts.MaxNumberOfUnits}{ts.Note}");
        foreach (var group in groups) {
            TrainsetInTrainComposition mergedTrainset = new()
            {
                ArrivalStationName = group.First().ArrivalStationName,
                ArrivalTime = group.First().ArrivalTime,
                DepartureStationName = group.First().DepartureStationName,
                DepartureTime = group.First().DepartureTime,
                Class = group.First().Class,
                OperatorName = group.First().OperatorName,
                FinalStationName = group.First().FinalStationName,
                MaxNumberOfUnits = group.First().MaxNumberOfUnits,
                Note= group.First().Note,
                OrderInTrain = group.First().OrderInTrain,
                Number=group.First().Number,
                OperationDaysFlags = (byte)group.Sum(g => g.OperationDaysFlags),
            };
            trainsets.Add(mergedTrainset);
        }
        TrainComposition merged = new() { 
            OperatorName=composition.OperatorName, 
            TrainCategoryResourceCode=composition.TrainCategoryResourceCode, 
            Prefix=composition.Prefix, 
            TrainNumber=composition.TrainNumber,
            Trainsets = trainsets,
        };
        return merged;
    }
}