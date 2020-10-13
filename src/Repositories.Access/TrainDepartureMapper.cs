using System.Data;
using System.Resources;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    internal static class TrainDepartureMapper
    {
        private static ResourceManager Notes => App.Contract.Resources.Notes.ResourceManager;

        public static TrainDeparture AsTrainDeparture(this IDataRecord me) =>
            new TrainDeparture
            {
                DepartureTime = new CallTime { IsStop = true, Time = me.GetTime("DepartureTime") },
                Loco = new Loco
                {
                    Class = me.GetString("LocoClass"),
                    IsRailcar = me.GetBool("IsRailcar"),
                    Number = me.GetInt("LocoNumber"),
                    OperationDays = me.GetByte("LocoOperationDaysFlag").OperationDays(),
                    OperatorName = me.GetString("LocoOperatorName")
                },
                StationName = me.GetString("StationName"),
                TrackNumber = me.GetString("TrackNumber"),
                Train = new TrainInfo
                {
                    CategoryName = me.GetStringResource("TrainCategoryName", Notes),
                    IsCargo = me.GetBool("IsCargo"),
                    IsPassenger = me.GetBool("IsPassenger"),
                    Number = me.GetInt("TrainNumber"),
                    OperationDays = me.GetByte("TrainOperationDaysFlag").OperationDays(),
                    OperatorName = me.GetString("TrainOperatorName"),
                    Prefix = me.GetString("TrainCategoryPrefix")
                }
            };
    }
}
