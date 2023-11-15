using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;
internal static class StationTrainOrderMapper
{
    public static StationTrainOrder ToStationTrainOrder(this IDataRecord me) =>
        new()
        {
            Designation = me.GetString("Signature"),
            Name = me.GetString("FullName"),
            Trains = new List<StationTrain>(100),
        };

    public static StationTrain ToStationTrain(this IDataRecord me, IEnumerable<TrainCategory> trainCategories) =>
        new()
        {
            CallId = me.GetInt("CallId"),
            ArrivalTime = me.GetString("ArrivalTime"),
            DepartureTime = me.GetString("DepartureTime"),
            DestinationName = me.GetString("Destination"),
            OperatingDayFlag = me.GetByte("OperatingDayFlag"),
            OperatorName = me.GetString("Operator"),
            OriginName = me.GetString("Origin"),
            ProductResourcName = me.GetString("ProductResourceCode"),
            SortTime = me.GetTimeAsDouble("SortTime"),
            TrackNumber = me.GetString("Designation"),
            TrainPrefix = trainCategories.SingleOrDefault(c => c.ResourceCode == me.GetString("ProductResourceCode"))?.Prefix,
            TrainNumber = me.GetInt("Number"),
            IsStop = me.GetBool("IsStop"),
            ShowOperatorName = me.GetBool("ShowOperatorName"),
        };
}
