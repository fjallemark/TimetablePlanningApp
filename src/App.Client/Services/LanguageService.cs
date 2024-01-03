using System.Net.Http.Json;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client.Services;

internal class LanguageService(HttpClient http)
{
    private readonly HttpClient Http = http;

    public async Task<IEnumerable<LanguageLabels>> GetWaybillLabels() =>
        await Http.GetFromJsonAsync<IEnumerable<LanguageLabels>>("api/languages/all/labels/waybills").ConfigureAwait(false) ?? Array.Empty<LanguageLabels>();
}
