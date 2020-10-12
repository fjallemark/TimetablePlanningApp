using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Emit;
using System.Text.Json;
using System.Threading.Tasks;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.App.Client.Services
{
    public class PrintedReportsService
    {
        public PrintedReportsService(HttpClient http)
        {
            Http = http;
        }
        private readonly HttpClient Http;
        private static JsonSerializerOptions Options => new JsonSerializerOptions
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
            new HttpRequestMessage(HttpMethod.Get, requestUri);

        private static async Task<IEnumerable<T>> Items<T>(HttpResponseMessage response) =>
           await response.Content.ReadFromJsonAsync<IEnumerable<T>>(Options).ConfigureAwait(false);

        private static async Task<T> Item<T>(HttpResponseMessage response) =>
            await response.Content.ReadFromJsonAsync<T>(Options).ConfigureAwait(false);
    }
}
