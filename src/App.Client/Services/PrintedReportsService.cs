using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client.Services
{
    public class PrintedReportsService : IPrintedReportsService
    {
        public PrintedReportsService(HttpClient http)
        {
            Http = http;
        }
        private readonly HttpClient Http;
        private static JsonSerializerOptions Options => new()
        {
            PropertyNameCaseInsensitive = true,
            IgnoreReadOnlyProperties = true
        };

        public Task<(HttpStatusCode statusCode, IEnumerable<Waybill> items)> GetWaybillsAsync(int layoutId) =>
            GetItems<Waybill>($"api/layouts/{layoutId}/reports/waybills");

        public Task<(HttpStatusCode statusCode, IEnumerable<LocoSchedule> items)> GetLocoSchedulesAsync(int layoutId) =>
            GetItems<LocoSchedule>($"api/layouts/{layoutId}/reports/locoschedules");

        public Task<(HttpStatusCode statusCode, IEnumerable<TrainsetSchedule> items)> GetTrainsetSchedulesAsync(int layoutId) =>
             GetItems<TrainsetSchedule>($"api/layouts/{layoutId}/reports/trainsetschedules");

        public Task<(HttpStatusCode statusCode, DriverDutyBooklet? item)> GetDriverDutiesAsync(int layoutId) =>
             GetItem<DriverDutyBooklet>($"api/layouts/{layoutId}/reports/driverduties");

        public Task<(HttpStatusCode statusCode, IEnumerable<TrainCallNote> items)> GetTrainCallNotesAsync(int layoutId) =>
             GetItems<TrainCallNote>($"api/layouts/{layoutId}/reports/traincallnotes");

        public Task<(HttpStatusCode statusCode, IEnumerable<BlockDestinations> items)> GetBlockDestinations(int layoutId) =>
              GetItems<BlockDestinations>($"api/layouts/{layoutId}/reports/blockdestinations");

        public Task<(HttpStatusCode statusCode, IEnumerable<TimetableStretch> items)> GetTimetableStretches(int layoutId) =>
            GetItems<TimetableStretch>($"api/layouts/{layoutId}/reports/timetablestretches");
        public Task<(HttpStatusCode statusCode, IEnumerable<TimetableTrainSection> items)> GetTimetableTrains(int layoutId) =>
            GetItems<TimetableTrainSection>($"api/layouts/{layoutId}/reports/timetabletrains");
        public Task<(HttpStatusCode statusCode, IEnumerable<TrainDeparture> items)> GetTrainDepartures(int layoutId) =>
            GetItems<TrainDeparture>($"api/layouts/{layoutId}/reports/traininitialdepartures");
        public Task<(HttpStatusCode statusCode, IEnumerable<StationInstruction> items)> GetStationInstructions(int layoutId) =>
            GetItems<StationInstruction>($"api/layouts/{layoutId}/reports/stationinstructions");

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
}
