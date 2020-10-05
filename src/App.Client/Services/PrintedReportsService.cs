using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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
            Get<Waybill>($"api/layouts/{layoutId}/reports/waybills");

        public Task<(HttpStatusCode statusCode, IEnumerable<LocoSchedule> items)> GetLocoSchedulesAsync(int layoutId)=>
            Get<LocoSchedule>($"api/layouts/{layoutId}/reports/locoschedules");

        public Task<(HttpStatusCode statusCode, IEnumerable<TrainsetSchedule> items)> GetTrainsetSchedulesAsync(int layoutId) =>
             Get<TrainsetSchedule>($"api/layouts/{layoutId}/reports/trainsetschedules");

        public Task<(HttpStatusCode statusCode, IEnumerable<DriverDuty> items)> GetDriverDutiesAsync(int layoutId) =>
             Get<DriverDuty>($"api/layouts/{layoutId}/reports/driverduties");

        public Task<(HttpStatusCode statusCode, IEnumerable<TrainCallNote> items)> GetTrainCallNotesAsync(int layoutId) =>
             Get<TrainCallNote>($"api/layouts/{layoutId}/reports/traincallnotes");

        public Task<(HttpStatusCode statusCode, IEnumerable<BlockDestinations> items)> GetBlockDestinations(int layoutId) =>
              Get<BlockDestinations>($"api/layouts/{layoutId}/reports/blockdestinations");

        private async Task<(HttpStatusCode statusCode, IEnumerable<T> items)> Get<T>(string requestUrl)
        {
            using var request = CreateRequest(requestUrl);
            var response = await Http.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return (response.StatusCode, await Content<T>(response).ConfigureAwait(false));
            return (response.StatusCode, Array.Empty<T>());
        }

        private static HttpRequestMessage CreateRequest(string requestUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            //request.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);
            return request;
        }

        private static async Task<IEnumerable<T>> Content<T>(HttpResponseMessage response) =>
           await response.Content.ReadFromJsonAsync<IEnumerable<T>>(Options).ConfigureAwait(false);
    }
}
