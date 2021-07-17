using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Web;

namespace Tellurian.Trains.Planning.App.Client.Services
{
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
}
