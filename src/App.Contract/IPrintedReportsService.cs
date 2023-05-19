using System.Net;

namespace Tellurian.Trains.Planning.App.Contracts;

public interface IPrintedReportsService
{
    Task<(HttpStatusCode statusCode, IEnumerable<BlockDestinations> items)> GetBlockDestinationsAsync(int layoutId);
    Task<(HttpStatusCode statusCode, DriverDutyBooklet? item)> GetDriverDutiesAsync(int layoutId);
    Task<(HttpStatusCode statusCode, Layout? item)> GetLayoutAsync(int layoutId);
    Task<(HttpStatusCode statusCode, IEnumerable<LocoSchedule> items)> GetLocoSchedulesAsync(int layoutId);
    Task<(HttpStatusCode statusCode, StationDutyBooklet? item)> GetStationDutiesAsync(int layoutId);
    Task<(HttpStatusCode statusCode, IEnumerable<StationTrainOrder>? items)> GetStationsTrainOrderAsync(int layoutId);
    Task<(HttpStatusCode statusCode, IEnumerable<TimetableStretch> items)> GetTimetableStretchesAsync(int layoutId, string? line);
    Task<(HttpStatusCode statusCode, IEnumerable<TimetableTrainSection> items)> GetTimetableTrainsAsync(int layoutId);
    Task<(HttpStatusCode statusCode, IEnumerable<Train> items)> GetTrainsAsync(int layoutId);
    Task<(HttpStatusCode statusCode, IEnumerable<TrainCallNote> items)> GetTrainCallNotesAsync(int layoutId);
    Task<(HttpStatusCode statusCode, IEnumerable<TrainDeparture> items)> GetTrainDeparturesAsync(int layoutId);
    Task<(HttpStatusCode statusCode, IEnumerable<TrainsetSchedule> items)> GetTrainsetSchedulesAsync(int layoutId);
}