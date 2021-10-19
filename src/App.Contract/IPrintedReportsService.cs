using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Contracts
{
    public interface IPrintedReportsService
    {
        Task<(HttpStatusCode statusCode, IEnumerable<BlockDestinations> items)> GetBlockDestinations(int layoutId);
        Task<(HttpStatusCode statusCode, DriverDutyBooklet? item)> GetDriverDutiesAsync(int layoutId);
        Task<(HttpStatusCode statusCode, IEnumerable<LocoSchedule> items)> GetLocoSchedulesAsync(int layoutId);
        Task<(HttpStatusCode statusCode, IEnumerable<StationInstruction> items)> GetStationInstructions(int layoutId);
        Task<(HttpStatusCode statusCode, IEnumerable<TimetableStretch> items)> GetTimetableStretches(int layoutId);
        Task<(HttpStatusCode statusCode, IEnumerable<TimetableTrainSection> items)> GetTimetableTrains(int layoutId);
        Task<(HttpStatusCode statusCode, IEnumerable<TrainCallNote> items)> GetTrainCallNotesAsync(int layoutId);
        Task<(HttpStatusCode statusCode, IEnumerable<TrainDeparture> items)> GetTrainDepartures(int layoutId);
        Task<(HttpStatusCode statusCode, IEnumerable<TrainsetSchedule> items)> GetTrainsetSchedulesAsync(int layoutId);
        Task<(HttpStatusCode statusCode, IEnumerable<Waybill> items)> GetWaybillsAsync(int layoutId);
    }
}