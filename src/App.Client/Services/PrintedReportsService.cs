using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client.Services;

public class PrintedReportsService(HttpClient http) : IPrintedReportsService
{
    private readonly HttpClient Http = http;
    private static JsonSerializerOptions Options => new()
    {
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyProperties = true
    };
    public Task<(HttpStatusCode statusCode, IEnumerable<BlockDestinations> items)> GetBlockDestinationsAsync(int layoutId) =>
          GetItems<BlockDestinations>($"api/layouts/{layoutId}/reports/blockdestinations");

    public Task<(HttpStatusCode statusCode, DriverDutyBooklet? item)> GetDriverDutiesAsync(int layoutId) =>
         GetItem<DriverDutyBooklet>($"api/layouts/{layoutId}/reports/driverduties");

    public Task<(HttpStatusCode statusCode, Layout? item)> GetLayoutAsync(int layoutId) =>
        GetItem<Layout>($"api/layouts/{layoutId}/reports/layout");

    public Task<(HttpStatusCode statusCode, IEnumerable<LocoSchedule> items)> GetLocoSchedulesAsync(int layoutId) =>
       GetItems<LocoSchedule>($"api/layouts/{layoutId}/reports/locoschedules");

    public Task<(HttpStatusCode statusCode, StationDutyBooklet? item)> GetStationDutiesAsync(int layoutId) =>
          GetItem<StationDutyBooklet>($"api/layouts/{layoutId}/reports/stationduties");
    public Task<(HttpStatusCode statusCode, IEnumerable<StationTrainOrder>? items)> GetStationsTrainOrderAsync(int layoutId) =>
           GetItem<IEnumerable<StationTrainOrder>>($"api/layouts/{layoutId}/reports/stationstrainorder");

    public Task<(HttpStatusCode statusCode, IEnumerable<TimetableStretch> items)> GetTimetableStretchesAsync(int layoutId, string? line) =>
        GetItems<TimetableStretch>($"api/layouts/{layoutId}/reports/timetablestretches?line={line}");

    public Task<(HttpStatusCode statusCode, IEnumerable<TimetableTrainSection> items)> GetTimetableTrainsAsync(int layoutId) =>
        GetItems<TimetableTrainSection>($"api/layouts/{layoutId}/reports/timetabletrains");

    public Task<(HttpStatusCode statusCode, IEnumerable<Train> items)> GetTrainsAsync(int layoutId, string? operatorSignature = null) =>
        GetItems<Train>($"api/layouts/{layoutId}/reports/trains?operator={operatorSignature}");

    public Task<(HttpStatusCode statusCode, IEnumerable<TrainsetSchedule> items)> GetTrainsetSchedulesAsync(int layoutId) =>
         GetItems<TrainsetSchedule>($"api/layouts/{layoutId}/reports/trainsetschedules");
   public Task<(HttpStatusCode statusCode, IEnumerable<TrainDeparture> items)> GetTrainDeparturesAsync(int layoutId) =>
        GetItems<TrainDeparture>($"api/layouts/{layoutId}/reports/trainstartlabels");

    public Task<(HttpStatusCode statusCode, IEnumerable<TrainCallNote> items)> GetTrainCallNotesAsync(int layoutId) =>
         GetItems<TrainCallNote>($"api/layouts/{layoutId}/reports/traincallnotes");

     private async Task<(HttpStatusCode statusCode, IEnumerable<T> items)> GetItems<T>(string requestUrl)
    {
        using var request = CreateRequest(requestUrl);
        var response = await Http.SendAsync(request).ConfigureAwait(false);
        if (response.IsSuccessStatusCode) return (response.StatusCode, await Items<T>(response).ConfigureAwait(false));
        return (response.StatusCode, Array.Empty<T>());
    }

    private async Task<(HttpStatusCode statusCode, T? item)> GetItem<T>(string requestUrl) where T : class
    {
        using var request = CreateRequest(requestUrl);
        var response = await Http.SendAsync(request).ConfigureAwait(false);
        if (response.IsSuccessStatusCode) return (response.StatusCode, await Item<T>(response).ConfigureAwait(false));
        return (response.StatusCode, null);
    }

    private static HttpRequestMessage CreateRequest(string requestUri) =>
        new(HttpMethod.Get, requestUri);

    private static async Task<IEnumerable<T>> Items<T>(HttpResponseMessage response) =>
       await response.Content.ReadFromJsonAsync<IEnumerable<T>>(Options).ConfigureAwait(false) ?? Array.Empty<T>();

    private static async Task<T?> Item<T>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<T>(Options).ConfigureAwait(false);
}
