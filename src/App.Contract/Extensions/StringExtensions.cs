using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Tellurian.Trains.Planning.App.Contracts.Extensions;

public static partial class StringExtensions
{
    public static bool HasValue([NotNullWhen(true)] this string? me) => !string.IsNullOrWhiteSpace(me);
    public static bool IsEmpty([NotNullWhen(false)] this string? me) => string.IsNullOrWhiteSpace(me);

    public static string OrElse(this string? value, string orElseValue) => string.IsNullOrWhiteSpace(value) ? orElseValue : value;

    public static bool AnyOf(this string? value, string[] values) => 
        value is null || values.Length == 0 ? false :
        values.Contains(value);

    public static string WithHtmlRemoved(this string? withHtml) =>
        withHtml is null ? string.Empty :
        HtmlRemove().Replace(withHtml, string.Empty).Replace("&nbsp", " ").Replace(';', ' ');

    [GeneratedRegex("<.*?>")]
    private static partial Regex HtmlRemove();
}
