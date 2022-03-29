using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Tellurian.Trains.Planning.App.Client.Services;

public static class LocalizedStringExtensions
{
    public static string ToLower(this LocalizedString me) => me.ToString().ToLower(CultureInfo.CurrentCulture);
}
