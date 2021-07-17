using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client.Services
{
    internal static class TrackDestinationExtensions
    {
        public static string FlexDirection(this TrainBlocking me) =>
            me.ReverseBlockOrder() ? "row-reverse" : "row";

        public static bool ReverseBlockOrder(this TrainBlocking me) =>
            me.Train != null && (me.Train.Number % 2 == 0 ? !me.ReverseBlockDestinations :
            me.ReverseBlockDestinations);

        public static string Display(this TrainInfo me) =>
            $"{me.Prefix} {me.Number}";
    }
}
