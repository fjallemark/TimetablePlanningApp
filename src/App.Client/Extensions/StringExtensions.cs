namespace Tellurian.Trains.Planning.App.Client.Extensions;

public static class StringExtensions
{
    
    public static bool EqualsAny(this string value, string? commaSeparatedValues) =>
        commaSeparatedValues is null ? true :
        commaSeparatedValues.Split(',').Any(v => v.Trim().Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
}
