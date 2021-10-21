using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client.Services
{
    internal static class TrackDestinationExtensions
    {
        public static string FlexDirection(this TrainBlocking me, bool reverseItAgain = false) =>
            me.ReverseBlockOrder(reverseItAgain) ? "row-reverse" : "row";

        private static bool ReverseBlockOrder(this TrainBlocking me, bool reverseItAgain = false) =>
            me.Train != null && (me.Train.Number % 2 == 0 ? !me.IsBlockOrderReversed(reverseItAgain) :
            me.IsBlockOrderReversed(reverseItAgain));

        private static bool IsBlockOrderReversed(this TrainBlocking me, bool reverseItAgain = false) =>
            reverseItAgain ? !me.ReverseBlockDestinations : me.ReverseBlockDestinations;

        public static string Display(this TrainInfo me) =>
            $"{me.Prefix} {me.Number}";
    }
}
