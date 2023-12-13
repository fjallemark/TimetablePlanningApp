using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Contracts.Extensions;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Client.Extensions;

public static class StationTrainExtensions
{
    public static string TrainIdentity(this StationTrain? me) =>
        me is null ? string.Empty :
        me.ShowOperatorName ? $"{me.OperatorName} {me.TrainPrefix}{me.TrainNumber}" :
        $"{me.TrainPrefix}{me.TrainNumber}";

    public static string Origin(this StationTrain me) =>
         me.HideArrival  ? string.Empty : me.OriginName;
    public static string Destination(this StationTrain me) =>
        me.HideDeparture || me.DepartureTime.IsEmpty() ? string.Empty : me.DestinationName;

    public static string DisplayedArrivalTime(this StationTrain me) =>
        me is null || me.HideArrival ? string.Empty : 
        me.IsStop  || me.ArrivalTime.HasValue() ? me.ArrivalTime : 
        me.DepartureTime;

    public static string DisplayedDepartureTime(this StationTrain me) =>
        me.HideDeparture ? string.Empty : me.DepartureTime;

    public static string RowStyle(this StationTrain me) =>
        me.IsDeparture() ? "background-color: #ffffee" : "";

    public static string OriginCssClass(this StationTrain me) =>
        me.IsNotArrival()  ? string.Empty : "bold";

    public static string DestinationCssClass(this StationTrain me) =>
        me.IsNotDeparture() && me.ArrivalTime.HasValue() ? string.Empty : "bold";

    public static bool IsNotDeparture( this StationTrain me) =>
        me.HideDeparture || me.DepartureTime != me.SortTime.AsTime() || me.DepartureTime.IsEmpty();

    public static bool IsNotArrival(this StationTrain me) =>
        me.HideArrival || me.ArrivalTime != me.SortTime.AsTime() || me.ArrivalTime.IsEmpty();

    public static bool IsArrival(this StationTrain me) =>
        !me.HideArrival && me.ArrivalTime == me.SortTime.AsTime();
    public static bool IsDeparture(this StationTrain me) =>
        !me.HideDeparture && me.DepartureTime == me.SortTime.AsTime();

    public static string TrainCategory(this StationTrain? me)
    {
        if (me is null) return string.Empty;
        var rm = Notes.ResourceManager;
        return rm.GetString(me.ProductResourcName) ?? string.Empty;
    }




}
