using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;
internal static class ShuntingLocoMapper
{

    public static ShuntingLoco ToShuntingLoco(this IDataRecord record) =>
        new()
        {
            HomeStationName = record.GetString("HomeStationName"),
            OperatorName = record.GetString("LocoOperator", ""),
            Class = record.GetString("LocoClass"),
            VehicleNumber = record.GetString("LocoNumber"),
            OwnerName = record.GetString("OwnerName"),
            Note = record.GetString("Note",""),
            IsRailcar = record.GetBool("IsRailCar"),
            OperationDaysFlags = OperationDays.AllDays,
        };
}
