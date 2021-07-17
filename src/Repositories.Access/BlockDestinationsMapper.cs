using System.Collections.Generic;
using System.Data;
using System.Resources;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    internal static class BlockDestinationsMapper
    {
        private static ResourceManager Notes => App.Contracts.Resources.Notes.ResourceManager;
        public static BlockDestinations AsBlockDestinations(this IDataRecord me) =>
            new()
            {
                OriginStationName = me.GetString("OriginStationName")
            };

        public static TrackTrains AsTrackDestination(this IDataRecord me) =>
            new()
            {
                TrackNumber = me.GetString("TrackNumber"),
                TrackDisplayOrder = me.GetInt("TrackDisplayOrder")
            };

        public static TrainBlocking AsTrainBlocking(this IDataRecord me) =>
            new()
            {
                DepartureTime = new CallTime { IsStop = true, Time = me.GetTime("DepartureTime") },
                Train = new TrainInfo
                {
                    CategoryName = me.GetStringResource("TrainCategoryName", Notes),
                    IsCargo = me.GetBool("IsCargo"),
                    IsPassenger = me.GetBool("IsPassenger"),
                    Number = me.GetInt("TrainNumber"),
                    OperatorName = me.GetString("TrainOperatorName"),
                    OperationDaysFlags = me.GetByte("TrainOperationDaysFlag"),
                    Prefix = me.GetString("TrainCategoryPrefix")
                },
                BlockDestinations = new List<BlockDestination>(),
                ReverseBlockDestinations = me.GetBool("ReverseBlockDestinations")
            };
    }
}
