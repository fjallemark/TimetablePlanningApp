namespace Tellurian.Trains.Planning.App.Shared
{
    public static class StringExtensions
    {
        public static bool HasValue(this string? me) => !string.IsNullOrEmpty(me);
    }
}
