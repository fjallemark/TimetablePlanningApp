using System.Data;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    internal static class BlockDestinationsMapper
    {
        public static BlockDestinations AsBlockDestinations(this IDataRecord me) =>
            new BlockDestinations
            {
                OriginStationName = me.GetString("OriginStationName")
            };

        public static TrackDestination AsTrackDestination(this IDataRecord me) =>
            new TrackDestination
            {
                 TrackNumber = me.GetString("TrackNumber"),
                 TrackDisplayOrder = me.GetInt16("TrackDisplayOrder")
            };
    }
}
