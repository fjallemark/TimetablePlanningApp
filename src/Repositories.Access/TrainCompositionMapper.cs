using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;
internal static class TrainCompositionMapper
{
    public static TrainComposition ToTrainComposition(this IDataRecord record) =>
        new()
        {
            OperatorName = record.GetString("TrainOperator"),
            TrainNumber = record.GetInt("TrainNumber"),
            TrainCategoryResourceCode = record.GetString("TrainCategoryResourceCode"),
            OperationDaysFlags = record.GetByte("TrainOperationDaysFlags"),
        };

    public static TrainsetInTrainComposition ToTrainsetInTrainComposition(this IDataRecord record) =>
        new() { 
            OperatorName = record.GetString("TrainsetOperator"),
            Number = record.GetInt("TrainsetNumber"),
            OperationDaysFlags = record.GetByte("TrainsetOperationDaysFlags"),
            Class = record.GetString("Class"),
            OrderInTrain = record.GetInt("OrderInTrain"),
            MaxNumberOfUnits = record.GetInt("MaxNumberOfWagons"),
            DepartureStationName = record.GetString("DepartureStationName"),
            DepartureTime = record.GetTimeAsTimespan("DepartureTime"),
            ArrivalStationName = record.GetString("ArrivalStationName"),
            ArrivalTime = record.GetTimeAsTimespan("ArrivalTime"),
            Note = record.GetString("Note"),
            FinalStationName = record.GetString("FinalStationName"),
        };
}
