using System.Collections.Specialized;
using Microsoft.AspNetCore.Components;
using System.Web;

namespace Tellurian.Trains.Planning.App.Client.Extensions;

public static class NavigationManagerExtensions
{
    public static NameValueCollection QueryString(this NavigationManager navigationManager)
    {
        return HttpUtility.ParseQueryString(new Uri(navigationManager.Uri.ToLowerInvariant()).Query);
    }

    // get single querystring value with specified key
    public static string? QueryString(this NavigationManager navigationManager, string key)
    {
        return navigationManager.QueryString()[key.ToLowerInvariant()];
    }
}
