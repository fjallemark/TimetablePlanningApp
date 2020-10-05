using System;
using System.Collections.Generic;
using System.Linq;
using Tellurian.Trains.Planning.App.Contract.Resources;

#pragma warning disable CA2227 // Collection properties should be read only

namespace Tellurian.Trains.Planning.App.Contract
{
    public class BlockDestinations
    {
        public string OriginStationName { get; set; } = string.Empty;
        public IList<TrackDestination> TrackDestinations { get; set;  } = new List<TrackDestination>();
    }

    public class TrackDestination
    {
        public string TrackNumber { get; set; } = string.Empty;
        public int TrackDisplayOrder { get; set; }
        public IList<BlockDestination> BlockDestinations { get; set;  } = new List<BlockDestination>();
        public override string ToString() => $"{Notes.Track} {TrackNumber}";
    }

    public static class TrackDestinationExtensions
    {
        public static string[] Destinations(this TrackDestination me) =>
            me is null ? Array.Empty<string>() :
            me.BlockDestinations.Select(Display).ToArray();

        public static string Display(this BlockDestination me) =>
                me is null ? string.Empty :
                string.IsNullOrWhiteSpace(me.TransferDestination) ? me.StationName :
                me.TransferDestination;
    }
}
