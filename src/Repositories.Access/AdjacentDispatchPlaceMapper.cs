using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;
internal static class AdjacentDispatchPlaceMapper
{
    public static AdjacentDispatchPlace ToAdjacentDispatchPlace(this IDataRecord record) =>
        new()
        {
            Name = record.GetString("FullName"),
            LayoutStationId = record.GetInt("LayoutStationId"),
            PhoneNote = record.GetString("PhoneNumberNote"),
            PhoneNumber = record.GetString("PhoneNumber"),
        };
}
