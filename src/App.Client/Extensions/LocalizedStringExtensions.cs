using Microsoft.Extensions.Localization;
using System.Globalization;
using Tellurian.Trains.Planning.App.Contracts.Extensions;

namespace Tellurian.Trains.Planning.App.Client.Extensions;

public static class LocalizedStringExtensions
{
    public static string ToLower(this LocalizedString me) => me.ToString().ToLower(CultureInfo.CurrentCulture);
}
