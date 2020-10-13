using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.App.Client.Services
{
    internal static class TrackDestinationExtensions
    {
        public static string FlexDirection(this TrackDestination me) =>
            me.TrainsDepartsToLeft ? "row-reverse" : "row";
    }
}
