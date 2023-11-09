using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;

internal static class StationMapper
{
    public static Station ToStation(this IDataRecord me) =>
        new()
        {
            Id = me.GetInt("StationId"),
            Name = me.GetString("StationName"),
            Signature = me.GetString("StationSignature"),
            Tracks = new List<StationTrack>()
        };

    public static StationTrack ToStationStrack(this IDataRecord me) =>
        new()
        {
            Id = me.GetInt("StationTrackId"),
            Number = me.GetString("TrackNumber"),
            DisplayOrder = me.GetInt("TrackDisplayOrder"),
            IsScheduledTrack = me.GetBool("IsScheduledTrack"),
            HasPlatform = me.GetBool("HasPlatform")
        };
}
