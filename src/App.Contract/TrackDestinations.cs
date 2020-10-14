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
        public IList<TrackTrains> Tracks { get; set; } = new List<TrackTrains>();
    }

    public class TrackTrains
    {
        public string TrackNumber { get; set; } = string.Empty;
        public int TrackDisplayOrder { get; set; }
        public bool ReverseBlockDestinations { get; set; }
        public IList<TrainBlocking> TrainBlocks { get; set; } = new List<TrainBlocking>();
        public override string ToString() => $"{Notes.Track} {TrackNumber}";
    }

    public class TrainBlocking
    {
        public TrainInfo Train { get; set; } = new TrainInfo();
        public CallTime DepartureTime { get; set; } = new CallTime();
        public bool ReverseBlockDestinations { get; set; }
        public IList<BlockDestination> BlockDestinations { get; set; } = new List<BlockDestination>();
    }

    public static class TrackDestinationExtensions
    {
        public static string[] Destinations(this TrainBlocking me) =>
            me is null ? Array.Empty<string>() :
            me.BlockDestinations.Select(Display).ToArray();

        public static string Display(this BlockDestination me) =>
                me is null ? string.Empty :
                string.IsNullOrWhiteSpace(me.TransferDestination) ? me.StationName :
                me.TransferDestination;
    }
}
