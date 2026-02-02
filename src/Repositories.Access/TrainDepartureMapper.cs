using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;

internal static class TrainDepartureMapper
{
    public static TrainDeparture ToTrainDeparture(this IDataRecord me) =>
        new()
        {
            DepartureTime = new CallTime { IsStop = true, Time = me.GetTime("DepartureTime") },
            CountryCode = me.GetString("CountryCode"),
            Loco = new TrainLoco
            {
                Class = me.GetString("LocoClass"),
                IsRailcar = me.GetBool("IsRailcar"),
                TurnusNumber = me.GetInt("LocoNumber"),
                OperationDaysFlags = me.GetByte("LocoOperationDaysFlag"),
                OperatorName = me.GetString("LocoOperatorName")
            },
            StationName = me.GetString("StationName"),
            TrackNumber = me.GetString("TrackNumber"),
            Train = new TrainInfo
            {
                CategoryId = me.GetInt("CategoryId"),
                IsCargo = me.GetBool("IsCargo"),
                IsPassenger = me.GetBool("IsPassenger"),
                Number = me.GetInt("TrainNumber"),
                OperationDaysFlags = me.GetByte("TrainOperationDaysFlag"),
                OperatorName = me.GetString("TrainOperatorName"),
                OverriddenDestination = me.GetString("TrainDestination", null),
            }
        };
}
