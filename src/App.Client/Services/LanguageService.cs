using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.App.Client.Services
{
    internal class LanguageService
    {
        public LanguageService(HttpClient http)
        {
            Http = http;
        }
        private readonly HttpClient Http;
        public async Task<IEnumerable<LanguageLabels>> GetWaybillLabels() =>
            await Http.GetFromJsonAsync<IEnumerable<LanguageLabels>>("api/languages/all/labels/waybills").ConfigureAwait(false);
    }
}
