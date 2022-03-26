using System.Diagnostics.CodeAnalysis;

namespace Tellurian.Trains.Planning.App.Contracts
{
    public static class StringExtensions
    {
        public static bool HasValue([NotNullWhen(true)] this string? me) => !string.IsNullOrWhiteSpace(me);
    }
}
