using System.Diagnostics.CodeAnalysis;

namespace Tellurian.Trains.Planning.App.Contract
{
    public static class StringExtensions
    {
        public static bool HasValue([NotNullWhen(true)] this string? me) => !string.IsNullOrEmpty(me);
    }
}
