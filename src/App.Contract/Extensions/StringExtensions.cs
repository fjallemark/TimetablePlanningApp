using System.Diagnostics.CodeAnalysis;

namespace Tellurian.Trains.Planning.App.Contracts.Extensions;

public static class StringExtensions
{
    public static bool HasValue([NotNullWhen(true)] this string? me) => !string.IsNullOrWhiteSpace(me);

    public static string OrElse(this string? value, string orElseValue) => string.IsNullOrWhiteSpace(value) ? orElseValue : value;
}
