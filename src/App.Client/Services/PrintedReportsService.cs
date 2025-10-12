using System.Net;
using System.Net.Http.Headers;
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

    public Task<(HttpStatusCode statusCode, Layout? item)> GetLayoutAsync(int layoutId = 0) =>
        GetItem<Layout>($"api/layouts/{layoutId}/reports/layout");

    public Task<(HttpStatusCode statusCode, IEnumerable<LocoSchedule> items)> GetLocoSchedulesAsync(int layoutId) =>
        GetItems<LocoSchedule>($"api/layouts/{layoutId}/reports/locoschedules");

    public Task<(HttpStatusCode statusCode, IEnumerable<ShuntingLoco> items)> GetShuntingLocosAsync(int layoutId) =>
        GetItems<ShuntingLoco>($"api/layouts/{layoutId}/reports/shuntinglocos");

    public Task<(HttpStatusCode statusCode, StationDutyBooklet? item)> GetStationDutiesAsync(int layoutId, string? countryCode = null) =>
        GetItem<StationDutyBooklet>($"api/layouts/{layoutId}/reports/stationduties?countrycode={countryCode}");

    public Task<(HttpStatusCode statusCode, IEnumerable<StationInstruction> items)> GetStationInstructionsAsync(int layoutId) =>
        GetItems<StationInstruction>($"api/layouts/{layoutId}/reports/stationsinstructions");

    public Task<(HttpStatusCode statusCode, IEnumerable<StationTrainOrder> items)> GetStationsTrainOrderAsync(int layoutId) =>
        GetItems<StationTrainOrder>($"api/layouts/{layoutId}/reports/stationstrainorders");

    public Task<(HttpStatusCode statusCode, IEnumerable<TimetableStretch> items)> GetTimetableStretchesAsync(int layoutId, string? line) =>
        GetItems<TimetableStretch>($"api/layouts/{layoutId}/reports/timetablestretches?line={line}");
    public Task<(HttpStatusCode statusCode, IEnumerable<TimetableStretch> items)> UpdateTrainAndGetTimetableStretchesAsync(int layoutId, int trainId, int minutes, string? line) =>
        GetItems<TimetableStretch>($"api/layouts/{layoutId}/reports/updatetrain?trainId={trainId}&minutes={minutes}");

    public Task<(HttpStatusCode statusCode, IEnumerable<TimetableTrainSection> items)> GetTimetableTrainsAsync(int layoutId) =>
        GetItems<TimetableTrainSection>($"api/layouts/{layoutId}/reports/timetabletrains");

    public Task<(HttpStatusCode statusCode, IEnumerable<Train> items)> GetTrainsAsync(int layoutId, string? operatorName = null) =>
        GetItems<Train>($"api/layouts/{layoutId}/reports/trains?operatorname={operatorName}");

    public Task<(HttpStatusCode statusCode, IEnumerable<TrainComposition> items)> GetTrainCompositionsAsync(int layoutId, string? operatorName = null) =>
       GetItems<TrainComposition>($"api/layouts/{layoutId}/reports/traincompositions?operatorname={operatorName}");
    public Task<(HttpStatusCode statusCode, IEnumerable<TrainsetSchedule> items)> GetTrainsetSchedulesAsync(int layoutId) =>
        GetItems<TrainsetSchedule>($"api/layouts/{layoutId}/reports/trainsetschedules");

    public Task<(HttpStatusCode statusCode, IEnumerable<TrainsetSchedule> items)> GetTrainsetWagonCardsAsync(int layoutId) =>
        GetItems<TrainsetSchedule>($"api/layouts/{layoutId}/reports/trainsetwagoncards");

    public Task<(HttpStatusCode statusCode, IEnumerable<TrainDeparture> items)> GetTrainDeparturesAsync(int layoutId) =>
        GetItems<TrainDeparture>($"api/layouts/{layoutId}/reports/trainstartlabels");

    public Task<(HttpStatusCode statusCode, IEnumerable<TrainCallNote> items)> GetTrainCallNotesAsync(int layoutId) =>
        GetItems<TrainCallNote>($"api/layouts/{layoutId}/reports/traincallnotes");

    public Task<(HttpStatusCode statusCode, IEnumerable<VehicleStartInfo> items)> GetVehicleStartInfosAsync(int layoutId) =>
        GetItems<VehicleStartInfo>($"api/layouts/{layoutId}/reports/vehiclestartinfos");

    public Task<HttpStatusCode> RenumberDuties(int layoutId) =>
        ExecuteCommand($"api/layouts/{layoutId}/reports/renumberduties");

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
        if (response.IsSuccessStatusCode)
            return (response.StatusCode, await Item<T>(response).ConfigureAwait(false));
        return (response.StatusCode, null);
    }

    private async Task<HttpStatusCode> ExecuteCommand(string requestUrl)
    {
        using var request = CreateRequest(requestUrl);
        var response = await Http.SendAsync(request).ConfigureAwait(false);
        return response.StatusCode;
    }

    private static HttpRequestMessage CreateRequest(string requestUri) =>
        new(HttpMethod.Get, requestUri);

    private static async Task<IEnumerable<T>> Items<T>(HttpResponseMessage response) =>
       await response.Content.ReadFromJsonAsync<IEnumerable<T>>(Options).ConfigureAwait(false) ?? Array.Empty<T>();

    private static async Task<T?> Item<T>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<T>(Options).ConfigureAwait(false);
    public Task<HttpStatusCode> RenumberDuties(int layouyId, string? operatorSignature = null) => throw new NotImplementedException();
}
