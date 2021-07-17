using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client.Services
{
    public static class WaybillExtensions
    {
        public static string FlagSrc(this Waybill me) =>
            me.Destination is null ? string.Empty :
            $"images/flags/{me.Destination.Language}.png";
    }
}
