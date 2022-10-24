using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client.Services;

public static class StationTrainOrderExtensions
{
    public static string Destination(this StationTrain? me ) =>
        string.IsNullOrWhiteSpace(me?.DepartureTime) || me.DepartureTime.StartsWith("(") ? string.Empty : me.DestinationName;

    public static string Origin(this StationTrain? me) =>
         string.IsNullOrWhiteSpace(me?.ArrivalTime) || me.ArrivalTime.StartsWith("(") ? string.Empty : me.OriginName;
}
