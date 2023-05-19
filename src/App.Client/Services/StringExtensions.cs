namespace Tellurian.Trains.Planning.App.Client.Services;

public static class StringExtensions
{
    public static bool EqualsAny(this string value, string commaSeparatedValues) =>
        commaSeparatedValues.Split(',').Any(v => v.Trim().Equals(value.Trim(), System.StringComparison.OrdinalIgnoreCase));
}
