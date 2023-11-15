namespace Tellurian.Trains.Planning.App.Client.Extensions;

public static class TimeExtensions
{
    public static string AsTime(this double minutes) => $"{Math.Round(minutes / 60, 0, MidpointRounding.ToZero):00}:{Math.Round(minutes % 60, 0, MidpointRounding.ToZero):00}";

}
