using System.Diagnostics.CodeAnalysis;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client.Services;

public static class StationTrainOrderExtensions
{
    public static string Destination(this StationTrain? me) =>
        me.IsNotDeparture() ? string.Empty : me.DestinationName;

    public static string Origin(this StationTrain? me) =>
         me.IsNotArrival() ? string.Empty : me.OriginName;

    public static string OriginCssClass(this StationTrain? me) =>
        me.IsNotArrival() ? string.Empty : "bold";
    public static string DestinationCssClass(this StationTrain? me) =>
        me.IsNotDeparture() ? string.Empty : "bold";

    public static bool IsNotDeparture([NotNullWhen(false)] this StationTrain? me) =>
        string.IsNullOrWhiteSpace(me?.DepartureTime) || me.DepartureTime.StartsWith("(");

    public static bool IsNotArrival([NotNullWhen(false)] this StationTrain? me) =>
        string.IsNullOrWhiteSpace(me?.ArrivalTime) || me.ArrivalTime.StartsWith("(");
}
