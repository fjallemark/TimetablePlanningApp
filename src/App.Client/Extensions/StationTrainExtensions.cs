using System.Diagnostics.CodeAnalysis;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Client.Extensions;

public static class StationTrainExtensions
{
    public static string TrainIdentity(this StationTrain? me) =>
        me is null ? string.Empty :
        me.ShowOperatorName ? $"{me.OperatorName} {me.TrainPrefix}{me.TrainNumber}" :
        $"{me.TrainPrefix}{me.TrainNumber}";

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

    public static string TrainCategory(this StationTrain? me)
    {
        if (me is null) return string.Empty;
        var rm = Notes.ResourceManager;
        return rm.GetString(me.ProductResourcName) ?? string.Empty;
    }




}
